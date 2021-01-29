using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class FileHeaderAtom:SimpleAtom
    {
        private UInt32 myBrand = 0;

        public UInt32 Brand
        {
            get { return myBrand; }
            set { myBrand = value; }
        }

        public string BrandString
        {
            get { return Utilities.UInt32ToChars(myBrand); }

            set
            {
                myBrand = Utilities.CharsToUInt32(value);
            }
        }

        List<UInt32> myBrandTypes = new List<UInt32>();



        public FileHeaderAtom()
        {
            SimpleSize = 32;
            TypeString = "ftyp";
            BrandString = "M4B ";
        }

        public void AddBrandType(string brandtype)
        {
            if (String.IsNullOrEmpty(brandtype))
                return;

            if (brandtype.Length != 4)
                return;

            UInt32 brandnum = Utilities.CharsToUInt32(brandtype);
            myBrandTypes.Add(brandnum);
        }

        public void ReplaceBrandString(string original, string replacement)
        {
            if (original.Length != 4)
                return;
            if (replacement.Length != 4)
                return;

            if (BrandString == original)
                BrandString = replacement;

            UInt32 originnum = Utilities.CharsToUInt32(original);
            UInt32 replacenum = Utilities.CharsToUInt32(replacement);
            for (int i = 0; i < myBrandTypes.Count; i++)
            {
                if (myBrandTypes[i] == originnum)
                    myBrandTypes[i] = replacenum;
            }
        }

        public FileHeaderAtom(FileStream fileReader, long seekpos)
        {
            try
            {
                ReadData(fileReader, ref seekpos);
                myDataOK = true;
            }
            catch 
            {
                myDataOK = false;
            }
        }

        protected override void ReadData(FileStream fileReader, ref long seekpos)
        {
            base.ReadData(fileReader, ref seekpos);
            Brand = Utilities.GetFourBytes(fileReader, ref seekpos);

            //have to figure out how many brand types are listed from atom size less fields already read
            int listSize = (int)((SimpleSize - 12)/4);

            for (int i = 0; i < listSize; i++)
            {
                myBrandTypes.Add(Utilities.GetFourBytes(fileReader, ref seekpos));
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);

            Utilities.WriteFourBytes(Brand, fileWriter, ref seekpos);

            foreach(UInt32 brandtype in myBrandTypes)
            {
                Utilities.WriteFourBytes(brandtype, fileWriter, ref seekpos);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();

            long size = basesize + 4 + (myBrandTypes.Count * 4);
            return size;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("File Header Atom|");
            sb.Append("Brand: " + BrandString + "|");
            sb.Append("Brandtypes: ");
            foreach (UInt32 brandtype in myBrandTypes)
            {
                sb.Append(Utilities.UInt32ToChars(brandtype) + " ");
            }
            sb.Append("|");
            return sb.ToString();
        }

    }
}

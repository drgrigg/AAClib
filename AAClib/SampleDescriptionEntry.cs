using System;
using System.IO;

namespace AAClib
{
    //this isn't really an atom, but close enough that it's useful to inherit from it.
    public class SampleDescriptionEntry:SimpleAtom
    {
        //just map the atom data to our data
        public UInt32 Size
        {
            get { return SimpleSize; }
            set { SimpleSize = value; }
        }

        //just map the atom data to our data
        public UInt32 Format
        {
            get { return TypeNum; }
            set { TypeNum = value; }
        }

        public string FormatString
        {
            get { return TypeString; }
            set { TypeString = value; }
        }

        public UInt16 Reserved1 = 0; //note: total of 6 bytes
        public UInt32 Reserved2 = 0; //

        public UInt16 DataReference = 0;
//        public byte[] Remainder = null;



        public SampleDescriptionEntry()
        {
        }

        public SampleDescriptionEntry(FileStream fileReader, ref long seekpos)
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

        public override string ToString()
        {
            return base.ToString() + "Size: " + Size.ToString("#,##0") + ", Format:" + Utilities.UInt32ToChars(Format) + ",Reserved1:" + Reserved1.ToString() + ",Reserved2:" + Reserved2.ToString() + ", DataReference:" + DataReference.ToString() + "|";
        }

        protected override void ReadData(FileStream fileReader, ref long seekpos)
        {
            base.ReadData(fileReader, ref seekpos);
            Reserved1 = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Reserved2 = Utilities.GetFourBytes(fileReader, ref seekpos);
            DataReference = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mySize = CalculateSize();
            SetActualSize(mySize);
            base.WriteData(fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Reserved1, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(Reserved2, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(DataReference,fileWriter, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            return base.CalculateSize() + 2 + 4 + 2;
        }




    }
}

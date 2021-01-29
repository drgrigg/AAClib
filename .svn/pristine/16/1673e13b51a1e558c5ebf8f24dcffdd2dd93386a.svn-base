using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class DataReferenceEntry : SimpleAtom //alis, rsrc or url_
    {
        private byte myVersion = 0;

        public byte Version
        {
            get { return myVersion; }
            set { myVersion = value; }
        }
        private UInt32 myFlags = 1; //note: 3 bytes only

        public UInt32 Flags
        {
            get { return myFlags; }
            set { myFlags = value; }
        }

        private byte[] myData = new byte[0]; //this should be overwritten.

        public byte[] Data
        {
            get { return myData; }
            set { myData = value; }
        }

        public DataReferenceEntry(string typestring)
        {
            TypeString = typestring;
            for (int i = 0; i < myData.Length; i++)
            {
                myData[i] = 0;  
            }
        }

        public DataReferenceEntry(FileStream fileReader, ref long seekpos) //note the 'ref' here.
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
            Version = Utilities.GetOneByte(fileReader, ref seekpos);
            Flags = Utilities.GetThreeBytes(fileReader, ref seekpos);
            int datalen = (int)SimpleSize - 12;
            myData = new byte[datalen];
            Utilities.GetBuffer(fileReader, ref seekpos, datalen, ref myData);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);
            Utilities.WriteOneByte(Version, fileWriter, ref seekpos);
            Utilities.WriteThreeBytes(Flags, fileWriter, ref seekpos);
            int datalen = myData.Length;
            Utilities.WriteBuffer(fileWriter, ref seekpos, datalen, ref myData);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();
            long size = basesize + 1 + 3 + myData.Length;
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Data Reference Entry: ");
            sb.Append("Version:" + Version.ToString() + "|");
            sb.Append("Flags:" + Flags.ToString() + "|");
            sb.Append("Data: ");
            if ((myData != null) && (myData.Length > 0))
            {
                foreach (byte b in myData)
                {
                    sb.Append(Convert.ToString(b, 16) + " ");
                }
            }
            sb.Append("|");


            return sb.ToString();
        }

    }
}

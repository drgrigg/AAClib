using System;
using System.Text;
using System.IO;

namespace AAClib
{
    public class SoundMediaInformationAtom:SimpleAtom //smhd
    {
        private byte myVersion = 0;
        private UInt32 myFlags = 0; //NB: only 3 bytes

        public byte Version
        {
            get { return myVersion; }
            set { myVersion = value; }
        }

        public UInt32 Flags
        {
            get { return myFlags; }
            set { myFlags = value; }
        }

        private UInt16 myBalance = 0;

        public UInt16 Balance
        {
            get { return myBalance; }
            set { myBalance = value; }
        }
        private UInt16 myReserved = 0;

        public UInt16 Reserved
        {
            get { return myReserved; }
            set { myReserved = value; }
        }

        public SoundMediaInformationAtom()
        {
            TypeString = "smhd";
        }

        public SoundMediaInformationAtom(FileStream fileReader, long seekpos)
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
            Balance = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Reserved = Utilities.GetTwoBytes(fileReader, ref seekpos);
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
            Utilities.WriteTwoBytes(Balance, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Reserved, fileWriter, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();

            long size = basesize + 1 + 3 + 2 + 2;
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Sound Media Information Header|");
            return sb.ToString();
        }

    }
}

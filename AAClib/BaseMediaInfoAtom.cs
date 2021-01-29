using System;
using System.Text;
using System.IO;

namespace AAClib
{
    public class BaseMediaInfoAtom : SimpleAtom
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

        private UInt32 myNumEntries = 0;

        public UInt32 NumEntries
        {
            get { return myNumEntries; }
            set { myNumEntries = value; }
        }

        private UInt16 myGraphicsMode = 0;

        public UInt16 GraphicsMode
        {
            get { return myGraphicsMode; }
            set { myGraphicsMode = value; }
        }

        private UInt16 myRedColor = 0;

        public UInt16 RedColor
        {
            get { return myRedColor; }
            set { myRedColor = value; }
        }
        private UInt16 myGreenColor = 0;

        public UInt16 GreenColor
        {
            get { return myGreenColor; }
            set { myGreenColor = value; }
        }
        private UInt16 myBlueColor = 0;

        public UInt16 BlueColor
        {
            get { return myBlueColor; }
            set { myBlueColor = value; }
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

        public BaseMediaInfoAtom()
        {
            TypeString = "gmin";
        }

        public BaseMediaInfoAtom(FileStream fileReader, long seekpos)
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
            GraphicsMode = Utilities.GetTwoBytes(fileReader, ref seekpos);
            RedColor = Utilities.GetTwoBytes(fileReader, ref seekpos);
            GreenColor = Utilities.GetTwoBytes(fileReader, ref seekpos);
            BlueColor = Utilities.GetTwoBytes(fileReader, ref seekpos);
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
            Utilities.WriteTwoBytes(GraphicsMode, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(RedColor, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(GreenColor, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(BlueColor, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Balance, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Reserved, fileWriter, ref seekpos);

            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();
            long size = basesize + 1 + 3 + (6 * 2);
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Base media Info Atom|");
            sb.Append("Version:" + Version.ToString() + "|");
            sb.Append("Flags:" + Flags.ToString() + "|");
            sb.Append("Graphics Mode: " + GraphicsMode.ToString() + "|");
            sb.Append("Color: Red:" + Convert.ToString(RedColor, 16) + ", Green:" + Convert.ToString(GreenColor, 16) + ", Blue:" + Convert.ToString(BlueColor, 16) + "|");
            sb.Append("Balance: " + Balance.ToString() + "|");
            sb.Append("Reserved: " + Reserved.ToString() + "|");
            return sb.ToString();
        }
    }
}

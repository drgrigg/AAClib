using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public abstract class TableAtom:SimpleAtom
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

        public TableAtom()
        {
        }

        public TableAtom(FileStream fileReader, long seekpos)
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
            NumEntries = Utilities.GetFourBytes(fileReader, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            base.WriteData(fileWriter, ref seekpos);
            Utilities.WriteOneByte(Version, fileWriter, ref seekpos);
            Utilities.WriteThreeBytes(Flags, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(NumEntries, fileWriter, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();
            long size = basesize + 1 + 3 + 4;
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Table Atom|");
            sb.Append("Version:" + Version.ToString() + "|");
            sb.Append("Flags:" + Flags.ToString() + "|");
            return sb.ToString();

        }

    }
}

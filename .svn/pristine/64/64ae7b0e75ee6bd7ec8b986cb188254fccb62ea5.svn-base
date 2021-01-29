using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public abstract class DatedAtom:SimpleAtom
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

        private UInt64 myCreation = 0;
        private UInt64 myModified = 0;

        public UInt64 CreationSecs
        {
            get { return myCreation; }
            set { myCreation = value; }
        }

        public UInt64 ModifiedSecs
        {
            get { return myModified; }
            set { myModified = value; }
        }

        public DateTime CreationDate
        {
            get
            {
                DateTime basetime = new DateTime(1904, 1, 1);
                return basetime.AddSeconds((double)myCreation);
            }
            set
            {
                TimeSpan span = value - (new DateTime(1904, 1, 1));
                myCreation = Convert.ToUInt64(span.TotalSeconds);
            }
        }

        public DateTime ModifiedDate
        {
            get
            {
                DateTime basetime = new DateTime(1904, 1, 1);
                return basetime.AddSeconds((double)myModified);
            }
            set
            {
                TimeSpan span = value - (new DateTime(1904, 1, 1));
                myModified = Convert.ToUInt64(span.TotalSeconds);
            }
        }

        public DatedAtom()
        {

        }

        public DatedAtom(FileStream fileReader, long seekpos)
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

            if (Version == 0)
                CreationSecs = Utilities.GetFourBytes(fileReader, ref seekpos);
            else
                CreationSecs = Utilities.GetEightBytes(fileReader, ref seekpos);

            if (Version == 0)
                ModifiedSecs = Utilities.GetFourBytes(fileReader, ref seekpos);
            else
                ModifiedSecs = Utilities.GetEightBytes(fileReader, ref seekpos);

            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            base.WriteData(fileWriter, ref seekpos);

            Utilities.WriteOneByte(Version, fileWriter, ref seekpos);
            Utilities.WriteThreeBytes(Flags, fileWriter, ref seekpos);

            if (Version == 0)
            {
                UInt32 temp = Convert.ToUInt32(CreationSecs);
                Utilities.WriteFourBytes(temp, fileWriter, ref seekpos);
            }
            else
                Utilities.WriteEightBytes(CreationSecs, fileWriter, ref seekpos);

            if (Version == 0)
            {
                UInt32 temp = Convert.ToUInt32(ModifiedSecs);
                Utilities.WriteFourBytes(temp, fileWriter, ref seekpos);
            }
            else
                Utilities.WriteEightBytes(ModifiedSecs, fileWriter, ref seekpos);

            Utilities.Reporter.DoProcessing(seekpos);

        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();
            long size = basesize + 1 + 3;
            if (Version == 0)
                size += 8;
            else
                size += 16;
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Dated Atom|");
            sb.Append("Version:" + Version.ToString() + "|");
            sb.Append("Flags:" + Flags.ToString() + "|");
            sb.Append("Creation Date: " + CreationDate.ToShortDateString() + "|");
            sb.Append("Modified Date: " + ModifiedDate.ToShortDateString() + "|");
            return sb.ToString();
        }
    }
}

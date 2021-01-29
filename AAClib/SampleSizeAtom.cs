using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class SampleSizeAtom:SimpleAtom //can't use TableAtom as order of fields is different
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

        private UInt32 myDefaultSampleSize = 0;

        public UInt32 DefaultSampleSize
        {
            get { return myDefaultSampleSize; }
            set { myDefaultSampleSize = value; }
        }


        private UInt32 myNumEntries = 0;

        public UInt32 NumEntries
        {
            get { return myNumEntries; }
            set { myNumEntries = value; }
        }

        public List<UInt32> mySampleSizeList = new List<UInt32>();

        public SampleSizeAtom()
        {
            TypeString = "stsz";
        }

        public SampleSizeAtom(FileStream fileReader, long seekpos)
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

        public void ClearSampleSizes()
        {
            mySampleSizeList = new List<UInt32>();
            myDefaultSampleSize = 0;
        }

        public void ResetNumEntries()
        {
            NumEntries = Convert.ToUInt32(mySampleSizeList.Count);
        }

        public void AddSampleSize(UInt32 samplesize)
        {
            mySampleSizeList.Add(samplesize);
        }

        protected override void ReadData(FileStream fileReader, ref long seekpos)
        {
            base.ReadData(fileReader, ref seekpos);
            Version = Utilities.GetOneByte(fileReader, ref seekpos);
            Flags = Utilities.GetThreeBytes(fileReader, ref seekpos);
            DefaultSampleSize = Utilities.GetFourBytes(fileReader, ref seekpos);

            NumEntries = Utilities.GetFourBytes(fileReader, ref seekpos);

            if (DefaultSampleSize == 0) //only get table if no default sample size
            {
                for (int i = 0; i < NumEntries; i++)
                {
                    mySampleSizeList.Add(Utilities.GetFourBytes(fileReader, ref seekpos));
                }
            }
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
            Utilities.WriteFourBytes(DefaultSampleSize, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(NumEntries, fileWriter, ref seekpos);

            if ((DefaultSampleSize == 0) && (mySampleSizeList.Count > 0)) //only have table if no default sample size
            {
                for (int i = 0; i < mySampleSizeList.Count; i++)
                {
                    Utilities.WriteFourBytes(mySampleSizeList[i], fileWriter, ref seekpos);
                }
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            //this is trickier than it should be, because 'NumEntries' may have a non-zero value even if there's no table
            //and we may need to keep this value.  No idea what's going on!

            if ((DefaultSampleSize == 0) && (mySampleSizeList.Count > 0)) //if there's a table, then NumEntries should show size of table.
                NumEntries = (UInt32)mySampleSizeList.Count;
            
            long basesize = base.CalculateSize();

            long size = basesize + 1 + 3 + 4 + 4;

            if ((DefaultSampleSize == 0) && (mySampleSizeList.Count > 0)) //if there's a table, size is the size plus the table size.
                size += (NumEntries * 4);

            return size;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Sample Size Atom|");
            sb.Append("Version:" + Version.ToString() + "|");
            sb.Append("Flags:" + Flags.ToString() + "|");
            sb.Append("Default Sample Size: " + DefaultSampleSize.ToString("#,##0") + "|");
            if ((DefaultSampleSize == 0) && (mySampleSizeList.Count > 0))
            {
                sb.Append("Num Entries:" + NumEntries.ToString("#,##0") + "|");
                for (int i = 0; i < Math.Min(NumEntries, 10); i++)
                {
                    sb.Append("Sample Size: " + mySampleSizeList[i].ToString("#,##0") + "|");
                }
                if (NumEntries > 10)
                    sb.Append("...[remainder omitted]");
            }

            return sb.ToString();
        }

        public UInt32 GetSampleSize(int index)
        {
            if ((index >= 0) && (index < mySampleSizeList.Count))
                return mySampleSizeList[index];
            else
                return 0;
        }
    }
}

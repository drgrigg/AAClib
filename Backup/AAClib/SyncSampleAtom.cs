using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class SyncSampleAtom:TableAtom
    {
        private List<UInt32> mySyncList = new List<UInt32>();

        public int SyncSampleCount
        {
            get { return mySyncList.Count; }
        }

        public void AddSyncSample(UInt32 offset)
        {
            mySyncList.Add(offset);
        }

        public void RemoveSyncSample(UInt32 offset)
        {
            mySyncList.Remove(offset);
        }

        public UInt32 SyncSample(int index)
        {
            if (index < mySyncList.Count)
                return mySyncList[index];
            else
                return 0;
        }


        public SyncSampleAtom()
        {
        }

        public SyncSampleAtom(FileStream fileReader, long seekpos)
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

            for (int i = 0; i < NumEntries; i++)
            {
                mySyncList.Add(Utilities.GetFourBytes(fileReader,ref seekpos));
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);
            foreach (UInt32 sync in mySyncList)
            {
                Utilities.WriteFourBytes(sync, fileWriter, ref seekpos);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            NumEntries = (UInt32)mySyncList.Count;
            long basesize = base.CalculateSize();

            long size = basesize + (NumEntries * 4);
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Sync Sample Atom|");
            sb.Append("Num Entries: " + NumEntries.ToString("#,##0") + "|");
            for (int i = 0; i < Math.Min(NumEntries, 10); i++)
            {
                sb.Append("SyncSample: " + mySyncList[i].ToString("#,##0") + "|");
            }
            if (NumEntries > 10)
                sb.Append("...[remainder omitted]");

            return sb.ToString();
        }
    }
}

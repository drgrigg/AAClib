using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class SampleToChunkEntry
    {
        public UInt32 FirstChunk = 0;
        public UInt32 SamplesPerChunk = 0;
        public UInt32 SampleDescriptionID = 0;

        /// <summary>
        /// LastChunk is a CALCULATED field for convenience - neither read nor written to disk.
        /// </summary>
        public UInt32 LastChunk = 0;

        public override string ToString()
        {
            return "FirstChunk: " + FirstChunk.ToString("#,##0") + ", SamplesPerChunk: " + SamplesPerChunk.ToString("#,##0") + ", SampleDescriptionID: " + SampleDescriptionID.ToString("#,##0");
        }
    }

    public class SampleToChunkAtom:TableAtom //stsc
    {
        private List<SampleToChunkEntry> mySTCList = new List<SampleToChunkEntry>();

        public SampleToChunkAtom()
        {
            TypeString = "stsc";
        }

        public SampleToChunkAtom(FileStream fileReader, long seekpos)
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

        public void AddSampleToChunkEntry(SampleToChunkEntry stce)
        {
            mySTCList.Add(stce);
        }

        public void RemoveSampleToChunkEntry(SampleToChunkEntry stce)
        {
            mySTCList.Remove(stce);
        }

        public void ClearSampleToChunkEntries()
        {
            mySTCList = new List<SampleToChunkEntry>();
        }

        public void ConsolidateEntries()
        {
            if (mySTCList.Count > 1)
            {
                for (int i = (mySTCList.Count - 1); i > 0; i--)
                {
                    SampleToChunkEntry thisEntry = mySTCList[i];
                    SampleToChunkEntry prevEntry = mySTCList[i - 1];
                    if (
                        (prevEntry.SampleDescriptionID == thisEntry.SampleDescriptionID)
                        &&
                         (prevEntry.SamplesPerChunk == thisEntry.SamplesPerChunk)
                    )
                    {
                        //then this entry is redundant, so delete it
                        mySTCList.Remove(thisEntry);
                    }
                }
            }
            ResetNumEntries();
        }

        public void ResetNumEntries()
        {
            NumEntries = Convert.ToUInt32(mySTCList.Count);
        }

        protected override void ReadData(FileStream fileReader, ref long seekpos)
        {
            base.ReadData(fileReader, ref seekpos);

            for (int i = 0; i < NumEntries; i++)
            {
                SampleToChunkEntry stentry = new SampleToChunkEntry();
                stentry.FirstChunk = Utilities.GetFourBytes(fileReader, ref seekpos);
                stentry.SamplesPerChunk = Utilities.GetFourBytes(fileReader, ref seekpos);
                stentry.SampleDescriptionID = Utilities.GetFourBytes(fileReader, ref seekpos);
                mySTCList.Add(stentry);
                Utilities.Reporter.DoProcessing(seekpos);
            }
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);

            foreach (SampleToChunkEntry stentry in mySTCList)
            {
                Utilities.WriteFourBytes(stentry.FirstChunk, fileWriter, ref seekpos);
                Utilities.WriteFourBytes(stentry.SamplesPerChunk, fileWriter, ref seekpos);
                Utilities.WriteFourBytes(stentry.SampleDescriptionID, fileWriter, ref seekpos);
                Utilities.Reporter.DoProcessing(seekpos);
            }
        }

        public override long CalculateSize()
        {
            NumEntries = (UInt32)mySTCList.Count;
            long basesize = base.CalculateSize();

            long size = basesize + (NumEntries * 12);
            return size;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Sample to Chunk Atom|");
            sb.Append("NumEntries: " + NumEntries.ToString("#,##0") + "|");
            for (int i = 0; i < Math.Min(NumEntries, 20); i++)
            {
                sb.Append(mySTCList[i].ToString() + "|");
            }
            if (NumEntries > 20)
                sb.Append("...[remainder omitted]");

            return sb.ToString();
        }

        public SampleToChunkEntry GetSampleToChunk(int index)
        {
            if ((index >= 0) && (index < mySTCList.Count))
                return mySTCList[index];
            else
                return null;
        }

        public void CalculateLastChunks(UInt32 lastchunknum)
        {
            if (mySTCList.Count == 0)
                return;

            if (mySTCList.Count == 1)
            {
                mySTCList[0].LastChunk = lastchunknum;
                return;
            }

            for (int i = 0; i < (mySTCList.Count - 1); i++)
            {
                mySTCList[i].LastChunk = mySTCList[i+1].FirstChunk - 1;
            }
            mySTCList[mySTCList.Count - 1].LastChunk = lastchunknum;

        }
    }
}

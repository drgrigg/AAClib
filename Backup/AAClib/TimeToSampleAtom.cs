using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class TimeToSampleEntry
    {
        public UInt32 SampleCount = 0;
        public UInt32 SampleDuration = 0;

        public override string ToString()
        {
            return "SampleCount:" + SampleCount.ToString("#,##0") + ", SampleDuration:" + SampleDuration.ToString("#,##0");
        }
    }

    public class TimeToSampleAtom : TableAtom
    {
        private List<TimeToSampleEntry> myTTSList = new List<TimeToSampleEntry>();

        public void AddTimeToSample(TimeToSampleEntry tts)
        {
            myTTSList.Add(tts);
        }

        public void RemoveTimeToSample(TimeToSampleEntry tts)
        {
            myTTSList.Remove(tts);
        }

        public TimeToSampleEntry TimeToSample(int index)
        {
            if (index < myTTSList.Count)
                return myTTSList[index];
            else
                return null;
        }

        public int TimeToSampleCount
        {
            get { return myTTSList.Count; }
        }

        public TimeToSampleAtom()
        {
            TypeString = "stts";
        }

        public TimeToSampleAtom(FileStream fileReader, long seekpos)
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


        public void ClearTimeToSamples()
        {
            myTTSList = new List<TimeToSampleEntry>();
        }

        public void ResetNumEntries()
        {
            NumEntries = Convert.ToUInt32(myTTSList.Count);
        }

        public void ConsolidateEntries()
        {
            ConsolidateEntries(false);
        }

        public void ConsolidateEntries(bool removeSingletons)
        {
            if (myTTSList.Count > 1)
            {
                for (int i = (myTTSList.Count - 1); i > 0; i--)
                {
                    TimeToSampleEntry thisEntry = myTTSList[i];
                    TimeToSampleEntry prevEntry = myTTSList[i - 1];

                    //this is a fudge to fool iTunes, we remove 'odd' entries but retain total sample count
                    if ((removeSingletons)
                        &&
                        (thisEntry.SampleCount == 1)
                        &&
                        (prevEntry.SampleDuration >= thisEntry.SampleDuration)
                        )
                    {
                        prevEntry.SampleCount += thisEntry.SampleCount;
                        //then this entry is redundant, so delete it
                        myTTSList.Remove(thisEntry);
                    }
                }

                for (int i = (myTTSList.Count - 1); i > 0; i--)
                {
                    TimeToSampleEntry thisEntry = myTTSList[i];
                    TimeToSampleEntry prevEntry = myTTSList[i - 1];

                    if (prevEntry.SampleDuration == thisEntry.SampleDuration)
                    {
                        prevEntry.SampleCount += thisEntry.SampleCount;
                        //then this entry is redundant, so delete it
                        myTTSList.Remove(thisEntry);
                        continue;
                    }
                }
            }
            ResetNumEntries();
        }

        public void AddTimeToSample(UInt32 sampleCount, UInt32 sampleDuration)
        {
            TimeToSampleEntry ttse = new TimeToSampleEntry();
            ttse.SampleCount = sampleCount;
            ttse.SampleDuration = sampleDuration;
            myTTSList.Add(ttse);
        }

        protected override void ReadData(FileStream fileReader, ref long seekpos)
        {
            base.ReadData(fileReader, ref seekpos);

            for (int i = 0; i < NumEntries; i++)
            {
                TimeToSampleEntry tse = new TimeToSampleEntry();
                tse.SampleCount = Utilities.GetFourBytes(fileReader, ref seekpos);
                tse.SampleDuration = Utilities.GetFourBytes(fileReader, ref seekpos);

                AddTimeToSample(tse);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos, 16));

            base.WriteData(fileWriter, ref seekpos);
            foreach (TimeToSampleEntry tse in myTTSList)
            {
                Utilities.WriteFourBytes(tse.SampleCount, fileWriter, ref seekpos);
                Utilities.WriteFourBytes(tse.SampleDuration, fileWriter, ref seekpos);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            NumEntries = (UInt32)myTTSList.Count;
            long basesize = base.CalculateSize();
            long size = basesize + NumEntries * 8;
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Time to Sample Atom|");
            sb.Append("Num Entries: " + NumEntries.ToString("#,##0") + "|");
            for (int i = 0; i < Math.Min(NumEntries, 10); i++)
            {
                sb.Append(myTTSList[i].ToString() + "|");
            }
            if (NumEntries > 10)
                sb.Append("...[remainder omitted]");

            return sb.ToString();
        }

        public UInt32 FindSampleForTime(UInt32 wantedTime)
        {
            UInt32 cumulativeTime = 0;
            UInt32 cumulativeSamples = 0;

            UInt32 previousTime = 0;
            UInt32 previousSamples = 0;

            //calculate EndTimes
            for (int i = 0; i < myTTSList.Count; i++)
            {
                TimeToSampleEntry ttse = myTTSList[i];
                if (ttse.SampleDuration == 0)
                    return 0;

                previousSamples = cumulativeSamples;
                previousTime = cumulativeTime;
                cumulativeTime += ttse.SampleCount * ttse.SampleDuration;
                cumulativeSamples += ttse.SampleCount;

                if (cumulativeTime >= wantedTime)
                {
                    return previousSamples + ((wantedTime - previousTime) / ttse.SampleDuration);

                }
            }
            return 0; //wanted time was greater than max time
        }

        public UInt32 GetSampleStartTime(UInt32 SampleNum)
        {
            UInt32 cumulativeTime = 0;
            UInt32 cumulativeSamples = 0;

            UInt32 previousTime = 0;
            UInt32 previousSamples = 0;

            //calculate EndTimes
            for (int i = 0; i < myTTSList.Count; i++)
            {
                TimeToSampleEntry ttse = myTTSList[i];
                previousSamples = cumulativeSamples;
                previousTime = cumulativeTime;
                cumulativeTime += ttse.SampleCount * ttse.SampleDuration;
                cumulativeSamples += ttse.SampleCount;

                if (cumulativeSamples >= SampleNum)
                {
                    return previousTime + (ttse.SampleDuration * (SampleNum - previousSamples - 1));
                }
            }
            return 0; //wanted sample was greater than max sample
        }

        //-----------------------------------------------------------------------------------
        public long GetSampleEndTime(UInt32 SampleNum)
        {
            UInt32 cumulativeTime = 0;
            UInt32 cumulativeSamples = 0;

            //calculate EndTimes
            for (int i = 0; i < myTTSList.Count; i++)
            {
                TimeToSampleEntry ttse = myTTSList[i];

                cumulativeTime += ttse.SampleCount * ttse.SampleDuration;
                cumulativeSamples += ttse.SampleCount;

                if (cumulativeSamples >= SampleNum)
                {
                    return cumulativeTime;
                }
            }
            return 0; //wanted sample was greater than max sample
        }
    }
}

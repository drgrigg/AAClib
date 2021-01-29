using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class MediaHeaderAtom:DatedAtom
    {      
        private UInt32 myTimeScale = 0;
        private UInt32 myDuration = 0;
        private UInt16 myLanguage = 0;
        private UInt16 myQuality = 0;

        public UInt32 TimeUnitsPerSecond
        {
            get { return myTimeScale; }
            set { myTimeScale = value; }
        }

        public UInt32 DurationInTimeUnits
        {
            get { return myDuration; }
            set { myDuration = value; }
        }

        public double DurationInSecs
        {
            get
            {
                return (double)myDuration / (double)myTimeScale;
            }
        }

        public UInt16 Language
        {
            get { return myLanguage; }
            set { myLanguage = value; }
        }

        public UInt16 Quality
        {
            get { return myQuality; }
            set { myQuality = value; }
        }

        public MediaHeaderAtom()
        {
            TypeString = "mdhd";
            CreationDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        public MediaHeaderAtom(FileStream fileReader, long seekpos)
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
            TimeUnitsPerSecond = Utilities.GetFourBytes(fileReader, ref seekpos);
            DurationInTimeUnits = Utilities.GetFourBytes(fileReader, ref seekpos);
            Language = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Quality = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            base.WriteData(fileWriter, ref seekpos);
            Utilities.WriteFourBytes(TimeUnitsPerSecond, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(DurationInTimeUnits, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Language, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Quality, fileWriter, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();
            long size = basesize + 4 + 4 + 2 + 2;
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Media Header Atom|");
            sb.Append("Time Units per Second: " + TimeUnitsPerSecond.ToString("#,##0") + "|");
            sb.Append("Duration in Time Units: " + DurationInTimeUnits.ToString("#,##0") + "|");
            sb.Append("Duration in Seconds: " + DurationInSecs.ToString("#,##0.0") + "|");
            sb.Append("Language: " + Language.ToString() + "|");
            sb.Append("Quality: " + Quality.ToString() + "|");

            return sb.ToString();
        }

    }
}

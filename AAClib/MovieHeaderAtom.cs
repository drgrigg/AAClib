using System;
using System.Text;
using System.IO;

namespace AAClib
{
    public class MovieHeaderAtom:DatedAtom
    {
        private UInt32 myTimeScale = 0;

        /// <summary>
        /// TimeScale is essentially 'time units per second'
        /// </summary>
        public UInt32 TimeScale
        {
            get { return myTimeScale; }
            set { myTimeScale = value; }
        }
        private UInt32 myDuration = 0;

        /// <summary>
        /// Duration is in time units
        /// </summary>
        public UInt32 DurationInTimeUnits
        {
            get { return myDuration; }
            set { myDuration = value; }
        }

        public double DurationInSeconds
        {
            get { return (double)myDuration / (double)myTimeScale; }
            set
            {
                double durationinunits = value * (double)myTimeScale;
                myDuration = Convert.ToUInt32(durationinunits);
            }
        }

        private UInt32 myPreferredRate = 65536;

        public UInt32 PreferredRate
        {
            get { return myPreferredRate; }
            set { myPreferredRate = value; }
        }
        private UInt16 myPreferredVolume = 256;

        public UInt16 PreferredVolume
        {
            get { return myPreferredVolume; }
            set { myPreferredVolume = value; }
        }
        private byte[] myReserved = new byte[10];

        public byte[] Reserved
        {
            get { return myReserved; }
            set { myReserved = value; }
        }
        private byte[] myMatrix = new byte[36] {0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,40,0,0,0};

        public byte[] Matrix
        {
            get { return myMatrix; }
            set { myMatrix = value; }
        }
        private UInt32 myPreviewTime = 0;

        public UInt32 PreviewTime
        {
            get { return myPreviewTime; }
            set { myPreviewTime = value; }
        }
        private UInt32 myPreviewDuration = 0;

        public UInt32 PreviewDuration
        {
            get { return myPreviewDuration; }
            set { myPreviewDuration = value; }
        }
        private UInt32 myPosterTime = 0;

        public UInt32 PosterTime
        {
            get { return myPosterTime; }
            set { myPosterTime = value; }
        }
        private UInt32 mySelectionTime = 0;

        public UInt32 SelectionTime
        {
            get { return mySelectionTime; }
            set { mySelectionTime = value; }
        }
        private UInt32 mySelectionDuration = 0;

        public UInt32 SelectionDuration
        {
            get { return mySelectionDuration; }
            set { mySelectionDuration = value; }
        }
        private UInt32 myCurrentTime = 0;

        public UInt32 CurrentTime
        {
            get { return myCurrentTime; }
            set { myCurrentTime = value; }
        }
        private UInt32 myNextTrackID = 1;

        public UInt32 NextTrackID
        {
            get { return myNextTrackID; }
            set { myNextTrackID = value; }
        }

        public MovieHeaderAtom()
        {
        }

        public MovieHeaderAtom(FileStream fileReader, long seekpos)
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
            TimeScale = Utilities.GetFourBytes(fileReader, ref seekpos);
            DurationInTimeUnits = Utilities.GetFourBytes(fileReader, ref seekpos);
            PreferredRate = Utilities.GetFourBytes(fileReader, ref seekpos);
            PreferredVolume = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Utilities.GetBuffer(fileReader, ref seekpos, 10, ref myReserved);
            Utilities.GetBuffer(fileReader, ref seekpos, 36, ref myMatrix);
            PreviewTime = Utilities.GetFourBytes(fileReader, ref seekpos);
            PreviewDuration = Utilities.GetFourBytes(fileReader, ref seekpos);
            PosterTime = Utilities.GetFourBytes(fileReader, ref seekpos);
            SelectionTime = Utilities.GetFourBytes(fileReader, ref seekpos);
            SelectionDuration = Utilities.GetFourBytes(fileReader, ref seekpos);
            CurrentTime = Utilities.GetFourBytes(fileReader, ref seekpos);
            NextTrackID = Utilities.GetFourBytes(fileReader, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);
            Utilities.WriteFourBytes(TimeScale, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(DurationInTimeUnits, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(PreferredRate, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(PreferredVolume, fileWriter, ref seekpos);
            Utilities.WriteBuffer(fileWriter, ref seekpos, 10, ref myReserved);
            Utilities.WriteBuffer(fileWriter, ref seekpos, 36, ref myMatrix);
            Utilities.WriteFourBytes(PreviewTime, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(PreviewDuration, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(PosterTime, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(SelectionTime, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(SelectionDuration, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(CurrentTime, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(NextTrackID, fileWriter, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();
            long size = basesize + 12 + 2 + 10 + 36 + (7 * 4);
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Movie Header Atom|");
            sb.Append("TimeScale: " + TimeScale.ToString("#,##0") + "|");
            sb.Append("Duration: " + DurationInTimeUnits.ToString("#,##0") + "|");
            sb.Append("Duration in Secs: " + DurationInSeconds.ToString("#,##0.0") + "|");
            sb.Append("PreferredRate:" + PreferredRate.ToString("") + "|");
            sb.Append("PreferredVolume:" +  PreferredVolume.ToString("") + "|");

            sb.Append("Reserved: ");
            if ((myReserved != null) && (myReserved.Length > 0))
            {
                foreach (byte b in myReserved)
                {
                    sb.Append(Convert.ToString(b, 16) + " ");
                }
            }
            sb.Append("|");

            sb.Append("Matrix: ");
            if ((myMatrix != null) && (myMatrix.Length > 0))
            {
                foreach (byte b in myMatrix)
                {
                    sb.Append(Convert.ToString(b, 16) + " ");
                }
            }
            sb.Append("|");

            sb.Append("PreviewTime:" +  PreviewTime.ToString("") + "|");
            sb.Append("PreviewDuration:" +  PreviewDuration.ToString("") + "|");
            sb.Append("PosterTime:" +  PosterTime.ToString("") + "|");
            sb.Append("SelectionTime:" +  SelectionTime.ToString("") + "|");
            sb.Append("SelectionDuration:" +  SelectionDuration.ToString("") + "|");
            sb.Append("CurrentTime:" +  CurrentTime.ToString("") + "|");
            sb.Append("NextTrackID: " + NextTrackID.ToString() + "|");

            sb.Append(base.ToString());

            return sb.ToString();
        }


    }
}

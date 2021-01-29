using System;
using System.Text;
using System.IO;

namespace AAClib
{
    public class TrackHeaderAtom : DatedAtom
    {

        private UInt32 myTrackID = 1;

        public UInt32 TrackID
        {
            get { return myTrackID; }
            set { myTrackID = value; }
        }
        private UInt32 myReserved1 = 0;

        public UInt32 Reserved1
        {
            get { return myReserved1; }
            set { myReserved1 = value; }
        }
        private UInt32 myDuration = 0;

        public UInt32 Duration
        {
            get { return myDuration; }
            set { myDuration = value; }
        }
        private UInt64 myReserved2 = 0;

        public UInt64 Reserved2
        {
            get { return myReserved2; }
            set { myReserved2 = value; }
        }

        private UInt16 myLayer = 0;

        public UInt16 Layer
        {
            get { return myLayer; }
            set { myLayer = value; }
        }


        private UInt16 myAlternate = 0;

        public UInt16 Alternate
        {
            get { return myAlternate; }
            set { myAlternate = value; }
        }
        private UInt16 myVolume = 1;

        public UInt16 Volume
        {
            get { return myVolume; }
            set { myVolume = value; }
        }
        private UInt16 myReserved3 = 0;

        public UInt16 Reserved3
        {
            get { return myReserved3; }
            set { myReserved3 = value; }
        }

        public byte[] Matrix = new byte[36];

        private UInt32 myWidth = 0;

        public UInt32 Width
        {
            get { return myWidth; }
            set { myWidth = value; }
        }
        private UInt32 myHeight = 0;

        public UInt32 Height
        {
            get { return myHeight; }
            set { myHeight = value; }
        }


        public TrackHeaderAtom()
        {
            TypeString = "tkhd";
            Flags = 8 + 4 + 2 + 1; //all enabled
            CreationDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        public TrackHeaderAtom(FileStream fileReader, long seekpos)
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

            TrackID = Utilities.GetFourBytes(fileReader, ref seekpos);
            Reserved1 = Utilities.GetFourBytes(fileReader, ref seekpos);
            Duration = Utilities.GetFourBytes(fileReader, ref seekpos);
            Reserved2 = Utilities.GetEightBytes(fileReader, ref seekpos);
            Layer = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Alternate = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Volume = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Reserved3 = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Utilities.GetBuffer(fileReader, ref seekpos, 36, ref Matrix);
            Width = Utilities.GetFourBytes(fileReader, ref seekpos);
            Height = Utilities.GetFourBytes(fileReader, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);

            Utilities.WriteFourBytes(TrackID, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(Reserved1, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(Duration, fileWriter, ref seekpos);
            Utilities.WriteEightBytes(Reserved2, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Layer, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Alternate, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Volume, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Reserved3, fileWriter, ref seekpos);
            Utilities.WriteBuffer(fileWriter, ref seekpos, 36, ref Matrix);
            Utilities.WriteFourBytes(Width, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(Height, fileWriter, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();

            long size = basesize + 3 * 4 + 8 + 4 * 2 + 36 + 2 * 4;
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Track Header Atom|");
            sb.Append("Track ID = " + TrackID.ToString() + "|");
            sb.Append("Reserved1:" + Reserved1.ToString() + "|");
            sb.Append("Duration:" + Duration.ToString("#,##0") + "|");
            sb.Append("Reserved2:" + Reserved2.ToString() + "|");
            sb.Append("Layer:" + Layer.ToString() + "|");
            sb.Append("Alternate:" + Alternate.ToString() + "|");
            sb.Append("Volume:" + Volume.ToString() + "|");
            sb.Append("Reserved3:" + Reserved3.ToString() + "|");

            sb.Append("Matrix: ");
            if ((Matrix != null) && (Matrix.Length > 0))
            {
                foreach (byte b in Matrix)
                {
                    sb.Append(Convert.ToString(b, 16) + " ");
                }
            }
            sb.Append("|");
            sb.Append("Width:" + Width.ToString() + "|");
            sb.Append("Height:" + Height.ToString() + "|");
            return sb.ToString();
        }
    }
}

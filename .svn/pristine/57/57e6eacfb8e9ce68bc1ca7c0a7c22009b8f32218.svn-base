using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class VideoDescriptionEntry:SampleDescriptionEntry
    {
        private UInt16 myVersion = 0;

        public UInt16 Version
        {
            get { return myVersion; }
            set { myVersion = value; }
        }
        private UInt16 myRevisionLevel = 0;

        public UInt16 RevisionLevel
        {
            get { return myRevisionLevel; }
            set { myRevisionLevel = value; }
        }
        private UInt32 myVendor = 0;

        public UInt32 Vendor
        {
            get { return myVendor; }
            set { myVendor = value; }
        }
        private UInt32 myTemporalQuality = 0;

        public UInt32 TemporalQuality
        {
            get { return myTemporalQuality; }
            set { myTemporalQuality = value; }
        }
        private UInt32 mySpatialQuality = 0;

        public UInt32 SpatialQuality
        {
            get { return mySpatialQuality; }
            set { mySpatialQuality = value; }
        }
        private UInt16 myWidth = 0;

        public UInt16 Width
        {
            get { return myWidth; }
            set { myWidth = value; }
        }
        private UInt16 myHeight = 0;

        public UInt16 Height
        {
            get { return myHeight; }
            set { myHeight = value; }
        }
        private UInt32 myHResolutionRaw = 0;

        public UInt32 HResolutionRaw
        {
            get { return myHResolutionRaw; }
            set { myHResolutionRaw = value; }
        }

        public double HResolution
        {
            get
            {
                try
                {
                    int integer_part = Convert.ToInt32((myHResolutionRaw & 0xFFFF0000) >> 16);
                    int fraction_part = Convert.ToInt32((myHResolutionRaw & 0xFFFF));
                    return (double)integer_part + (((double)fraction_part) / 256); //very uncertain about this divisor.  Should it be 100?
                }
                catch
                {
                    return 0.0;
                }
            }
            set
            {
                double integer_dbl = (Math.Floor(value));
                double fraction_dbl = Math.Floor((value - integer_dbl) * 256);//very uncertain about this multiplier.  Should it be 100?
                try
                {
                    int integer_part = Convert.ToInt32(integer_dbl);
                    int fraction_part = Convert.ToInt32(fraction_dbl);
                    myHResolutionRaw = (UInt32)((integer_part << 16  + fraction_part));
                }
                catch
                {
                    //do nowt
                }
            }
        }
        private UInt32 myVResolutionRaw = 0;

        public double VResolution
        {
            get
            {
                try
                {
                    int integer_part = Convert.ToInt32((myVResolutionRaw & 0xFFFF0000) >> 16);
                    int fraction_part = Convert.ToInt32((myVResolutionRaw & 0xFFFF));
                    return (double)integer_part + (((double)fraction_part) / 256); //very uncertain about this divisor.  Should it be 100?
                }
                catch
                {
                    return 0.0;
                }
            }
            set
            {
                double integer_dbl = (Math.Floor(value));
                double fraction_dbl = Math.Floor((value - integer_dbl) * 256);//very uncertain about this multiplier.  Should it be 100?
                try
                {
                    int integer_part = Convert.ToInt32(integer_dbl);
                    int fraction_part = Convert.ToInt32(fraction_dbl);
                    myVResolutionRaw = (UInt32)((integer_part << 16 + fraction_part));
                }
                catch
                {
                    //do nowt
                }
            }
        }

        public UInt32 VResolutionRaw
        {
            get { return myVResolutionRaw; }
            set { myVResolutionRaw = value; }
        }
        private UInt32 myDataSize = 0;

        public UInt32 DataSize
        {
            get { return myDataSize; }
            set { myDataSize = value; }
        }
        private UInt16 myFrameCount = 0;

        public UInt16 FrameCount
        {
            get { return myFrameCount; }
            set { myFrameCount = value; }
        }
        private string myCompressorName = "";

        public string CompressorName
        {
            get { return myCompressorName; }
            set { myCompressorName = value; }
        }
        private UInt16 myColorDepth = 0;

        public UInt16 ColorDepth
        {
            get { return myColorDepth; }
            set { myColorDepth = value; }
        }
        private UInt16 myColorTableID = 0xFFFF; //seems to be be default

        public UInt16 ColorTableID
        {
            get { return myColorTableID; }
            set { myColorTableID = value; }
        }

        //how to deal with a color table? Not enough info in the spec.
        //just use this to remember any remaining info we can't interpret
        private byte[] myRemainder = null;



        public VideoDescriptionEntry()
        {
            TypeString = "jpeg";
            ColorDepth = 24;
            ColorTableID = 65535;
            CompressorName = "Photo - JPEG";
            DataSize = 0;
            FrameCount = 1;
            RevisionLevel = 0;
            SpatialQuality = 512;
            TemporalQuality = 0;
            Vendor = 0x6170706C;
            //HResolutionRaw = 4718592;
            //VResolutionRaw = 4718592;
            HResolution = 72.0;
            VResolution = 72.0;
            Width = 300;
            Height = 300;
        }

        public VideoDescriptionEntry(FileStream fileReader, ref long seekpos)
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
            //keep this to calculate data size we've read
            long startseek = seekpos;

            base.ReadData(fileReader, ref seekpos);
            Version = Utilities.GetTwoBytes(fileReader, ref seekpos);
            RevisionLevel = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Vendor = Utilities.GetFourBytes(fileReader, ref seekpos);
            TemporalQuality = Utilities.GetFourBytes(fileReader, ref seekpos);
            SpatialQuality = Utilities.GetFourBytes(fileReader, ref seekpos);
            Width = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Height = Utilities.GetTwoBytes(fileReader, ref seekpos);
            HResolutionRaw = Utilities.GetFourBytes(fileReader, ref seekpos);
            VResolutionRaw = Utilities.GetFourBytes(fileReader, ref seekpos);
            DataSize = Utilities.GetFourBytes(fileReader, ref seekpos);
            FrameCount = Utilities.GetTwoBytes(fileReader, ref seekpos);
            
            //Compressor name is in a fixed length 32 byte block:

            byte[] tempbuffer = new byte[32];
            Utilities.GetBuffer(fileReader, ref seekpos, 32, ref tempbuffer);

            CompressorName = Utilities.GetStringFromArray(tempbuffer);

            ColorDepth = Utilities.GetTwoBytes(fileReader, ref seekpos);
            ColorTableID = Utilities.GetTwoBytes(fileReader, ref seekpos);

            long readbytes = seekpos - startseek;
            if (readbytes < Size) //then we haven't read all the data, so just capture it.
            {
                long longbytes = (long)Size - readbytes;

                int shortbytes = Convert.ToInt32(longbytes);

                myRemainder = new byte[Size - readbytes];
                Utilities.GetBuffer(fileReader, ref seekpos, shortbytes, ref myRemainder);
            }

            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mySize = CalculateSize();
            SetActualSize(mySize);

            base.WriteData(fileWriter, ref seekpos);

            Utilities.WriteTwoBytes(Version, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(RevisionLevel, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(Vendor, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(TemporalQuality, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(SpatialQuality, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Width, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Height, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(HResolutionRaw, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(VResolutionRaw, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(DataSize, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(FrameCount, fileWriter, ref seekpos);

            int strlen = CompressorName.Length;
            if (strlen > 255) strlen = 255;
            byte[] tempbuffer = new byte[32];
            tempbuffer[0] = Convert.ToByte(strlen);
            for (int i = 1; i <= strlen; i++)
            {
                tempbuffer[i] = (byte)CompressorName[i - 1];
            }

            Utilities.WriteBuffer(fileWriter, ref seekpos, 32, ref tempbuffer);

            Utilities.WriteTwoBytes(ColorDepth, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(ColorTableID, fileWriter, ref seekpos);

            if ((myRemainder != null) && (myRemainder.Length > 0))
            {
                Utilities.WriteBuffer(fileWriter, ref seekpos, myRemainder.Length, ref myRemainder);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            long size = base.CalculateSize();
            size += 2 + 2 + 4 + 4 + 4 + 2 + 2 + 4 + 4 + 4 + 2 + 32 + 2 + 2;
            if (myRemainder != null)
                size += myRemainder.Length;
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("VideoDescriptionEntry: ");
            sb.Append(" Version: " + Version.ToString());
            sb.Append(" RevisionLevel: " + RevisionLevel.ToString());
            sb.Append(" Vendor: " + Vendor.ToString());
            sb.Append(" TemporalQuality: " + TemporalQuality.ToString());
            sb.Append(" SpatialQuality: " + SpatialQuality.ToString());
            sb.Append(" Width: " + Width.ToString());
            sb.Append(" Height: " + Height.ToString());
            sb.Append(" HResolutionRaw: " + HResolutionRaw.ToString());
            sb.Append(" VResolutionRaw: " + VResolutionRaw.ToString());
            sb.Append(" DataSize: " + DataSize.ToString());
            sb.Append(" FrameCount: " + FrameCount.ToString());
            sb.Append(" CompressorName: " + CompressorName);
            sb.Append(" ColorDepth: " + ColorDepth.ToString());
            sb.Append(" ColorTableID: " + ColorTableID.ToString());

            sb.Append("Remainder: ");
            if ((myRemainder != null) && (myRemainder.Length > 0))
            {
                foreach (byte b in myRemainder)
                {
                    sb.Append(Convert.ToString(b, 16) + " ");
                }
            }
            sb.Append("|");

            return sb.ToString();
        }

        //-----------------------------------------------------------------------
    }
}

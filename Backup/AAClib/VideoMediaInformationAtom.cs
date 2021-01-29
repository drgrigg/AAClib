using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class VideoMediaInformationAtom : SimpleAtom //vmhd
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

        private UInt16 myGraphicsMode = 0;

        public UInt16 GraphicsMode
        {
            get { return myGraphicsMode; }
            set { myGraphicsMode = value; }
        }
        private UInt16 myRedColor = 0;

        public UInt16 RedColor
        {
            get { return myRedColor; }
            set { myRedColor = value; }
        }

        private UInt16 myGreenColor = 0;

        public UInt16 GreenColor
        {
            get { return myGreenColor; }
            set { myGreenColor = value; }
        }

        private UInt16 myBlueColor = 0;

        public UInt16 BlueColor
        {
            get { return myBlueColor; }
            set { myBlueColor = value; }
        }


        public VideoMediaInformationAtom()
        {
            TypeString = "vmhd";
        }

        public VideoMediaInformationAtom(FileStream fileReader, long seekpos)
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
            GraphicsMode = Utilities.GetTwoBytes(fileReader, ref seekpos);
            RedColor = Utilities.GetTwoBytes(fileReader, ref seekpos);
            GreenColor = Utilities.GetTwoBytes(fileReader, ref seekpos);
            BlueColor = Utilities.GetTwoBytes(fileReader, ref seekpos);
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
            Utilities.WriteTwoBytes(GraphicsMode, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(RedColor, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(GreenColor, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(BlueColor, fileWriter, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();
            long size = basesize + 1 + 3 + 4 * 2;
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Video Media Information Header|");
            sb.Append("Version:" + Version.ToString() + "|");
            sb.Append("Flags:" + Flags.ToString() + "|");
            sb.Append("GraphicsMode: " + GraphicsMode.ToString() + "|");
            sb.Append("Red:0x" + Convert.ToString(RedColor,16) + ", Green:0x" + Convert.ToString(GreenColor,16) + ", Blue:0x" + Convert.ToString(BlueColor,16) + "|");
            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class TextDescriptionEntry:SampleDescriptionEntry
    {
        private UInt32 myUnknown1 = 1;

        public UInt32 Unknown1
        {
            get { return myUnknown1; }
            set { myUnknown1 = value; }
        }

        private UInt32 myUnknown2 = 1;

        public UInt32 Unknown2
        {
            get { return myUnknown2; }
            set { myUnknown2 = value; }
        }

        private UInt16 myRedColor = 0xFFFF;

        public UInt16 RedColor
        {
            get { return myRedColor; }
            set { myRedColor = value; }
        }

        private UInt16 myGreenColor = 0xFFFF;

        public UInt16 GreenColor
        {
            get { return myGreenColor; }
            set { myGreenColor = value; }
        }

        private UInt16 myBlueColor = 0xFFFF;

        public UInt16 BlueColor
        {
            get { return myBlueColor; }
            set { myBlueColor = value; }
        }

        private UInt64 myUnknown3 = 0;

        public UInt64 Unknown3
        {
            get { return myUnknown3; }
            set { myUnknown3 = value; }
        }

        private UInt32 myUnknown4 = 0;

        public UInt32 Unknown4
        {
            get { return myUnknown4; }
            set { myUnknown4 = value; }
        }

        private UInt16 myUnknown5 = 0x10;

        public UInt16 Unknown5
        {
            get { return myUnknown5; }
            set { myUnknown5 = value; }
        }

        private UInt16 myUnknown6 = 0x0D;

        public UInt16 Unknown6
        {
            get { return myUnknown6; }
            set { myUnknown6 = value; }
        }

        private UInt32 myUnknown7 = 0;

        public UInt32 Unknown7
        {
            get { return myUnknown7; }
            set { myUnknown7 = value; }
        }

        private UInt16 myUnknown8 = 0x0D;

        public UInt16 Unknown8
        {
            get { return myUnknown8; }
            set { myUnknown8 = value; }
        }


        private byte[] myRemainder = new byte[7];

        public TextDescriptionEntry()
        {
            FormatString = "text";
            for (int i = 0; i < myRemainder.Length; i++)
            {
                myRemainder[i] = 0;   
            }
        }

        public TextDescriptionEntry(FileStream fileReader, ref long seekpos)
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
            Unknown1 = Utilities.GetFourBytes(fileReader, ref seekpos);
            Unknown2 = Utilities.GetFourBytes(fileReader, ref seekpos);
            RedColor = Utilities.GetTwoBytes(fileReader, ref seekpos);
            GreenColor = Utilities.GetTwoBytes(fileReader, ref seekpos);
            BlueColor = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Unknown3 = Utilities.GetEightBytes(fileReader, ref seekpos);
            Unknown4 = Utilities.GetFourBytes(fileReader, ref seekpos);
            Unknown5 = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Unknown6 = Utilities.GetTwoBytes(fileReader, ref seekpos);
            Unknown7 = Utilities.GetFourBytes(fileReader, ref seekpos);
            Unknown8 = Utilities.GetTwoBytes(fileReader, ref seekpos);

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
            Utilities.WriteFourBytes(Unknown1, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(Unknown2, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(RedColor, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(GreenColor, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(BlueColor, fileWriter, ref seekpos);
            Utilities.WriteEightBytes(Unknown3, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(Unknown4, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Unknown5, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Unknown6, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(Unknown7, fileWriter, ref seekpos);
            Utilities.WriteTwoBytes(Unknown8, fileWriter, ref seekpos);

            if ((myRemainder != null) && (myRemainder.Length > 0))
            {
                Utilities.WriteBuffer(fileWriter, ref seekpos, myRemainder.Length, ref myRemainder);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            return 59;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("TextDescriptionEntry:");
            sb.Append(" Unknown1:" + Unknown1.ToString());
            sb.Append(" Unknown2:" + Unknown2.ToString());
            sb.Append(" RedColor:" + RedColor.ToString());
            sb.Append(" GreenColor:" + GreenColor.ToString());
            sb.Append(" BlueColor:" + BlueColor.ToString());
            sb.Append(" Unknown3:" + Unknown3.ToString());
            sb.Append(" Unknown4:" + Unknown4.ToString());
            sb.Append(" Unknown5:" + Unknown5.ToString());
            sb.Append(" Unknown6:" + Unknown6.ToString());
            sb.Append(" Unknown7:" + Unknown7.ToString());
            sb.Append(" Unknown8:" + Unknown8.ToString());
            sb.Append("|");
            return sb.ToString();
        }
    }
}

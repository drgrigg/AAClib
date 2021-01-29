using System;
using System.Text;
using System.IO;

namespace AAClib
{
    public class BaseTextAtom : SimpleAtom //'text' inside a gmhd BaseMediaInfoHeader atom.
    {
        //REALLY don't understand the purpose of this - can't find any reference.

        private UInt16 myVersion = 1;

        public UInt16 Version
        {
            get { return myVersion; }
            set { myVersion = value; }
        }

        private UInt64 myUnknown1 = 0;

        public UInt64 Unknown1
        {
            get { return myUnknown1; }
            set { myUnknown1 = value; }
        }
        private UInt64 myUnknown2 = 1;

        public UInt64 Unknown2
        {
            get { return myUnknown2; }
            set { myUnknown2 = value; }
        }
        private UInt64 myUnknown3 = 0;

        public UInt64 Unknown3
        {
            get { return myUnknown3; }
            set { myUnknown3 = value; }
        }
        private UInt64 myUnknown4 = 0x4000;

        public UInt64 Unknown4
        {
            get { return myUnknown4; }
            set { myUnknown4 = value; }
        }

        private byte[] myRemainder = new byte[2];

        public BaseTextAtom()
        {
            TypeString = "text";
            for (int i = 0; i < myRemainder.Length; i++)
            {
                myRemainder[i] = 0;
            }
        }

        public BaseTextAtom(FileStream fileReader, long seekpos)
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
            Unknown1 = Utilities.GetEightBytes(fileReader, ref seekpos);
            Unknown2 = Utilities.GetEightBytes(fileReader, ref seekpos);
            Unknown3 = Utilities.GetEightBytes(fileReader, ref seekpos);
            Unknown4 = Utilities.GetEightBytes(fileReader, ref seekpos);

            long readbytes = seekpos - startseek;
            if (readbytes < GetActualSize()) //then we haven't read all the data, so just capture it.
            {
                long longbytes = GetActualSize() - readbytes;

                int shortbytes = Convert.ToInt32(longbytes);

                myRemainder = new byte[shortbytes];
                Utilities.GetBuffer(fileReader, ref seekpos, shortbytes, ref myRemainder);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mySize = CalculateSize();
            SetActualSize(mySize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos, 16));

            base.WriteData(fileWriter, ref seekpos);


            Utilities.WriteTwoBytes(Version, fileWriter, ref seekpos);
            Utilities.WriteEightBytes(Unknown1, fileWriter, ref seekpos);
            Utilities.WriteEightBytes(Unknown2, fileWriter, ref seekpos);
            Utilities.WriteEightBytes(Unknown3, fileWriter, ref seekpos);
            Utilities.WriteEightBytes(Unknown4, fileWriter, ref seekpos);
            if ((myRemainder != null) && (myRemainder.Length > 0))
            {
                Utilities.WriteBuffer(fileWriter, ref seekpos, myRemainder.Length, ref myRemainder);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            return base.CalculateSize() + 2 + 8 + 8 + 8 + 8 + myRemainder.Length;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Base Text Atom|");
            sb.Append("Version: " + Version.ToString() + "|");
            sb.Append("Unknown1: " + Unknown1.ToString() + "|");
            sb.Append("Unknown2: " + Unknown2.ToString() + "|");
            sb.Append("Unknown3: " + Unknown3.ToString() + "|");
            sb.Append("Unknown4: " + Unknown4.ToString() + "|");
            sb.Append("Remainder: ");
            if ((myRemainder != null) && (myRemainder.Length > 0))
            {
                foreach (byte b in myRemainder)
                {
                    sb.Append(Convert.ToString(b,16) + " ");
                }
            }
            sb.Append("|");


            return sb.ToString();
        }

    }
}

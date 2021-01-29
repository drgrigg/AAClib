using System;
using System.Text;
using System.IO;

namespace AAClib
{
    public class SoundDescriptionEntry:SampleDescriptionEntry
    {

        // in base:

                 //        -> 4 bytes description length = long unsigned length
                 //-> 4 bytes description audio format = long ASCII text string 'mp4a'
                 //  - if encrypted to ISO/IEC 14496-12 or 3GPP standards then use:
                 //-> 4 bytes description audio format = long ASCII text string 'enca'
                 //  - if encoded to 3GPP GSM 6.10 AMR narrowband standards then use:
                 //-> 4 bytes description audio format = long ASCII text string 'samr'
                 //  - if encoded to 3GPP GSM 6.10 AMR wideband standards then use:
                 //-> 4 bytes description audio format = long ASCII text string 'sawb'
                 //-> 6 bytes reserved = 48-bit value set to zero
                 //-> 2 bytes data reference index
                 //    = short unsigned index from 'dref' box


        // in this:

                 //-> 2 bytes QUICKTIME audio encoding version = short hex version
                 //  - default = 0 ; audio data size before decompression = 1
                 //-> 2 bytes QUICKTIME audio encoding revision level
                 //    = byte hex version
                 //  - default = 0 ; video can revise this value
                 //-> 4 bytes QUICKTIME audio encoding vendor
                 //    = long ASCII text string
                 //  - default = 0
                 //-> 2 bytes audio channels = short unsigned count
                 //    (mono = 1 ; stereo = 2)
                 //-> 2 bytes audio sample size = short unsigned value
                 //    (8 or 16)
                 //-> 2 bytes QUICKTIME audio compression id = short integer value
                 //  - default = 0
                 //-> 2 bytes QUICKTIME audio packet size = short value set to zero
                 //-> 4 bytes audio sample rate = long unsigned fixed point rate



        //private UInt16 myVersion = 0;

        //public UInt16 Version
        //{
        //    get { return myVersion; }
        //    set { myVersion = value; }
        //}
        //private UInt16 myRevisionLevel = 0;

        //public UInt16 RevisionLevel
        //{
        //    get { return myRevisionLevel; }
        //    set { myRevisionLevel = value; }
        //}
        //private UInt32 myVendor = 0;

        //public UInt32 Vendor
        //{
        //    get { return myVendor; }
        //    set { myVendor = value; }
        //}


        //private UInt16 myNumberOfChannels = 2;

        //public UInt16 NumberOfChannels
        //{
        //    get { return myNumberOfChannels; }
        //    set { myNumberOfChannels = value; }
        //}

        //private UInt16 mySampleSize = 16;

        //public UInt16 SampleSize
        //{
        //    get { return mySampleSize; }
        //    set { mySampleSize = value; }
        //}


        //private UInt16 myCompressionID = 0;

        //public UInt16 CompressionID
        //{
        //    get { return myCompressionID; }
        //    set { myCompressionID = value; }
        //}

        //private UInt16 myPacketSize = 0;

        //public UInt16 PacketSize
        //{
        //    get { return myPacketSize; }
        //    set { myPacketSize = value; }
        //}

        //private UInt32 mySampleRate = 0;

        //public UInt32 SampleRate
        //{
        //    get { return mySampleRate; }
        //    set { mySampleRate = value; }
        //}

        
        private byte[] myRemainder;


        public SoundDescriptionEntry(string formattype)
        {
            TypeString = formattype;
        }

        public SoundDescriptionEntry(FileStream fileReader, ref long seekpos)
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
            //Version = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //RevisionLevel = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //Vendor = Utilities.GetFourBytes(fileReader, ref seekpos);   ???????????
            //NumberOfChannels = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //SampleSize = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //CompressionID = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //PacketSize = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //SampleRate = Utilities.GetFourBytes(fileReader, ref seekpos);

            long readbytes = seekpos - startseek;
            if (readbytes < Size) //then we haven't read all the data, so just capture it.
            {
                long longremainder = (long)Size - readbytes;
                try
                {
                    int intremainder = Convert.ToInt32(longremainder);
                    myRemainder = new byte[intremainder];
                    Utilities.GetBuffer(fileReader, ref seekpos, intremainder, ref myRemainder);
                }
                catch 
                {
                    throw new InvalidCastException();
                }

            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mySize = CalculateSize();
            SetActualSize(mySize);

            base.WriteData(fileWriter, ref seekpos);

            //Version = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //RevisionLevel = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //Vendor = Utilities.GetFourBytes(fileReader, ref seekpos);   ???????????
            //NumberOfChannels = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //SampleSize = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //CompressionID = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //PacketSize = Utilities.GetTwoBytes(fileReader, ref seekpos);
            //SampleRate = Utilities.GetFourBytes(fileReader, ref seekpos);

            if ((myRemainder != null) && (myRemainder.Length > 0))
            {
                Utilities.WriteBuffer(fileWriter, ref seekpos, myRemainder.Length, ref myRemainder);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            long size = base.CalculateSize();
            return size + myRemainder.Length;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Sound Description|");
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
        //----------------------------------------------------
    }
}

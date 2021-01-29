using System;
using System.IO;

namespace AAClib
{
    /// <summary>
    /// This is the atom which contains all of the media data.  We have to treat it specially, as we're not going to 
    /// load all of the data into memory.  Also need to account for the possibility of a 'wide' expansion extension.
    /// </summary>
    public class MediaDataAtom:SimpleAtom //mdat
    {
        private bool myHasWide = false;

        public bool HasWide
        {
            get { return myHasWide; }
            set { myHasWide = value; }
        }

        private UInt32 myWideSize = 0;
        private UInt32 myWideType = 0;
        private UInt32 myInnerMdatSize = 0;
        private UInt32 myInnerMdatType = 0;

        public MediaDataAtom()
        {
        }

        public MediaDataAtom(FileStream fileReader, long seekpos)
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

            //now have to TEST for presence of wide atom
            long lastseekpos = seekpos;

            myWideSize = Utilities.GetFourBytes(fileReader, ref seekpos);
            myWideType = Utilities.GetFourBytes(fileReader, ref seekpos);
            myInnerMdatSize = Utilities.GetFourBytes(fileReader, ref seekpos);
            myInnerMdatType = Utilities.GetFourBytes(fileReader, ref seekpos);

            if (myWideType == 0x77696465) //'wide'
            {
                if (myInnerMdatType == 0x6D646174) //'mdat'
                {
                    myHasWide = true;
                    DataPos = seekpos;
                }
                else //otherwise, it's a simple atom,reset seekpos
                {
                    seekpos = lastseekpos;
                }
            }
            else //otherwise, it's a simple atom,reset seekpos
            {
                seekpos = lastseekpos;
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            //note that we don't recalculate size here in this atom type.
            base.WriteData(fileWriter, ref seekpos); //just writes header
            if (myHasWide)
            {
                Utilities.WriteFourBytes(myWideSize, fileWriter, ref seekpos);
                Utilities.WriteFourBytes(myWideType, fileWriter, ref seekpos);
                Utilities.WriteFourBytes(myInnerMdatSize, fileWriter, ref seekpos);
                Utilities.WriteFourBytes(myInnerMdatType, fileWriter, ref seekpos);
            }
            //actual media data written by calling routine.
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            //this returns just the HEADER size - actual size set by the calling routine.
            long size = base.CalculateSize();
            if (myHasWide)
                size += 16;
            return size;
        }

        
    }
}

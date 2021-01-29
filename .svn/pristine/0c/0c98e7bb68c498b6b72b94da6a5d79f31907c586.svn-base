using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{

    public class SampleDescriptionAtom : TableAtom //stsd
    {

        private List<SampleDescriptionEntry> myDescList = new List<SampleDescriptionEntry>();


        public SampleDescriptionAtom()
        {
            TypeString = "stsd";
        }
        

        public SampleDescriptionAtom(FileStream fileReader, long seekpos, Utilities.MediaTypes mediatype)
        {
            try
            {
                ReadData(fileReader, ref seekpos, mediatype);
                myDataOK = true;
            }
            catch 
            {
                myDataOK = false;
            }
        }

        public void AddSampleDescription(SampleDescriptionEntry sd)
        {
            myDescList.Add(sd);
        }

        public void RemoveSampleDescription(SampleDescriptionEntry sd)
        {
            myDescList.Remove(sd);
        }

        public SampleDescriptionEntry SampleDescription(int index)
        {
            if (index < myDescList.Count)
                return myDescList[index];
            else
                return null;
        }

        public int SampleListCount
        {
            get { return myDescList.Count; }
        }

        /// <summary>
        /// This ugly version if we don't know media type in advance.
        /// </summary>
        /// <param name="fileReader"></param>
        /// <param name="seekpos"></param>
        protected override void ReadData(FileStream fileReader, ref long seekpos)
        {
            base.ReadData(fileReader, ref seekpos);

            for (int i = 0; i < NumEntries; i++)
            {
                //have to get format string FIRST before determining what we will then need to read.
                long startdesc = seekpos;
                SampleDescriptionEntry sde = new SampleDescriptionEntry(); //note that we don't read it all in at this point.
                sde.Size = Utilities.GetFourBytes(fileReader, ref seekpos);
                sde.Format = Utilities.GetFourBytes(fileReader, ref seekpos); //this is all we are after

                //restore seekpos so we can start again with correct atom type.
                seekpos = startdesc;

                switch (sde.FormatString)
                {

                    case "avc1":


                    case "mp4v":
                    case "png ":
                    case "gif ":
                    case "tiff":
                    case "jpeg":
                    case "raw ":
                        VideoDescriptionEntry vde = new VideoDescriptionEntry(fileReader, ref seekpos);
                        AddSampleDescription(vde);
                        break;

                    case "text":
                        TextDescriptionEntry txe = new TextDescriptionEntry(fileReader, ref seekpos);
                        AddSampleDescription(txe);
                        break;

                    default: //picks up any other formats, assumes they are sound formats.
                        SoundDescriptionEntry sounde = new SoundDescriptionEntry(fileReader, ref seekpos);
                        AddSampleDescription(sounde);
                        break;

                }
            }
        }

        /// <summary>
        /// This version if we already know the media type
        /// </summary>
        /// <param name="fileReader"></param>
        /// <param name="seekpos"></param>
        /// <param name="mediatype"></param>
        public void ReadData(FileStream fileReader, ref long seekpos, Utilities.MediaTypes mediatype)
        {
            if (mediatype == Utilities.MediaTypes.Unknown)
            {
                ReadData(fileReader, ref seekpos);
                return;
            }

            base.ReadData(fileReader, ref seekpos);

            for (int i = 0; i < NumEntries; i++)
            {
                switch (mediatype)
                {
                    case Utilities.MediaTypes.Sound:
                        SoundDescriptionEntry sounde = new SoundDescriptionEntry(fileReader, ref seekpos);
                        AddSampleDescription(sounde);
                        break;

                    case Utilities.MediaTypes.Video:
                        VideoDescriptionEntry vde = new VideoDescriptionEntry(fileReader, ref seekpos);
                        AddSampleDescription(vde);
                        break;

                    case Utilities.MediaTypes.Text:
                        TextDescriptionEntry txe = new TextDescriptionEntry(fileReader, ref seekpos);
                        AddSampleDescription(txe);
                        break;
                }
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);
            foreach (SampleDescriptionEntry sde in myDescList)
            {
                sde.WriteData(fileWriter, ref seekpos);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            NumEntries = (UInt32)myDescList.Count;
            //tricky!
            long desctotal = 0;
            foreach (SampleDescriptionEntry sde in myDescList)
            {
                desctotal += sde.CalculateSize(); //assume it's telling us the truth!!
            }

            long basesize = base.CalculateSize();

            long size = basesize + desctotal;
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Sample Description Atom|");
            sb.Append("NumEntries: " + NumEntries.ToString("#,##0") + "|");
            for (int i = 0; i < Math.Min(NumEntries, 10); i++)
            {
                sb.Append(myDescList[i].ToString() + "|");
            }
            if (NumEntries > 10)
                sb.Append("...[remainder omitted]");

            return sb.ToString();
        }


        public int FindImageDescMatch(UInt32 formatnum, int width, int height, int colorDepth)
        {
            if (myDescList.Count == 0)
                return -1;

            for (int i = 0; i < myDescList.Count; i++)
            {
                try
                {
                    VideoDescriptionEntry vde = (VideoDescriptionEntry)myDescList[i];
                    if (
                            (vde.Format == formatnum)
                            &&
                            (vde.Width == width)
                            &&
                            (vde.Height == height)
                            &&
                            (vde.ColorDepth == colorDepth)
                            )
                        return i;
                }
                catch
                {
                    return -1;
                }
            }
            return -1;
        }
        //---------------------------------------------------------------------------------------
    }
}

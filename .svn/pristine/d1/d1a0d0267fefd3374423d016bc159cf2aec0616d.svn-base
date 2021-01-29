using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class MediaInformationAtom:ContainerAtom
    {
        public MediaInformationAtom()
        {
            IsContainer = true;
            TypeString = "minf";
        }

        public MediaInformationAtom(FileStream fileReader, long seekpos)
        {
            IsContainer = true;

            try
            {
                ReadData(fileReader, ref seekpos); //just calls base.ReadData
                myDataOK = true;
            }
            catch
            {
                myDataOK = false;
            }
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            base.WriteData(fileWriter, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public MediaHeaderAtom GetMediaHeader()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is MediaHeaderAtom)
                    return (MediaHeaderAtom)atom;
            }
            return null;
        }

        public HandlerReferenceAtom GetHandlerReference()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is HandlerReferenceAtom)
                    return (HandlerReferenceAtom)atom;
            }
            return null;
        }

        public DataInformationAtom GetDataInformation()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is DataInformationAtom)
                    return (DataInformationAtom)atom;
            }
            return null;
        }

        public SoundMediaInformationAtom GetSoundMediaInformation()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is SoundMediaInformationAtom)
                    return (SoundMediaInformationAtom)atom;
            }
            return null;
        }

        public VideoMediaInformationAtom GetVideoMediaInformation()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is VideoMediaInformationAtom)
                    return (VideoMediaInformationAtom)atom;
            }
            return null;
        }

        public SampleTableAtom GetSampleTable()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is SampleTableAtom)
                    return (SampleTableAtom)atom;
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() + "Media Information Atom|";
        }
    }
}

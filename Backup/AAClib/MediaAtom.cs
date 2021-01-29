using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class MediaAtom:ContainerAtom //mdia
    {
        public MediaAtom()
        {
            IsContainer = true;
            TypeString = "mdia";
        }

        public MediaAtom(FileStream fileReader, long seekpos)
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

        public MediaInformationAtom GetMediaInformation()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is MediaInformationAtom)
                    return (MediaInformationAtom)atom;
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() + "Media Atom|";
        }
    }
}

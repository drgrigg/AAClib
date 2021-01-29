using System.IO;

namespace AAClib
{

    public class TrackReferenceAtom:ContainerAtom  //tref
    {
        public TrackReferenceAtom()
        {
            IsContainer = true;
            TypeString = "tref";
        }

        public TrackReferenceAtom(FileStream fileReader, long seekpos)
        {
            try
            {
                IsContainer = true;

                ReadData(fileReader, ref seekpos);
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

        public override string ToString()
        {
            return base.ToString() + "Track Reference Atom|";
        }

        public TrackReferenceTypeAtom GetTrackReferenceType(string reftypestring)
        {
            foreach (TrackReferenceTypeAtom treftype in ChildAtoms)
            {
                if (treftype.TypeString == reftypestring)
                    return treftype;
            }
            return null;
        }
    }
}

using System.IO;

namespace AAClib
{
    public class IListAtom : ContainerAtom
    {
        public IListAtom()
        {
            IsContainer = true;
            TypeString = "ilst";
        }

        public IListAtom(FileStream fileReader, long seekpos)
        {
            IsContainer = true;
            TypeString = "ilst";

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

        public override string ToString()
        {
            return base.ToString() + "IList Atom|";
        }

    }
}

using System.IO;

namespace AAClib
{

    public class DataInformationAtom : ContainerAtom  //dinf
    {
        public DataInformationAtom()
        {
            IsContainer = true;
            TypeString = "dinf";
        }

        public DataInformationAtom(FileStream fileReader, long seekpos)
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
            return base.ToString() + "Data Information Atom|";

        }
    }
}

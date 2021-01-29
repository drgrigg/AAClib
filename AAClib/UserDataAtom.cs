using System.IO;

namespace AAClib
{

    public class UserDataAtom : ContainerAtom
    {
        public UserDataAtom()
        {
            IsContainer = true;
            TypeString = "udta";
        }

        public UserDataAtom(FileStream fileReader, long seekpos)
        {
            IsContainer = true;

            try
            {
                base.ReadData(fileReader, ref seekpos); //just calls base.ReadData
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
            return base.ToString() + "|User Data Atom|";
        }

        public static UserDataAtom CreateUserDataAtom()
        {
            var uda = new UserDataAtom();
            var mta = new MetaDataAtom();
            uda.AddChild(mta);
            
            var hdlr = new HandlerReferenceAtom {ComponentSubTypeString = "mdir", ManufacturerString = "appl"};
            mta.AddChild(hdlr);

            var ilst = new IListAtom();
            mta.AddChild(ilst);

            return uda;
        }

        public MetaDataAtom GetMetaDataAtom()
        {
            foreach(var atom in ChildAtoms)
            {
                if (atom.TypeString == "meta")
                {
                    return (MetaDataAtom) atom;
                }
            }
            return null;
        }

    }
}

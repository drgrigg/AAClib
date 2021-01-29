using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class EditAtom:ContainerAtom //edts
    {
        public EditAtom()
        {
            IsContainer = true;
            TypeString = "edts";
        }

        public EditAtom(FileStream fileReader, long seekpos)
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


        public EditListAtom GetEditList()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is EditListAtom)
                    return (EditListAtom)atom;
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() + "Edit Atom|";
        }
    }
}

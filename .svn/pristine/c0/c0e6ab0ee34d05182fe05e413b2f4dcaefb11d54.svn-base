using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{

    public class BaseMediaInfoHeaderAtom:ContainerAtom //gmhd
    {
        public BaseMediaInfoHeaderAtom()
        {
            IsContainer = true;
            TypeString = "gmhd";
        }

        public BaseMediaInfoHeaderAtom(FileStream fileReader, long seekpos)
        {
            IsContainer = true;
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

        public override string ToString()
        {
            return base.ToString() + "Base Media Info Header Atom|";
        }
    }
}

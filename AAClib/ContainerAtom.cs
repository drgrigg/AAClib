using System;
using System.Collections.Generic;
using System.IO;

namespace AAClib
{
    /// <summary>
    /// Container atom has no data of its own (other than size and type) but contains other atoms.
    /// </summary>
    public class ContainerAtom:SimpleAtom
    {
        private List<SimpleAtom> myChildAtoms = new List<SimpleAtom>();

        public List<SimpleAtom> ChildAtoms
        {
            get { return myChildAtoms; }
        }

        public ContainerAtom()
        {
            IsContainer = true;
        }

        public ContainerAtom(FileStream fileReader, long seekpos)
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

        protected override void ReadData(FileStream fileReader, ref long seekpos)
        {
            base.ReadData(fileReader, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);
            foreach (SimpleAtom atom in myChildAtoms)
            {
                atom.WriteData(fileWriter, ref seekpos);
                Utilities.Reporter.DoProcessing(seekpos);
            }
        }

        public void SearchAndDestroy(string atomid)
        {
            for (int i = myChildAtoms.Count -1; i >= 0; i--)
            {
                SimpleAtom atom = myChildAtoms[i];
                if (atom.TypeString == atomid)
                {
                    myChildAtoms.RemoveAt(i);
                }
                else
                {
                    if (atom.IsContainer)
                    {
                        var container = (ContainerAtom) atom;
                        container.SearchAndDestroy(atomid);
                    }
                }
                
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public void AddChild(SimpleAtom atom)
        {
            myChildAtoms.Add(atom);
        }

        public void AddChildAt(int insertPoint, SimpleAtom newChild)
        {
            if ((insertPoint >= 0) && (insertPoint <= myChildAtoms.Count))
                myChildAtoms.Insert(insertPoint, newChild);
        }

        public void RemoveChild(SimpleAtom atom)
        {
            myChildAtoms.Remove(atom);
        }

        public int ChildCount()
        {
            return myChildAtoms.Count;
        }

        public override long CalculateSize()
        {
            long totalSize = 8;

            string atomType = "none";

            foreach (SimpleAtom atom in myChildAtoms)
            {
                atomType = atom.TypeString;
                try
                {
                    totalSize += atom.CalculateSize();
                }
                catch //(Exception ex)
                {
                    throw new SystemException("Error in Calculate Size for " + atomType);
                }
            }
            return totalSize;
        }


    }
}

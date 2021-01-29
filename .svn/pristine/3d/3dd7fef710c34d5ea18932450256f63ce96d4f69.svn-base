using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace AAClib
{
    public class MetaDataAtom:ContainerAtom
    {
        //'meta' is a container with a twist, contains 4 extra bytes

        private UInt32 myFlags = 0;
        public UInt32 Flags
        {
            get { return myFlags; }
            set { myFlags = value; }
        }


        public MetaDataAtom()
        {
            IsContainer = true;
            TypeString = "meta"; 
        }

        public MetaDataAtom(FileStream fileReader, long seekpos)
        {
            IsContainer = true;
            TypeString = "meta"; 

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
            Flags = Utilities.GetFourBytes(fileReader, ref seekpos);
            DataPos += 4;
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            //have to override completely to get in Flags write.

            long mysize = CalculateSize();
            SetActualSize(mysize);

            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos, 16));

            //this is SimpleAtom.WriteData
            Utilities.WriteFourBytes(SimpleSize, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(TypeNum, fileWriter, ref seekpos);

            if (SimpleSize == 1)
            {
                Utilities.WriteEightBytes(ExtendedSize, fileWriter, ref seekpos);
            }

            //and here's the extra data before the list of child atoms
            Utilities.WriteFourBytes(Flags, fileWriter, ref seekpos);

            foreach (SimpleAtom atom in ChildAtoms)
            {
                atom.WriteData(fileWriter, ref seekpos);
                Utilities.Reporter.DoProcessing(seekpos);
            }

            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            return base.CalculateSize() + 4;
        }

        public override string ToString()
        {
            return base.ToString() + "Meta Data Atom|";
        }

        public string GetTitle()
        {
            return GetDataAsString("©nam");
        }

        public void PutTitle(string value)
        {
            PutDataAsString("©nam", value);
        }

        public string GetArtist()
        {
            var tempart = GetDataAsString("©art");
            if (tempart == "")
            {
                tempart = GetDataAsString("©ART");
            }
            return tempart;
        }

        public void PutArtist(string value)
        {
            PutDataAsString("©art", value);
            PutDataAsString("©ART", value);
        }

        public string GetAlbum()
        {
            return GetDataAsString("©alb");
        }

        public void PutAlbum(string value)
        {
            PutDataAsString("©alb", value);
        }

        public string GetGenre()
        {
            return GetDataAsString("©gen");
        }

        public void PutGenre(string value)
        {
            PutDataAsString("©gen", value);
        }


        public Image GetArtwork()
        {
            var theItem = GetMetaDataItem("covr");
            if (theItem != null)
            {
                return theItem.GetImage();
            }
            return null;
        }

        public void PutArtwork(Image anImage)
        {
            var theItem = GetMetaDataItem("covr");
            if (theItem != null)
            {
                theItem.PutImage(anImage);
            }
            else
            {
                theItem = AddMetaDataItem("covr");
                if (theItem != null)
                {
                    theItem.PutImage(anImage);
                }
            }
        }

        public string GetDescription()
        {
            return GetDataAsString("desc");
        }

        public void PutDescription(string value)
        {
            PutDataAsString("desc", value);
        }


        public void ClearArtwork()
        {
            RemoveMetaDataItem("covr");
        }

        public string GetDataAsString(string metastring)
        {
            var theItem = GetMetaDataItem(metastring);
            if (theItem != null)
            {
                return theItem.GetString();
            }
            return "";
        }

        private void PutDataAsString(string typeString, string value)
        {
            var theItem = GetMetaDataItem(typeString);
            if (theItem != null)
            {
                theItem.PutString(value);
            }
            else
            {
                theItem = AddMetaDataItem(typeString);
                if (theItem != null)
                {
                    theItem.PutString(value);
                }
            }
        }


        private MetaDataItem GetMetaDataItem(string typeString)
        {
            foreach(SimpleAtom anAtom in this.ChildAtoms)
            {
                if (anAtom is IListAtom)
                {
                    var ilistatom = (IListAtom) anAtom;
                    foreach (SimpleAtom childatom in ilistatom.ChildAtoms)
                    {
                        if (childatom is MetaDataItem && childatom.TypeString == typeString)
                        {
                            return (MetaDataItem)childatom;
                        }
                    }
                }
            }
            return null;
        }

        private void RemoveMetaDataItem(string typeString)
        {
            SimpleAtom toBeRemoved = null;

            foreach (SimpleAtom anAtom in this.ChildAtoms)
            {
                if (anAtom is IListAtom)
                {
                    var ilistatom = (IListAtom) anAtom;
                    foreach (SimpleAtom childatom in ilistatom.ChildAtoms)
                    {
                        if (childatom is MetaDataItem && childatom.TypeString == typeString)
                        {
                            toBeRemoved = childatom;
                        }
                    }
                    if (toBeRemoved != null)
                    {
                        ilistatom.ChildAtoms.Remove(toBeRemoved);
                    }
                }
            }
        }


        private MetaDataItem AddMetaDataItem(string typeString)
        {
            foreach (SimpleAtom anAtom in this.ChildAtoms)
            {
                if (anAtom is IListAtom)
                {
                    var ilistatom = (IListAtom)anAtom;
                    var mdi = new MetaDataItem { TypeString = typeString};
                    ilistatom.AddChild(mdi);
                    return mdi;
                }
            }
            return null;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace AAClib
{
    public class FileAtom:ContainerAtom
    {
        public string SourceFilePath = "";

        private long myOriginalMovieSize = 0;

        public long OriginalMovieSize
        {
            get { return myOriginalMovieSize; }
            set
            {
                if (value >= 0)
                    myOriginalMovieSize = value;
            }
        }

        private long myOriginalMDATstart = 0;

        public long OriginalMDATstart
        {
            get { return myOriginalMDATstart; }
            set
            {
                if (value >= 0)
                    myOriginalMDATstart = value;
            }
        }

        //this acts as a convenient pointer to any meta data we find.
        private MetaDataAtom myMetaData;

        public MetaDataAtom MetaData
        {
            get { return myMetaData; }
            set { myMetaData = value; }
        }


        public FileAtom()
        {
            IsContainer = true;
            TypeString = "xxxx"; //don't treat this as a normal atom - can't be read as such
            SetActualSize(0xFFFFFFFFFF); //set this large so we don't ever pull it off the stack.
        }

        public MovieAtom GetMovie()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is MovieAtom)
                    return (MovieAtom)atom;
            }
            return null;
        }

        public bool EnsureFreeAfterMovie()
        {
            if (ChildAtoms.Count == 0)
                return false;

            MovieAtom movie = GetMovie();
            if (movie == null)
                return false;

            UnInterpretedAtom freeAtom;

            int indexOfMovie = ChildAtoms.IndexOf(movie);
            if (indexOfMovie >= (ChildAtoms.Count-1)) //it was the last atom
            {
                freeAtom = new UnInterpretedAtom {TypeString = "free"};
                freeAtom.SetActualSize(1024);
                ChildAtoms.Add(freeAtom);
                return true;
            }

            SimpleAtom simple = ChildAtoms[indexOfMovie + 1]; //get atom AFTER movie
            if (simple.TypeString == "free")
            {
                //make sure it's big enough.
                if (simple.CalculateSize() < 1024)
                {
                    simple.SetActualSize(1024);
                }
                return true;
            }

            freeAtom = new UnInterpretedAtom {TypeString = "free"};
            freeAtom.SetActualSize(1024);
            ChildAtoms.Insert(indexOfMovie + 1, freeAtom);
            return true;

        }

        public MediaDataAtom GetMediaData()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is MediaDataAtom)
                    return (MediaDataAtom)atom;
            }
            return null;
        }

        public FileHeaderAtom GetFileHeader()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is FileHeaderAtom)
                    return (FileHeaderAtom)atom;
            }
            return null;
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {

        }

        public override string ToString()
        {
            return base.ToString() + "File Atom|";
        }


        //--------------------------------------------------------------
    }
}

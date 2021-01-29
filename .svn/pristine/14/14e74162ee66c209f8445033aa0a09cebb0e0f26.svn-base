using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class SampleTableAtom:ContainerAtom //stbl
    {
        public SampleTableAtom()
        {
            IsContainer = true;
            TypeString = "stbl";
        }

        public SampleTableAtom(FileStream fileReader, long seekpos)
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

        public SampleDescriptionAtom GetSampleDescriptions()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is SampleDescriptionAtom)
                    return (SampleDescriptionAtom)atom;
            }
            return null;
        }

        public TimeToSampleAtom GetTimeToSamples()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is TimeToSampleAtom)
                    return (TimeToSampleAtom)atom;
            }
            return null;
        }

        public SyncSampleAtom GetSyncSamples()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is SyncSampleAtom)
                    return (SyncSampleAtom)atom;
            }
            return null;
        }

        public SampleSizeAtom GetSampleSizes()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is SampleSizeAtom)
                    return (SampleSizeAtom)atom;
            }
            return null;
        }

        public SampleToChunkAtom GetSampleToChunks()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is SampleToChunkAtom)
                    return (SampleToChunkAtom)atom;
            }
            return null;
        }

        public ChunkOffsetAtom GetChunkOffsets()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is ChunkOffsetAtom)
                    return (ChunkOffsetAtom)atom;
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() + "Sample Table Atom|";
        }
        
    }
}

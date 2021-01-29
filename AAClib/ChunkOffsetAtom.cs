using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class ChunkOffsetAtom:TableAtom
    {

        private System.Collections.Generic.List<UInt32> myChunkOffsets = new List<UInt32>();


        public int OffsetCount
        {
            get { return myChunkOffsets.Count; }
        }

        public void AddChunkOffset(UInt32 offset)
        {
            myChunkOffsets.Add(offset);
        }

        public void RemoveChunkOffset(UInt32 offset)
        {
            myChunkOffsets.Remove(offset);
        }

        public void ClearChunkOffsets()
        {
            myChunkOffsets = new List<UInt32>();
        }

        public void ResetNumEntries()
        {
            NumEntries = Convert.ToUInt32(myChunkOffsets.Count);
        }

        public void AddToOffset(int chunkstart, UInt32 offset)
        {
            if (myChunkOffsets.Count < (chunkstart + 1))
                return;

            for (int i = chunkstart; i < myChunkOffsets.Count; i++)
            {
                myChunkOffsets[i] += offset;
            }
        }

        public void AddToOffset(int chunkstart, long loffset)
        {
            try
            {
                UInt32 offset = Convert.ToUInt32(loffset);
                AddToOffset(chunkstart, offset);
            }
            catch
            {
                //do nowt
            }
        }

        public void SubtractFromOffset(int chunkstart, UInt32 offset)
        {
            if (myChunkOffsets.Count < (chunkstart + 1))
                return;

            for (int i = chunkstart; i < myChunkOffsets.Count; i++)
            {
                if (myChunkOffsets[i] > offset)
                    myChunkOffsets[i] -= offset;
            }
        }

        public void SubtractFromOffset(int chunkstart, long loffset)
        {
            try
            {
                UInt32 offset = Convert.ToUInt32(loffset);
                SubtractFromOffset(chunkstart, offset);
            }
            catch
            {
                //do nowt
            }
        }




        public UInt32 ChunkOffset(int index)
        {
            if (index < myChunkOffsets.Count)
                return myChunkOffsets[index];
            else
                return 0;
        }

        public ChunkOffsetAtom()
        {
            TypeString = "stco";
        }

        public ChunkOffsetAtom(FileStream fileReader, long seekpos)
        {
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

            for (int i = 0; i < NumEntries; i++)
            {
                AddChunkOffset(Utilities.GetFourBytes(fileReader, ref seekpos));
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);
            foreach (UInt32 offset in myChunkOffsets)
            {
                Utilities.WriteFourBytes(offset, fileWriter, ref seekpos);
            }
            Utilities.Reporter.DoProcessing(seekpos);

        }

        public override long CalculateSize()
        {
            try
            {
                if (myChunkOffsets != null)
                    NumEntries = (UInt32)myChunkOffsets.Count;
                else
                {
                    NumEntries = 0;
                }
                long basesize = base.CalculateSize();

                long size = basesize + (NumEntries * 4);
                return size;
            }
            catch
            {
                throw new ArithmeticException(this.GetType().ToString());
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Chunk Offset Atom|");
            sb.Append("NumEntries: " + NumEntries.ToString("#,##0") + "|");
            for (int i = 0; i < Math.Min(NumEntries, 20); i++)
            {
                sb.Append("Offset:" + myChunkOffsets[i].ToString("#,##0") + "|");
            }
            if (NumEntries > 20)
                sb.Append("...[remainder omitted]");

            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class TrackReferenceTypeAtom : SimpleAtom //chap, 
    {
        private List<UInt32> myTrackIDs = new List<UInt32>();
        private UInt32 myNumEntries = 0;

        public UInt32 NumEntries
        {
            get { return myNumEntries; }
            set { myNumEntries = value; }
        }

        public TrackReferenceTypeAtom(string typeString)
        {
            TypeString = typeString;
        }

        public TrackReferenceTypeAtom(FileStream fileReader, long seekpos)
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

        public void AddTrackReference(UInt32 trackID)
        {
            foreach (UInt32 id in myTrackIDs)
            {
                if (id == trackID)
                    //nothing to do
                    return;
            }
            myTrackIDs.Add(trackID);
        }

        public void RemoveTrackReference(UInt32 trackID)
        {
            for (int i = (myTrackIDs.Count-1); i >= 0; i--)
            {
                if (myTrackIDs[i] == trackID)
                {
                    myTrackIDs.RemoveAt(i);
                }
                
            }
        }

        protected override void ReadData(FileStream fileReader, ref long seekpos)
        {
            base.ReadData(fileReader, ref seekpos);
            NumEntries = (SimpleSize - 8) / 4;
            for (int i = 0; i < NumEntries; i++)
            {
                myTrackIDs.Add(Utilities.GetFourBytes(fileReader, ref seekpos));
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mySize = CalculateSize();
            SetActualSize(mySize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);
            foreach (UInt32 trackid in myTrackIDs)
            {
                Utilities.WriteFourBytes(trackid, fileWriter, ref seekpos);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            NumEntries = (UInt32)myTrackIDs.Count;
            long basesize = base.CalculateSize();
            long size = basesize + (NumEntries * 4);
	        return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Track Reference Type: " + TypeString + "|");
            for (int i = 0; i < NumEntries; i++)
            {
                sb.Append("TrackID: " + myTrackIDs[i].ToString("#,##0") + "|");
            }

            return sb.ToString();
        }
    }

}

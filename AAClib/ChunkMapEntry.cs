using System;
using System.Collections.Generic;
using System.Text;

namespace AAClib
{
    public enum ChunkSourceTypes
    {
        None,
        SourceFile,
        ImageIndex,
        TextIndex
    }

    public class ChunkMapEntry : IComparable<ChunkMapEntry>
    {
        /// <summary>
        /// Where we're going to get the data from
        /// </summary>
        private ChunkSourceTypes myChunkSourceType = ChunkSourceTypes.SourceFile;

        public ChunkSourceTypes ChunkSourceType
        {
            get { return myChunkSourceType; }
            set { myChunkSourceType = value; }
        }



        /// <summary>
        /// The time in the media when this chunk plays - expressed in numbers of time units
        /// ???? IN WHAT TIME SCALE ???????
        /// </summary>
        private long myStartTime = 0;

        public long StartTime
        {
            get { return myStartTime; }
            set { myStartTime = value; }
        }

        private long myEndTime = 0;

        public long EndTime
        {
            get { return myEndTime; }
            set { myEndTime = value; }
        }

        /// <summary>
        /// The file index to the file which this chunk is from
        /// </summary>
        private int myfileID = 0;

        public int FileID
        {
            get { return myfileID; }
            set { myfileID = value; }
        }

        /// <summary>
        /// The track which this chunk is from
        /// </summary>
        private UInt32 myTrackID = 0;

        public UInt32 TrackID
        {
            get { return myTrackID; }
            set { myTrackID = value; }
        }

        /// <summary>
        /// The chunk index within the nominated track.  TrackID + Index must be unique.
        /// </summary>
        private int myTrackIndex = 0;

        public int TrackIndex
        {
            get { return myTrackIndex; }
            set { myTrackIndex = value; }
        }

        /// <summary>
        /// Offset into the source file where we'll find this chunk.
        /// </summary>
        private long mySourceOffset = 0;

        public long SourceOffset
        {
            get { return mySourceOffset; }
            set { mySourceOffset = value; }
        }

        /// <summary>
        /// Offset into the new file where we'll put this chunk - we'll sort on this.
        /// </summary>
        private long myDestOffset = 0;

        public long DestOffset
        {
            get { return myDestOffset; }
            set { myDestOffset = value; }
        }

        /// <summary>
        /// Size of the chunk in bytes
        /// </summary>
        private UInt32 myChunkSize = 0;

        public UInt32 ChunkSize
        {
            get { return myChunkSize; }
            set { myChunkSize = value; }
        }

        /// <summary>
        /// The sample number at the beginning of this chunk
        /// </summary>
        private UInt32 myStartSample = 0;

        public UInt32 StartSample
        {
            get { return myStartSample; }
            set { myStartSample = value; }
        }

        /// <summary>
        /// Number of samples in this chunk
        /// </summary>
        private UInt32 myNumSamples = 0;

        public UInt32 NumSamples
        {
            get { return myNumSamples; }
            set { myNumSamples = value; }
        }

        public ChunkMapEntry()
        {
        }

        public ChunkMapEntry(UInt32 trackid, int index, long offset)
        {
            myTrackID = trackid;
            myTrackIndex = index;
            mySourceOffset = offset;
            myDestOffset = offset;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Chunk Map Entry|");
            sb.Append("Type=" + myChunkSourceType.ToString() + "|");
            sb.Append("TrackID=" + myTrackID.ToString() + "|");
            sb.Append("StartTime=" + myStartTime.ToString() + "|");
            sb.Append("SourceOffset=" + mySourceOffset.ToString() + "|");
            sb.Append("DestOffset=" + myDestOffset.ToString() + "|");
            sb.Append("StartSample=" + myStartSample.ToString() + "|");
            sb.Append("NumSamples=" + myNumSamples.ToString() + "|");
            sb.Append("ChunkSize=" + myChunkSize.ToString() + "|");
            return sb.ToString();
        }


        #region IComparable<ChunkMapEntry> Members

        public int CompareTo(ChunkMapEntry other)
        {
            int compareStart = this.DestOffset.CompareTo(other.DestOffset);
            if (compareStart != 0)
                return compareStart;
            else //same offset for both, so compare by trackid
            {
                int compareTrack = this.TrackID.CompareTo(other.TrackID);
                return compareTrack;
            }
        }

        #endregion
    }

    public class CompareByDest : IComparer<ChunkMapEntry>
    {
        public int Compare(ChunkMapEntry firstEntry, ChunkMapEntry secondEntry)
        {
            int compareStart = firstEntry.DestOffset.CompareTo(secondEntry.DestOffset);
            if (compareStart != 0)
                return compareStart;
            else //same offset for both, so compare by trackid
            {
                int compareTrack = firstEntry.TrackID.CompareTo(secondEntry.TrackID);
                return compareTrack;
            }
        }
    }

    public class CompareByStartTime : IComparer<ChunkMapEntry>
    {
        public int Compare(ChunkMapEntry firstEntry, ChunkMapEntry secondEntry)
        {
            int compareStart = firstEntry.StartTime.CompareTo(secondEntry.StartTime);
            if (compareStart != 0)
                return compareStart;
            else //same start time for both, so compare by destination offset
            {
                int compareDest = firstEntry.DestOffset.CompareTo(secondEntry.DestOffset);
                return compareDest;
            }
        }
    }
}

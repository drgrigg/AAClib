using System;
using System.Text;

namespace AAClib
{
    /// <summary>
    /// Stores start times, duration and text for non-sound items.
    /// </summary>
    public class TextTableEntry:IComparable<TextTableEntry>
    {

        public enum CharTypes
        {
            None,
            UTF_8,
            UTF_16_SmallEndian,
            UTF_16_BigEndian
        }

        private CharTypes myCharType = CharTypes.UTF_8;

        public CharTypes CharType
        {
            get { return myCharType; }
            set { myCharType = value; }
        }

        private long myStartTime = 0;

        public long StartTime
        {
            get { return myStartTime; }
            set { myStartTime = value; }
        }

        private string myText = "";

        public string Text
        {
            get { return myText; }
            set { myText = value; }
        }
        private long myDuration = 0;

        public long Duration
        {
            get { return myDuration; }
            set { myDuration = value; }
        }

        private UInt32 myTrackID = 0;

        public UInt32 TrackID
        {
            get { return myTrackID; }
            set { myTrackID = value; }
        }

        public TextTableEntry()
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("TextTableEntry:|");
            sb.Append("StartTime:" + myStartTime.ToString() + "|");
            sb.Append("Text:" + myText + "|");
            sb.Append("Duration:" + myDuration.ToString() + "|");

            return sb.ToString();
        }

        public int DataSize()
        {
            switch (CharType)
            {
                case CharTypes.UTF_8:
                    return TextSize() + 14; //allows for strlen bytes and encode atom

                case CharTypes.UTF_16_BigEndian:
                case CharTypes.UTF_16_SmallEndian:
                    return (TextSize()) + 2 + 14; //allows for strlen bytes, BOM and encode atom

                default:
                    return 0;
            }
        }

        public int TextSize()
        {
            switch (CharType)
            {
                case CharTypes.UTF_8:
                    return Encoding.UTF8.GetByteCount(Text); 

                case CharTypes.UTF_16_BigEndian:
                    return Encoding.BigEndianUnicode.GetByteCount(Text);

                case CharTypes.UTF_16_SmallEndian:
                    return Encoding.Unicode.GetByteCount(Text);

                default:
                    return 0;
            }
        }

        #region IComparable<TextTableEntry> Members

        public int CompareTo(TextTableEntry other)
        {
            return this.StartTime.CompareTo(other.StartTime);
        }

        #endregion
    }
}

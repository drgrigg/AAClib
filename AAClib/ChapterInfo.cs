using System;

namespace AAClib
{
    public class ChapterInfo
    {
        private TimeSpan myStartTime = new TimeSpan(0, 0, 0);

        public TimeSpan StartTime
        {
            get { return myStartTime; }
            set { myStartTime = value; }
        }

        private long myStartTimeInUnits = 0;

        public long StartTimeInUnits
        {
            get { return myStartTimeInUnits; }
            set { myStartTimeInUnits = value; }
        }

        private string myText = "";

        public string Text
        {
            get { return myText; }
            set { myText = value; }
        }

        private string myImagePath = "";

        public string ImagePath
        {
            get { return myImagePath; }
            set { myImagePath = value; }
        }

        private string mySourceImagePath = "";

        public string SourceImagePath
        {
            get { return mySourceImagePath; }
            set { mySourceImagePath = value; }
        }

        public override string ToString()
        {
            return myStartTime.Hours.ToString("00") + ":" +
                myStartTime.Minutes.ToString("00") + ":" +
                myStartTime.Seconds.ToString("00") + "   " + myText;

        }
    }
}

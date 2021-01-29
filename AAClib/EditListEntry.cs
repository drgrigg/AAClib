using System;

namespace AAClib
{
    public class EditListEntry
    {
        public UInt32 TrackDuration = 0;
        public UInt32 MediaTime = 0;
        public UInt32 MediaRate = 1;

        public override string ToString()
        {
            return "Edit List Entry|" + "Duration:" + TrackDuration.ToString() + ", Time:" + MediaTime.ToString() + ", Rate:" + MediaRate.ToString();
        }
    }
}

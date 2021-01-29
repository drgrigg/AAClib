using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class EditListAtom:TableAtom
    {

        private List<EditListEntry> myEditList = new List<EditListEntry>();

        public void AddEditListItem(EditListEntry ele)
        {
            myEditList.Add(ele);
        }

        public void RemoveEditListItem(EditListEntry ele)
        {
            myEditList.Remove(ele);
        }

        public EditListEntry EditListItem(int index)
        {
            if (index < myEditList.Count)
                return myEditList[index];
            else
                return null;
        }

        public int EditListCount
        {
            get { return myEditList.Count; }
        }

        public EditListAtom()
        {
            TypeString = "elst";
        }

        public EditListAtom(FileStream fileReader, long seekpos)
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
                EditListEntry entry = new EditListEntry();

                entry.TrackDuration = Utilities.GetFourBytes(fileReader, ref seekpos);
                entry.MediaTime = Utilities.GetFourBytes(fileReader, ref seekpos);
                entry.MediaRate = Utilities.GetFourBytes(fileReader, ref seekpos);

                AddEditListItem(entry);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);
            foreach(EditListEntry entry in myEditList)
            {
                Utilities.WriteFourBytes(entry.TrackDuration, fileWriter, ref seekpos);
                Utilities.WriteFourBytes(entry.MediaTime, fileWriter, ref seekpos);
                Utilities.WriteFourBytes(entry.MediaRate, fileWriter, ref seekpos);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            NumEntries = (UInt32)myEditList.Count;
            long basesize = base.CalculateSize();

            long size = basesize + (NumEntries * 12);
            return size;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Edit List Atom|");
            sb.Append("NumEntries: " + NumEntries.ToString("#,##0") + "|");
            for (int i = 0; i < Math.Min(NumEntries, 10); i++)
            {
                sb.Append(myEditList[i].ToString() + "|");
            }
            if (NumEntries > 10)
                sb.Append("...[remainder omitted]");

            return sb.ToString();
        }
    }
}

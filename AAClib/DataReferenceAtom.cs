using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class DataReferenceAtom : TableAtom //dref
    {
        private List<DataReferenceEntry> myDataRefs = new List<DataReferenceEntry>();

        public DataReferenceAtom()
        {
            TypeString = "dref";
        }

        public DataReferenceAtom(FileStream fileReader, long seekpos)
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

        public void AddDataReferenceEntry(string typestring)
        {
            DataReferenceEntry dre = new DataReferenceEntry(typestring);
            myDataRefs.Add(dre);
        }

        public void AddDataReferenceEntry(DataReferenceEntry dre)
        {
            myDataRefs.Add(dre);
        }


        public void RemoveDataReferenceEntry(DataReferenceEntry dre)
        {
            myDataRefs.Remove(dre);
        }

        protected override void ReadData(FileStream fileReader, ref long seekpos)
        {
            base.ReadData(fileReader, ref seekpos);
            for (int i = 0; i < NumEntries; i++)
            {
                DataReferenceEntry dre = new DataReferenceEntry(fileReader, ref seekpos);
                myDataRefs.Add(dre);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {

            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);
            foreach (DataReferenceEntry dre in myDataRefs)
            {
                dre.WriteData(fileWriter, ref seekpos);
            }
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            NumEntries = (UInt32)myDataRefs.Count;
            long basesize = base.CalculateSize();

            long size = basesize;
            foreach (DataReferenceEntry dre in myDataRefs)
            {
                size += dre.CalculateSize();  //hope this is telling the truth!
            }
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Data Reference Atom" + "|");
            for (int i = 0; i < NumEntries; i++)
            {
                sb.Append(myDataRefs[i].ToString() + "|");
            }

            return sb.ToString();
        }
    }
}

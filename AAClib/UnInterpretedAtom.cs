using System;
using System.IO;

namespace AAClib
{
    /// <summary>
    /// These are atoms which we don't bother parsing.  We'll just copy across all their data.
    /// This ASSUMES that this block of data is less than 4GB (max UInt32 size) - if it is, we'll throw
    /// an exception.
    /// </summary>
    public class UnInterpretedAtom:SimpleAtom
    {
        protected byte[] myData = null;

        public UnInterpretedAtom()
        {
            myData = new byte[1024];
        }

        public UnInterpretedAtom(FileStream fileReader, long seekpos)
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
            int datalen = (int)SimpleSize - 8; //don't count Size and TypeNum fields
            if (datalen < 0)
            {
                //can't handle ExtendedSize atoms with this type of atom
                throw new System.IO.InvalidDataException();
            }
            myData = new byte[datalen];
            Utilities.GetBuffer(fileReader, ref seekpos, datalen, ref myData);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);
            int datalen = myData.Length;
            Utilities.WriteBuffer(fileWriter, ref seekpos, datalen, ref myData);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override void SetActualSize(long value)
        {
           //only need to do this if we're creating our own uninterpreted atom, eg free atom.
           //have to create the data buffer.

           long basesize = base.CalculateSize();

            base.SetActualSize(value);
            myData = new byte[value - basesize];
        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();

            if (myData == null)
            {
                return basesize;
            }
            long size = basesize + (UInt32)myData.Length;
            return size;
        }

        public override string ToString()
        {
            return base.ToString() + "Un-interpreted: data length: " + myData.Length.ToString("#,##0");
        } 

    }
}

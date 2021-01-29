using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace AAClib
{
    public class MetaDataItem:SimpleAtom
    {

        private UInt32 myDataSize = 0;

        public UInt32 DataSize
        {
            get { return myDataSize; }
            set { myDataSize = value; }
        }

        private UInt32 myDataType = 0;

        public UInt32 DataType
        {
            get { return myDataType; }
            set { myDataType = value; }
        }


        private byte[] myData;

        public MetaDataItem()
        {
            IsContainer = false;

            //dummy stuff, to be overridden.
            myData = new byte[240];
            DataSize = 256;
        }

        public MetaDataItem(FileStream fileReader, long seekpos)
        {
            IsContainer = false;

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
            DataSize = Utilities.GetFourBytes(fileReader, ref seekpos);

            var temp = Utilities.GetFourBytes(fileReader, ref seekpos); //should be 'data'
            DataType = Utilities.GetFourBytes(fileReader, ref seekpos);
            Utilities.GetFourBytes(fileReader, ref seekpos); //ignore, should be null
            int numbytes = Convert.ToInt32(DataSize) - 16;
            myData = new byte[numbytes];
            Utilities.GetBuffer(fileReader, ref seekpos, numbytes, ref myData);

            Utilities.Reporter.DoProcessing(seekpos);

        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long size = CalculateSize();
            SetActualSize(size);

            base.WriteData(fileWriter, ref seekpos);
            Utilities.WriteFourBytes(DataSize, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(Utilities.CharsToUInt32("data"),fileWriter, ref seekpos);
            Utilities.WriteFourBytes(DataType, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(0, fileWriter, ref seekpos);
            int numbytes = Convert.ToInt32(DataSize) - 16;
            Utilities.WriteBuffer(fileWriter, ref seekpos, numbytes, ref myData);

            Utilities.Reporter.DoProcessing(seekpos);
        }

        public override long CalculateSize()
        {
            if (myData == null)
            {
                //this is an error, should throw an exception
                throw new SystemException("No data buffer for " + this.TypeString);
            }

            return base.CalculateSize() + 4 + 4 + 4 + 4 + myData.Length;
        }

        public override string ToString()
        {
            if (DataType == 1)
                return base.ToString() + "MetaDataItem: " + GetString() + "|";
            
            return base.ToString() + "MetaDataItem: DataType=" + DataType.ToString() + "|";
        }

        public string GetString()
        {
            if (DataType != 1)
                return "";

            if (myData.Length > 2)
            {
                if ((myData[0] == 0xFE) && (myData[1] == 0xFF))
                {
                    return Encoding.BigEndianUnicode.GetString(myData, 2, myData.Length);
                }
                if ((myData[0] == 0xFF) && (myData[1] == 0xFE))
                {
                    return Encoding.Unicode.GetString(myData, 2, myData.Length - 2);
                }
            }
            return Encoding.UTF8.GetString(myData, 0, myData.Length);
        }

        public Image GetImage()
        {
            if (DataType != 13 && DataType != 14)
                return null;

            try
            {
                return Image.FromStream(new MemoryStream(myData, 0, myData.Length));
            }
            catch
            {
                return null;
            }
        }

        public void PutImage(Image anImage)
        {
            DataType = 14;

            var ms = new MemoryStream();

            anImage.Save(ms, ImageFormat.Png);

            myData = ms.GetBuffer();
            DataSize = Convert.ToUInt32(myData.Length) + 16;
        }

        public void PutString(string value)
        {
            DataType = 1;

            myData = Encoding.UTF8.GetBytes(value);

            DataSize = Convert.ToUInt32(myData.Length) + 16;
        }

    }
}

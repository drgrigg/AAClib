using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
//    public delegate void ProcessingHandler(long seekpos);

    /// <summary>
    /// Base class for simple atoms or boxes
    /// </summary>
    public class SimpleAtom
    {
  //      public event ProcessingHandler Processing;

        protected bool myDataOK = false;

        public bool DataOK
        {
            get { return myDataOK; }
        }

        private long myLocation = 0;
        private long myDataPos = 8;
        private UInt32 myTypeNum = 0;
        private UInt32 mySimpleSize = 8;
        private UInt64 myExtendedSize = 0;

        //this is used just for pretty print of hierarchy.
        public int Depth = 0;

        //does the atom contain child atoms?
        private bool myIsContainer = false;

        public bool IsContainer
        {
            get { return myIsContainer; }
            set { myIsContainer = value; }
        }

        public SimpleAtom()
        {
            //shouldn't ever directly instantiate a simple atom, so this should never be called.
            TypeString = "....";
        }

        public SimpleAtom(FileStream fileReader, long seekpos)
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

        public int IntSize()
        {
            try
            {
                if (mySimpleSize < int.MaxValue)
                {
                    return Convert.ToInt32(mySimpleSize);
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public long Location
        {
            get { return myLocation; }
            set { if (value >= 0) myLocation = value; }
        }


        public long DataPos
        {
            get { return myDataPos; }
            set { if (value >= 0) myDataPos = value; }
        }

        public UInt32 TypeNum
        {
            get { return myTypeNum; }
            set { myTypeNum = value; }
        }

        /// <summary>
        /// Returns the type as a character string, eg 'moov' or sets it from such a string
        /// </summary>
        public string TypeString
        {
            get {  return Utilities.UInt32ToChars(myTypeNum);  }

            set {
                myTypeNum = Utilities.CharsToUInt32(value);
            }
        }



        public UInt32 SimpleSize
        {
            get { return mySimpleSize; }
            set
            {
                mySimpleSize = value;
            }
        }

        public UInt64 ExtendedSize
        {
            get { return myExtendedSize; }
            set
            {
                if (value > 0)
                {
                    mySimpleSize = 1;
                    myExtendedSize = value;
                }
            }
        }

        /// <summary>
        /// This returns the long value of the actual size based on SimpleSize and ExtendedSize.  Note it doesn't do any recalulation.
        /// </summary>
        /// <returns></returns>
        public long GetActualSize()
        {
            switch (mySimpleSize)
            {
                case 0:
                    return -1;
                case 1:
                    return (long)myExtendedSize;
                default:
                    return (long)mySimpleSize;
            }
        }

        ///// <summary>
        ///// This first recalculates the size which may have changed because of added or deleted children, table entries, etc
        ///// </summary>
        ///// <returns></returns>
        //public long GetRecalculatedSize()
        //{
        //    ReCalculateSize();

        //    return GetActualSize();
        //}

        public virtual void SetActualSize(long value)
        {
            try
            {
                if (value > 0xFFFFFFFF)
                {
                    ExtendedSize = Convert.ToUInt64(value);
                }
                else
                {
                    SimpleSize = Convert.ToUInt32(value);
                }
            }
            catch
            {
                //do nothing
            }
        }

        public long NextLocation()
        {
            return Location + GetActualSize();
        }

        public long LastByte()
        {
            return NextLocation() - 1;
        }


        protected virtual void ReadData(FileStream fileReader, ref long seekpos)
        {
            //read the atom data from the stream.

            Location = seekpos;
            DataPos = seekpos + 8;

            SimpleSize = Utilities.GetFourBytes(fileReader,ref seekpos);
            
            TypeNum = Utilities.GetFourBytes(fileReader,ref seekpos);
            

            if (SimpleSize == 1)
            {
                ExtendedSize = Utilities.GetEightBytes(fileReader,ref seekpos);
                
                DataPos = Location + 16;
            }
            if (SimpleSize == 0)
            {
                long temp = fileReader.Length - seekpos;
                if (temp >= 0)
                {
                    ExtendedSize = Convert.ToUInt32(temp);
                }
                DataPos = Location + 16;
            }
        }




        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SimpleSize:" + SimpleSize.ToString("#,##0") + "|");
            sb.Append("ExtendedSize:" + ExtendedSize.ToString("#,##0") + "|");
            sb.Append("TypeNum:" + TypeString + "|");
            return sb.ToString();
        }

        public virtual void WriteData(FileStream fileWriter, ref long seekpos)
        {
            Utilities.WriteFourBytes(SimpleSize, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(TypeNum, fileWriter, ref seekpos);

            if (SimpleSize == 1)
            {
                Utilities.WriteEightBytes(ExtendedSize, fileWriter, ref seekpos);
            }
        }

        public virtual long CalculateSize()
        {
            long size = 4 + 4;
            if (SimpleSize == 1)
            {
                size += 8;
            }
            return size;
        }



    }
}

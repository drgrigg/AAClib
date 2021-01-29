using System;
using System.Text;
using System.IO;

namespace AAClib
{
    public class HandlerReferenceAtom:SimpleAtom
    {
        private byte myVersion = 0;
        private UInt32 myFlags = 0; //NB: only 3 bytes

        public byte Version
        {
            get { return myVersion; }
            set { myVersion = value; }
        }

        public UInt32 Flags
        {
            get { return myFlags; }
            set { myFlags = value; }
        }

        private UInt32 myComponentType = 0;

        public UInt32 ComponentType
        {
            get { return myComponentType; }
            set { myComponentType = value; }
        }
        private UInt32 myComponentSubType = 0;

        public UInt32 ComponentSubType
        {
            get { return myComponentSubType; }
            set { myComponentSubType = value; }
        }
        private UInt32 myManufacturer = 0;

        public UInt32 Manufacturer
        {
            get { return myManufacturer; }
            set { myManufacturer = value; }
        }
        private UInt32 myComponentFlags = 0;

        public UInt32 ComponentFlags
        {
            get { return myComponentFlags; }
            set { myComponentFlags = value; }
        }
        private UInt32 myComponentMask = 0;

        public UInt32 ComponentMask
        {
            get { return myComponentMask; }
            set { myComponentMask = value; }
        }
        private string myComponentName = "";

        public string ComponentName
        {
            get { return myComponentName; }
            set
            {
                if (value.Length < 255)  //Pascal style string, can't be longer than 255 as only one byte for length indicator.
                    myComponentName = value;
            }
        }

        public string ComponentTypeString
        {
            get { return Utilities.UInt32ToChars(myComponentType); }

            set
            {
                myComponentType = Utilities.CharsToUInt32(value);
            }
        }

        public string ComponentSubTypeString
        {
            get { return Utilities.UInt32ToChars(myComponentSubType); }

            set
            {
                myComponentSubType = Utilities.CharsToUInt32(value);
            }
        }

        public string ManufacturerString
        {
            get { return Utilities.UInt32ToChars(myManufacturer); }

            set
            {
                myManufacturer = Utilities.CharsToUInt32(value);
            }
        }

        public HandlerReferenceAtom()
        {
            TypeString = "hdlr";
        }

        public HandlerReferenceAtom(FileStream fileReader, long seekpos)
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

            Version = Utilities.GetOneByte(fileReader, ref seekpos);
            Flags = Utilities.GetThreeBytes(fileReader, ref seekpos);
            ComponentType = Utilities.GetFourBytes(fileReader, ref seekpos);
            ComponentSubType = Utilities.GetFourBytes(fileReader, ref seekpos);
            Manufacturer = Utilities.GetFourBytes(fileReader, ref seekpos);
            ComponentFlags = Utilities.GetFourBytes(fileReader, ref seekpos);
            ComponentMask = Utilities.GetFourBytes(fileReader, ref seekpos);


            int strlen = Convert.ToInt32(this.LastByte() - seekpos) + 1; 
            //this should be the ACTUAL length of the bytes in the string area

            //read the raw bytes into an array
            byte[] strbytes = new byte[strlen];
            for (int i = 0; i < strlen; i++)
            {
                byte b = Utilities.GetOneByte(fileReader, ref seekpos);

                strbytes[i] = b;
            }

            ComponentName = Utilities.GetStringFromArray(strbytes);

            Utilities.Reporter.DoProcessing(seekpos);
        }



        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            long mysize = CalculateSize();
            SetActualSize(mysize);
            Utilities.Reporter.DoSendReport(ActivityTypes.Writing, seekpos, "Writing " + this.TypeString + " at 0x" + Convert.ToString(seekpos,16));

            base.WriteData(fileWriter, ref seekpos);

            Utilities.WriteOneByte(Version, fileWriter, ref seekpos);
            Utilities.WriteThreeBytes(Flags, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(ComponentType, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(ComponentSubType, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(Manufacturer, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(ComponentFlags, fileWriter, ref seekpos);
            Utilities.WriteFourBytes(ComponentMask, fileWriter, ref seekpos);

            byte strlen = 0;
            try
            {
                //Pascal-type string with length as first byte.
                strlen = Convert.ToByte(ComponentName.Length);
                Utilities.WriteOneByte(strlen, fileWriter, ref seekpos);

                if (strlen > 0)
                {
                    for (int i = 0; i < strlen; i++)
                    {
                        char c = ComponentName[i];
                        Utilities.WriteOneByte((byte)c, fileWriter, ref seekpos);
                    }
                }
                else
                {
                   Utilities.WriteOneByte(0, fileWriter, ref seekpos);  //not sure why this is needed, but...
                }
                Utilities.Reporter.DoProcessing(seekpos);
            }
            catch
            {
                throw new System.ArgumentOutOfRangeException();  //string too long for Pascal style string.
            }
        }

        public override long CalculateSize()
        {
            long basesize = base.CalculateSize();

            long size = basesize + (1 + 3 + 5 * 4 + 1 + ComponentName.Length);
            if (ComponentName.Length == 0)
                size++;  //not sure why this is needed, but...
            return size;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("Version:" + Version.ToString() + "|");
            sb.Append("Flags:" + Flags.ToString() + "|");
            sb.Append("Handler Reference Atom|");
            sb.Append("Type: " + ComponentTypeString + "|");
            sb.Append("SubType: " + ComponentSubTypeString + "|");
            sb.Append("Manufacturer: " + ManufacturerString + "|");
            sb.Append("ComponentName: " + ComponentName + "|");
            sb.Append("ComponentFlags: " + ComponentFlags + "|");
            sb.Append("ComponentMask: " + ComponentMask + "|");
            return sb.ToString();
        }
    }
}

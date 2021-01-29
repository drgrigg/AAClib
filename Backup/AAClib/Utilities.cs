using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class Utilities
    {

        public static string DefaultImage = "";

        public static ReporterEngine Reporter = new ReporterEngine();

        public enum MediaTypes
        {
            Unknown,
            Sound,
            Video,
            Text
        }

        public static string GetAppDataPath()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\ChapterMaster";
        }

        public static Utilities.MediaTypes GetCurrentMediaType(string componentSubTypeString)
        {
            switch (componentSubTypeString)
            {
                case "soun":
                    return Utilities.MediaTypes.Sound;

                case "vide":
                    return Utilities.MediaTypes.Video;

                case "text":
                    return Utilities.MediaTypes.Text;

                default:
                    return Utilities.MediaTypes.Unknown;
            }
        }


        public static void GetBuffer(FileStream fileReader, ref long seekpos, int numbytes, ref byte[] buffer)
        {
            if (numbytes > buffer.Length)
            {
                throw new System.ArgumentOutOfRangeException();
            }
            if (seekpos > (fileReader.Length - numbytes))
            {
                throw new System.IO.EndOfStreamException();
            }
            try
            {
                fileReader.Seek(seekpos, SeekOrigin.Begin);
                fileReader.Read(buffer, 0, numbytes);
                seekpos = seekpos + numbytes;
            }
            catch
            {
                throw;
            }
        }

        public static byte GetOneByte(FileStream fileReader, ref long seekpos)
        {
            byte[] buffer = new byte[1];
            Utilities.GetBuffer(fileReader, ref seekpos, 1, ref buffer);
            return buffer[0];
        }

        public static UInt16 GetTwoBytes(FileStream fileReader, ref long seekpos)
        {
            byte[] buffer = new byte[2];
            Utilities.GetBuffer(fileReader, ref seekpos, 2, ref buffer);
            UInt32 temp = (UInt32)buffer[0] * 256 + (UInt32)buffer[1];
            UInt16 retVal = 0;
            try
            {
                retVal = Convert.ToUInt16(temp);
            }
            catch
            {
                retVal = 0;
            }
            return retVal;
        }

        public static UInt32 GetThreeBytes(FileStream fileReader, ref long seekpos)
        {
            byte[] buffer = new byte[3];
            Utilities.GetBuffer(fileReader, ref seekpos, 3, ref buffer);
            return (UInt32)buffer[0] * 256 * 256 + (UInt32)buffer[1] * 256 + (UInt32)buffer[2];
        }

        public static UInt32 DataGetThreeBytes(byte[] buffer, ref long offset)
        {
            UInt32 retVal = (UInt32)buffer[offset + 0] * 256 * 256 + (UInt32)buffer[offset + 1] * 256 + (UInt32)buffer[offset + 2];
            offset += 3;
            return retVal;
        }

        public static UInt32 GetFourBytes(FileStream fileReader, ref long seekpos)
        {
            byte[] buffer = new byte[4];
            Utilities.GetBuffer(fileReader, ref seekpos, 4, ref buffer);
            return (UInt32)buffer[0] * 256 * 256 * 256 + (UInt32)buffer[1] * 256 * 256 + (UInt32)buffer[2] * 256 + (UInt32)buffer[3];
        }

        public static UInt32 DataGetFourBytes(byte[] buffer, ref int offset)
        {
            UInt32 retVal = (UInt32)buffer[offset] * 256 * 256 * 256 + (UInt32)buffer[offset + 1] * 256 * 256 + (UInt32)buffer[offset+2] * 256 + (UInt32)buffer[offset+3];
            offset += 4;
            return retVal;
        }

        public static UInt64 GetEightBytes(FileStream fileReader, ref long seekpos)
        {
            byte[] buffer = new byte[8];
            Utilities.GetBuffer(fileReader, ref seekpos, 8, ref buffer);
            UInt64 temp0 = (UInt64)buffer[0] << (8 * 7);
            UInt64 temp1 = (UInt64)buffer[1] << (8 * 6);
            UInt64 temp2 = (UInt64)buffer[2] << (8 * 5);
            UInt64 temp3 = (UInt64)buffer[3] << (8 * 4);
            UInt64 temp4 = (UInt64)buffer[4] << (8 * 3);
            UInt64 temp5 = (UInt64)buffer[5] << (8 * 2);
            UInt64 temp6 = (UInt64)buffer[6] << (8 * 1);
            UInt64 temp7 = (UInt64)buffer[7] << (8 * 0);

            return temp0 + temp1 + temp2 + temp3 + temp4 + temp5 + temp6 + temp7;
        }

        public static UInt64 DataGetEightBytes(byte[] buffer, ref long offset)
        {
            UInt64 temp0 = (UInt64)buffer[offset + 0] << (8 * 7);
            UInt64 temp1 = (UInt64)buffer[offset + 1] << (8 * 6);
            UInt64 temp2 = (UInt64)buffer[offset + 2] << (8 * 5);
            UInt64 temp3 = (UInt64)buffer[offset + 3] << (8 * 4);
            UInt64 temp4 = (UInt64)buffer[offset + 4] << (8 * 3);
            UInt64 temp5 = (UInt64)buffer[offset + 5] << (8 * 2);
            UInt64 temp6 = (UInt64)buffer[offset + 6] << (8 * 1);
            UInt64 temp7 = (UInt64)buffer[offset + 7] << (8 * 0);

            offset += 8;
            return temp0 + temp1 + temp2 + temp3 + temp4 + temp5 + temp6 + temp7;
        }



        public static void WriteOneByte(byte value, FileStream fileWriter, ref long seekpos)
        {
            byte[] buffer = new byte[1];
            buffer[0] = value;
            Utilities.WriteBuffer(fileWriter, ref seekpos, 1, ref buffer);
        }

        public static void WriteTwoBytes(UInt16 value, FileStream fileWriter, ref long seekpos)
        {
            byte[] buffer = new byte[2];
            buffer[0] = Convert.ToByte((value & 0xFF00) / 0x100);
            buffer[1] = Convert.ToByte(value & 0xFF);
            Utilities.WriteBuffer(fileWriter, ref seekpos, 2, ref buffer);
        }

        public static void WriteThreeBytes(UInt32 value, FileStream fileWriter, ref long seekpos)
        {
            byte[] buffer = new byte[3];
            buffer[0] = Convert.ToByte((value & 0xFF0000) / 0x10000);
            buffer[1] = Convert.ToByte((value & 0xFF00) / 0x100);
            buffer[2] = Convert.ToByte(value & 0xFF);
            Utilities.WriteBuffer(fileWriter, ref seekpos, 3, ref buffer);
        }

        public static void WriteFourBytes(UInt32 value, FileStream fileWriter, ref long seekpos)
        {
            byte[] buffer = new byte[4];
            buffer[0] = Convert.ToByte((value & 0xFF000000) / 0x1000000);
            buffer[1] = Convert.ToByte((value & 0xFF0000) / 0x10000);
            buffer[2] = Convert.ToByte((value & 0xFF00) / 0x100);
            buffer[3] = Convert.ToByte(value & 0xFF);
            Utilities.WriteBuffer(fileWriter, ref seekpos, 4, ref buffer);
        }

        public static void WriteEightBytes(UInt64 value, FileStream fileWriter, ref long seekpos)
        {
            byte[] buffer = new byte[8];
            buffer[0] = Convert.ToByte((value & 0xFF00000000000000) / 0x100000000000000);
            buffer[1] = Convert.ToByte((value & 0xFF000000000000) / 0x1000000000000);
            buffer[2] = Convert.ToByte((value & 0xFF0000000000) / 0x10000000000);
            buffer[3] = Convert.ToByte((value & 0xFF00000000) / 0x100000000);
            buffer[4] = Convert.ToByte((value & 0xFF000000) / 0x1000000);
            buffer[5] = Convert.ToByte((value & 0xFF0000) / 0x10000);
            buffer[6] = Convert.ToByte((value & 0xFF00) / 0x100);
            buffer[7] = Convert.ToByte(value & 0xFF);
            Utilities.WriteBuffer(fileWriter, ref seekpos, 8, ref buffer);
        }

        public static void WriteBuffer(FileStream fileWriter, ref long seekpos, int numbytes, ref byte[] buffer)
        {
            try
            {
                fileWriter.Seek(seekpos, SeekOrigin.Begin);
                fileWriter.Write(buffer, 0, numbytes);
                seekpos += numbytes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public static UInt32 CharsToUInt32(string value)
        {
            string temp = value;
            while (temp.Length < 4)
                temp += " ";

            return (UInt32)(((byte)temp[0]) * 256 * 256 * 256 + ((byte)temp[1]) * 256 * 256 + ((byte)temp[2]) * 256 + ((byte)temp[3]));
        }

        public static string UInt32ToChars(UInt32 value)
        {
            UInt32 u1, u2, u3, u4;
            char c1, c2, c3, c4;

            u1 = value / (256 * 256 * 256);
            u2 = (value & 16711680) / (256 * 256);
            u3 = (value & 65280) / (256);
            u4 = (value & 255);

            try
            {
                c1 = ConvertToPrintableChar(u1);
                c2 = ConvertToPrintableChar(u2);
                c3 = ConvertToPrintableChar(u3);
                c4 = ConvertToPrintableChar(u4);
            }
            catch
            {
                c1 = ' ';
                c2 = ' ';
                c3 = ' ';
                c4 = ' ';
            }

            return c1.ToString() + c2.ToString() + c3.ToString() + c4.ToString();
        }

        public static char ConvertToPrintableChar(UInt32 u)
        {
            char c;

            if ((u >= 32) && (u <= 127))
                c = Convert.ToChar(u);
            else
            {
                c = '.';
                //allow for some special cases
                switch(u)
                {
                    case 0xA9:
                        c = '©';
                        break;
                }
            }
               
            return c;
        }

        public static string ExtractExtension(string aPath)
        {
            int lastDot;
            lastDot = aPath.LastIndexOf(".");
            return aPath.Substring(lastDot + 1).ToLower();
        }

        /// <summary>
        /// Extract the path from a filename
        /// </summary>
        /// <param name="path">Eg: C:\Program Files\Adobe\Flash\fred.swf</param>
        /// <returns>C:\Program Files\Adobe\Flash</returns>
        public static string ExtractPath(string path)
        {
            try
            {
                int pos = path.LastIndexOf((char)92);
                if (pos >= 0)
                {
                    return path.Substring(0, pos);
                }
                else
                    return path;
            }
            catch (System.Exception ex)
            {
                return "";
            }
        }

        public static string GetStringFromArray(byte[] strbytes)
        {
            //this is a damn nuisance, but SOME audio software (dbPowerAmp)
            //uses C-type rather than Pascal-type strings (which is what the QT standard
            //mandates).  So, we have to be extremely tricky here.

            StringBuilder sb = new StringBuilder();
            int strlen = strbytes.Length;

            //test for type of string by checking for a zero byte
            int charnum = 0;
            bool foundzero = false;
            while ((charnum < strlen) && (!foundzero))
            {
                if (strbytes[charnum] == 0)
                {
                    foundzero = true;
                    strlen = charnum;
                }
                else
                {
                    charnum++;
                }
            }

            if (foundzero) //then it's a C string
            {
                for (int i = 0; i < strlen; i++)
                {
                    byte b = strbytes[i];
                    if (b >= 32)
                    {
                        sb.Append((char)b);
                    }
                }
            }
            else //it's a Pascal string
            {
                for (int i = 1; i < strlen; i++)
                {
                    byte b = strbytes[i];
                    if (b >= 32)
                    {
                        sb.Append((char)b);
                    }
                }
            }
            return sb.ToString();
        }

        //------------------------------------------------------------
    }
}

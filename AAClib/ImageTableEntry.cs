using System;
using System.Text;
using System.Drawing;
using System.IO;

namespace AAClib
{
    public class ImageTableEntry:IComparable<ImageTableEntry>
    {
        private long myStartTime = 0;

        public long StartTime
        {
            get { return myStartTime; }
            set { myStartTime = value; }
        }

        private string myFilePath = "";

        public string FilePath
        {
            get { return myFilePath; }
//            set { myFilePath = value; } //can't set it through this
        }

        private long myFileSize = 0;

        public long FileSize
        {
            get { return myFileSize; }
            // set { myFileSize = value; }
        }

        private long myDuration = 0;

        public long Duration
        {
            get { return myDuration; }
            set { myDuration = value; }
        }


        private int myWidth = 300;

        public int Width
        {
            get { return myWidth; }
            set { myWidth = value; }
        }

        private int myHeight = 300;

        public int Height
        {
            get { return myHeight; }
            set { myHeight = value; }
        }

        private double myHResolution = 72.0;

        public double HResolution
        {
            get { return myHResolution; }
            set { myHResolution = value; }
        }

        private double myVResolution = 72.0;

        public double VResolution
        {
            get { return myVResolution; }
            set { myVResolution = value; }
        }

        private int myColorDepth = 24;

        public int ColorDepth
        {
            get { return myColorDepth; }
            set { myColorDepth = value; }
        }

        private UInt32 myFormatNum = 0;

        public UInt32 FormatNum
        {
            get { return myFormatNum; }
            set { myFormatNum = value; }
        }

        public string FormatString
        {
            get { return Utilities.UInt32ToChars(myFormatNum); }

            set
            {
                myFormatNum = Utilities.CharsToUInt32(value);
            }
        }

        private string myCompressorName = "";

        public string CompressorName
        {
            get { return myCompressorName; }
            set { myCompressorName = value; }
        }

        private UInt32 myTrackID = 0;

        public UInt32 TrackID
        {
            get { return myTrackID; }
            set { myTrackID = value; }
        }


        public ImageTableEntry()
        {
        }

        public bool SetFilePath(string filepath)
        {
            if (!File.Exists(filepath))
            {
                return false;
            }

            try
            {
                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    myFileSize = fs.Length;
                    fs.Close();
                    fs.Dispose();
                }

                myFilePath = filepath;

                //now load it as an image to get further meta data
                using (Image pic = Image.FromFile(filepath))
                {
                    Width = pic.Width;
                    Height = pic.Height;
                    HResolution = pic.HorizontalResolution;
                    VResolution = pic.VerticalResolution;

                    string ext = Utilities.ExtractExtension(filepath);

                    if (ext == "bmp")
                    {
                        FormatString = "bmp ";
                        CompressorName = "BMP";
                    }
                    if (ext == "gif")
                    {
                        FormatString = "gif ";
                        CompressorName = "GIF";
                    }
                    if ((ext == "jpg") || (ext == "jpeg"))
                    {
                        FormatString = "jpeg";
                        CompressorName = "Photo - JPEG";
                    }
                    if (ext == "png")
                    {
                        FormatString = "png ";
                        CompressorName = "PNG";
                    }
                    if (ext == "tif")
                    {
                        FormatString = "tiff";
                        CompressorName = "TIFF";
                    }


                    switch (pic.PixelFormat)
                    {
                        case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                        case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                        case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                        case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                            ColorDepth = 16;
                            break;
                        case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                            ColorDepth = 1;
                            break;
                        case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                            ColorDepth = 24;
                            break;
                        case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                        case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                        case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                            ColorDepth = 32;
                            break;
                        case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                        case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                            ColorDepth = 48;
                            break;
                        case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
                        case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
                            ColorDepth = 64;
                            break;
                        case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                            ColorDepth = 8;
                            break;
                        default:
                            ColorDepth = 24;
                            break;
                    }
                    pic.Dispose();
                }
            }
            catch 
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ImageTableEntry:|");
            sb.Append("StartTime:" + myStartTime.ToString() + "|");
            sb.Append("File:" + myFilePath + "|");
            sb.Append("Duration:" + myDuration.ToString() + "|");

            return sb.ToString();
        }

        #region IComparable<VideoTableEntry> Members

        public int CompareTo(ImageTableEntry other)
        {
            return this.StartTime.CompareTo(other.StartTime);
        }        

        #endregion
    }
}

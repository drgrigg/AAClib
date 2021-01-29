using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace AAClib
{
    public class Globals
    {

        public const string Version = "1.3.1";
        public static string AddRegularPrefix = "Part";
        public static string SourceFile = "";
        public static string DestFile = "";
        public static bool Crop = false;
        public static bool Regular = false;
        public static double RegMins = 10.0;
        public static string ImageFile = "";
        public static string ImportFile = "";
        public static bool Importing = false;
        public static bool Verbose = false;


        private const string LogPath = @"C:\Temp";

        public const uint StandardTimeScale = 2400;

        public static List<string> ImportList;

        public static AACfile.ProcessingPhases CurrentPhase = AACfile.ProcessingPhases.None;


        /// <summary>
        /// Utility routine to return the last token in a delimited string.  Eg, LastToken("Kaleidio.SIF.Messages",'.') returns "Messages".
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="delimiter">Delimiter character</param>
        /// <returns>The last part of the path or delimited string.</returns>
        public static string LastToken(string source, char delimiter)
        {
            try
            {
                if (source.IndexOf(delimiter) == -1)
                    return source; //couldn't find the delimiter so return unchanged

                string[] tokens = source.Split(new char[] { delimiter });
                if (tokens.Length > 0)
                    return tokens[tokens.Length - 1];
                else
                    return source;
            }
            catch //(System.Exception ex)
            {
                return "";
            }
        }

        public static string GetAppDataPath()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\ChapterMaster";
        }

        public static string GetMyPics()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        public static string GetMyMusic()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        }

        public static string GetMyDocs()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        /// <summary>
        /// Extracts the filename (or last folder) from a path.  Includes the file extension.
        /// </summary>
        /// <param name="path">Eg: C:\Program Files\Adobe\Flash</param>
        /// <returns>Eg: Flash</returns>
        public static string ExtractFilename(string path)
        {
            return LastToken(path, (char)92);   //char 92 is backslash
        }

        /// <summary>
        /// Extract the filename (or last folder) from a path.  Optionally includes the file extension.
        /// </summary>
        /// <param name="path">Eg: C:\Program Files\Adobe\Flash\fred.swf</param>
        /// <param name="IncludeExtension">Determines if file extension is included or not</param>
        /// <returns>Eg. "fred" or "fred.swf".</returns>
        public static string ExtractFilename(string path, bool IncludeExtension)
        {
            if (IncludeExtension)
                return ExtractFilename(path);

            else //have to strip off extension
            {
                try
                {
                    string fname = ExtractFilename(path);
                    int dotpos = fname.LastIndexOf('.');
                    if (dotpos >= 0)
                        return fname.Substring(0, dotpos);
                    else
                        return fname;
                }
                catch //(System.Exception ex)
                {
                    return "";
                }
            }

        }

        /// <summary>
        /// Extract just the extension of the file (to test for file type)
        /// </summary>
        /// <param name="path">The path to analyse</param>
        /// <returns>Note that it doesn't include the '.', eg returns 'zip' not '.zip'</returns>
        public static string ExtractExtension(string path)
        {
            string temp = ExtractFilename(path);
            try
            {
                int dotpos = temp.LastIndexOf('.');
                if ((dotpos >= 0) && (dotpos < (temp.Length - 1))) //if it's the last character, we don't want to return anything (and don't want the dot)
                    return temp.Substring(dotpos + 1).ToLower();
                else
                    return "";
            }
            catch //(System.Exception ex)
            {
                return "";
            }
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
            catch //(System.Exception ex)
            {
                return "";
            }
        }

        public static string ChangeExtensionTo(string path, string ext)
        {
            return ExtractPath(path) + @"\" + ExtractFilename(path, false) + ext;
        }

        public static StringBuilder ErrorLog = new StringBuilder();
        public static bool anyErrors = false;

        public const int EvaluationDays = 10;

        public static string MyDocuments()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public static string LocalAppData()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        public static bool WriteAppData(string AppFolder, string Filename, string ToWrite)
        {
            StreamWriter SW;
            string myAppData = LocalAppData();
            if (Directory.Exists(myAppData))
            {
                if (!Directory.Exists(myAppData + @"\" + AppFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(myAppData + @"\" + AppFolder);
                    }
                    catch
                    {
                        //do nowt
                    }
                }
                try
                {
                    SW = File.CreateText(myAppData + @"\" + AppFolder + @"\" + Filename + ".ini");
                    SW.WriteLine(ToWrite);
                    SW.Flush();
                    SW.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static string ReadAppData(string AppFolder, string Filename)
        {
            StreamReader SR;
            string myAppData = LocalAppData();
            try
            {
                string tempStr;
                SR = File.OpenText(myAppData + @"\" + AppFolder + @"\" + Filename + ".ini");
                tempStr = SR.ReadLine();
                SR.Close();
                return tempStr;
            }
            catch
            {
                return "";
            }
        }

        public static bool SaveSetting(string Key, string Value)
        {
            RegistryKey sk;
            RegistryKey regkey = Registry.CurrentUser;
            try
            {
                sk = regkey.CreateSubKey(@"Software\Rightword\ChapterMaster\");
                sk.SetValue(Key, Value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetSetting(string Key)
        {
            RegistryKey sk;
            RegistryKey regkey = Registry.CurrentUser;
            try
            {
                sk = regkey.OpenSubKey(@"Software\Rightword\ChapterMaster\", false);
                return sk.GetValue(Key).ToString();
            }
            catch
            {
                return "";
            }
        }

        public static string ExtractDrive(string aPath)
        {
            string temp;
            if (aPath.Length < 3)
                return "C";
            temp = aPath.Substring(0, 3);
            if (temp.IndexOf(":") > 0)
            {
                return aPath.Substring(0, 1).ToUpper(); //eg "C"
            }
            else
            {
                return "";
            }
        }

        public static string LeadingZero(decimal aNum, int width)
        {
            return aNum.ToString().PadLeft(2, (char)48);
        }

        public static string LeadingZero(int aNum, int width)
        {
            return aNum.ToString().PadLeft(2, (char)48);
        }

        public static string LeadingZero(long aNum, int width)
        {
            return aNum.ToString().PadLeft(2, (char)48);
        }

        public static int Ceiling(float aNum)
        {
            int anInteger;
            anInteger = (int)aNum;
            if ((aNum - anInteger) > 0) anInteger++;
            return anInteger;
        }

        public static bool IsNumeric(string aString)
        {
            if (aString.Length == 0)
                return false;
            char[] tempChars = aString.ToCharArray();
            bool isOK = true;
            foreach (char C in tempChars)
            {
                if ((!Char.IsNumber(C)) && (C != '.'))
                    isOK = false;
            }
            return isOK;
        }

        public static string MakePattern(string aString)
        {
            if (aString.Length == 0)
                return "";

            StringBuilder retString = new StringBuilder();
            char[] tempChars = aString.ToCharArray();
            foreach (char C in tempChars)
            {
                if (Char.IsNumber(C))
                    retString.Append("?"); //question mark
                else
                    retString.Append(C.ToString());
            }
            return retString.ToString();
        }


        // -------------- XML Serialization Routines -------------------------//

        /* following two methods are taken from the Source Project web site:
         * ".NET XML and SOAP Serialization Samples, Tips"
         * By goxman
         * http://www.codeproject.com/soap/Serialization_Samples.asp  
         * 
         * slightly modified by David Grigg */

        /// <summary>
        /// Serializes an object to an XML string
        /// </summary>
        public static string ToXml(object objToXml,
            bool includeNameSpace)
        {
            StreamWriter stWriter = null;
            XmlSerializer xmlSerializer;
            string buffer;
            try
            {
                xmlSerializer =
                    new XmlSerializer(objToXml.GetType());
                MemoryStream memStream = new MemoryStream();
                stWriter = new StreamWriter(memStream);

                if (!includeNameSpace)
                {
                    XmlSerializerNamespaces xs = new XmlSerializerNamespaces();
                    //To remove namespace and any other inline 
                    //information tag                      
                    xs.Add("", "");
                    xmlSerializer.Serialize(stWriter, objToXml, xs);
                }
                else
                {
                    xmlSerializer.Serialize(stWriter, objToXml);
                }
                buffer = Encoding.ASCII.GetString(memStream.GetBuffer());
            }
            catch (System.Exception ex)
            {
                Globals.LogEvent("ToXML", ex.Message);
                return null;
            }
            finally
            {
                if (stWriter != null) stWriter.Close();
            }
            return buffer;
        }

        /// <summary>
        /// Loads a class from an XML string.
        /// </summary>
        public static object FromXml(string xmlString, System.Type ExpectedType)
        {
            XmlSerializer xmlSerializer;
            MemoryStream memStream = null;
            try
            {
                xmlSerializer = new XmlSerializer(ExpectedType);
                byte[] bytes = new byte[xmlString.Length];

                Encoding.ASCII.GetBytes(xmlString, 0, xmlString.Length, bytes, 0);
                memStream = new MemoryStream(bytes);
                object objectFromXml = xmlSerializer.Deserialize(memStream);
                return objectFromXml;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (memStream != null) memStream.Close();
            }
        }

        /// <summary>
        /// Loads a class from an XML file.
        /// </summary>
        public static object FromXmlFile(string filename, System.Type ExpectedType)
        {
            XmlSerializer xmlSerializer;

            if (!File.Exists(filename))
                return null;

            FileStream fStream = null;
            try
            {
                fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                xmlSerializer = new XmlSerializer(ExpectedType);
                object objectFromXml = xmlSerializer.Deserialize(fStream);
                return objectFromXml;
            }
            catch (Exception ex)
            {
                Globals.LogEvent("FromXmlFile:" + filename + ":" + ExpectedType.ToString(), ex.Message);
                return null;
            }
            finally
            {
                if (fStream != null) fStream.Close();
            }
        }


        /// <summary>
        /// Serializes an object to an XML file.
        /// </summary>
        public static bool ToXmlFile(string filename, object objToXml, bool includeNameSpace)
        {
            StreamWriter stWriter = null;
            XmlSerializer xmlSerializer;
            bool retVal = false;
            try
            {
                xmlSerializer =
                    new XmlSerializer(objToXml.GetType());
                FileStream fStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                stWriter = new StreamWriter(fStream);

                if (!includeNameSpace)
                {
                    XmlSerializerNamespaces xs = new XmlSerializerNamespaces();
                    //To remove namespace and any other inline 
                    //information tag                      
                    xs.Add("", "");
                    xmlSerializer.Serialize(stWriter, objToXml, xs);
                }
                else
                {
                    xmlSerializer.Serialize(stWriter, objToXml);
                }
                //Globals.LogEvent("MsgGlobals.ToXMLFile", "Saved options");
                //Globals.SaveLog();
                retVal = true;
            }
            catch (System.Exception ex)
            {
                Globals.LogEvent("MsgGlobals.ToXMLFile", ex.Message);
                Globals.SaveLog();
                retVal = false;
            }
            finally
            {
                if (stWriter != null) stWriter.Close();
            }
            return retVal;
        }

        // -------------- Event Logging Routines -------------------------//
        /// <summary>
        /// StringBuilder log for logging any event within the application.  Each new log event appends to this object.
        /// </summary>
        private static StringBuilder EventLog = new StringBuilder();


        /// <summary>
        /// Log an event identified by method.
        /// </summary>
        public static void LogEvent(string Method, string Message)
        {
            string theLog = DateTime.Today.ToShortDateString() + "|" + DateTime.Now.ToString("hh:mm:ss:ff") + "|" + Method + "|" + Message;

            EventLog.Append(theLog + "\r\n");
            System.Diagnostics.Debug.WriteLine(theLog);
        }

        /// <summary>
        /// Append the log to the given file (clears the log object).
        /// </summary>
        public static void SaveLog(string LogFile)
        {
            if (EventLog.Length > 0)
            {
                try
                {
                    StreamWriter sr = File.AppendText(LogFile);
                    sr.Write(EventLog.ToString());
                    sr.Flush();
                    sr.Close();
                    ClearLog();
                }
                catch
                {
                    //do nowt
                }
            }
        }

        /// <summary>
        /// Append the log to a file with a name based on today's date.
        /// </summary>
        public static void SaveLog()
        {
            DateTime today = DateTime.Today;
            string filename = "ChapterMaster" + "_" + today.Year.ToString("0000") + today.Month.ToString("00") + today.Day.ToString("00") + ".log";
            try
            {
                if (!Directory.Exists(Globals.LogPath))
                {
                    Directory.CreateDirectory(Globals.LogPath);
                }
                SaveLog(Globals.LogPath + @"\" + filename);
            }
            catch
            {
                //do nowt
            }
        }

        /// <summary>
        /// Clear the log object.
        /// </summary>
        public static void ClearLog()
        {
            EventLog.Remove(0, EventLog.Length);
        }

        /// <summary>
        /// Log a generic (not-method identified) event.
        /// </summary>
        public static void LogEvent(string Message)
        {
            LogEvent("Generic", Message);
        }

        public static bool UsesDecimalComma()
        {
            //this is so dumb!  there has to be a built in function to do this!
            decimal test = 5.3333M;
            string teststr = test.ToString(); //this generates culture-specific result
            return (teststr.IndexOf(',') >= 0);
        }

        public static char DecimalPoint()
        {
            if (UsesDecimalComma())
                return ',';
            else
                return '.';
        }

        public static string TimeCode(TimeSpan aTime, int decplaces)
        {
            string temp = aTime.ToString();
            int pointpos = temp.LastIndexOfAny(new char[] { '.', ',' });

            if (decplaces == 0)
            {
                if (pointpos > 0)
                {
                    return temp.Substring(0, pointpos);
                }
                else
                {
                    return temp;
                }
            }
            else
            {
                if (decplaces > 3)
                    decplaces = 3;

                if ((pointpos > 0) && (pointpos < temp.Length))
                {
                    temp += "0000"; //pad for safety
                    return temp.Substring(0, pointpos + decplaces + 1); //should give us one decimal place.
                }
                else
                {
                    return temp + DecimalPoint() + StrRepeat('0',decplaces);
                }
            }
        }

        public static string StrRepeat(char p, int repeats)
        {
            StringBuilder sb = new StringBuilder(repeats);
            for (int i = 0; i < repeats; i++)
            {
                sb.Append(p);
            }
            return sb.ToString();
        }

        //-------------------------------------------------
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TestAAC
{
    public class Global
    {
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
            catch (System.Exception ex)
            {
                return "";
            }
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
                catch (System.Exception ex)
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
            catch (System.Exception ex)
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
            catch (System.Exception ex)
            {
                return "";
            }
        }

        public string ChangeExtensionTo(string path, string ext)
        {
            return ExtractPath(path) + @"\" + ExtractFilename(path, false) + ext;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AAClib
{

    public class ChunkMap
    {
        public List<ChunkMapEntry> Entries = null;

        /// <summary>
        /// List of file sources for audio data
        /// </summary>
        private List<string> myFileTable = new List<string>();

        public List<string> FileTable
        {
            get { return myFileTable; }
        }

        /// <summary>
        /// List of file sources for images to be included
        /// </summary>
        private List<ImageTableEntry> myImageTable = new List<ImageTableEntry>();

        public List<ImageTableEntry> ImageTable
        {
            get { return myImageTable; }
        }

        /// <summary>
        /// List of text strings to be included (eg chapter headings)
        /// </summary>
        private List<TextTableEntry> myTextTable = new List<TextTableEntry>();

        public List<TextTableEntry> TextTable
        {
            get { return myTextTable; }
        }


        public ChunkMap()
        {
            Entries = new List<ChunkMapEntry>();
        }

        public void AddOffsetToDest(int startindex, long offset)
        {
            if (offset <= 0)
                return;

            if ((startindex >= 0) && (startindex < Entries.Count))
            {
                for (int i = startindex; i < Entries.Count; i++)
                {
                    Entries[i].DestOffset += offset;
                }
            }
        }

        public void ShuffleDown(long priorDest, long offset)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].DestOffset >= priorDest)
                      Entries[i].DestOffset += offset;
            }
        }


        public void ShuffleUp(long priorDest, long offset)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].DestOffset >= priorDest)
                    Entries[i].DestOffset -= offset;
            }
        }


        public void SubractOffsetFromDest(int startindex, long offset)
        {
            if (offset <= 0)
                return;

            //if (startindex > 0)
            //{
            //    ChunkMapEntry firstentry = Entries[startindex];
            //    ChunkMapEntry priorentry = Entries[startindex-1];
            //    long priorbyte = priorentry.DestOffset + priorentry.ChunkSize;
            //    if ((firstentry.DestOffset - offset) < priorbyte)
            //    {
            //        throw new InvalidOperationException();
            //    }
            //}

            if ((startindex >= 0) && (startindex < Entries.Count))
            {
                for (int i = startindex; i < Entries.Count; i++)
                {
                    if (offset < Entries[i].DestOffset)
                        Entries[i].DestOffset -= offset;
                }
            }
        }

        //public void CloseGapWithPrevious(int startindex)
        //{
        //    try
        //    {
        //        if ((startindex > 0) && (startindex < Entries.Count))
        //        {
        //            ChunkMapEntry thisentry = Entries[startindex];
        //            ChunkMapEntry priorentry = Entries[startindex - 1];
        //            long priorbyte = priorentry.DestOffset + priorentry.ChunkSize - 1;
        //            long offset = thisentry.DestOffset - priorbyte;
        //            for (int i = startindex; i < Entries.Count; i++)
        //            {
        //                if (offset < Entries[i].DestOffset)
        //                    Entries[i].DestOffset -= offset;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Utilities.Reporter.DoSendReport(ActivityTypes.Error, 0, "CloseGapWithPrevious: " + ex.Message);
        //    }
        //}

        private void AddTextTableEntry(UInt32 trackID, long startTime, string text, TextTableEntry.CharTypes chartype)
        {
            TextTableEntry tte;
            //check for prior entry at same time
            tte = GetTextEntryByStartTime(startTime);
            if (tte == null)
            {
                tte = new TextTableEntry();
                tte.CharType = chartype;
                tte.TrackID = trackID;
                tte.StartTime = startTime;
                tte.Text = text;
                myTextTable.Add(tte);
                myTextTable.Sort();
            }
            else //just amend the existing entry at this time slot.
            {
                tte.Text = text;
                myTextTable.Sort();
            }
        }

        public TextTableEntry GetTextEntryByStartTime(long startTime)
        {
            foreach (TextTableEntry tte in myTextTable)
            {
                if (tte.StartTime == startTime)
                    return tte;
            }
            return null;
        }

        public ImageTableEntry GetImageEntryByStartTime(long startTime)
        {
            foreach (ImageTableEntry ite in myImageTable)
            {
                if (ite.StartTime == startTime)
                    return ite;
            }
            return null;
        }

        public string GetTextByStartTime(long startTime)
        {
            foreach (TextTableEntry tte in myTextTable)
            {
                if (tte.StartTime == startTime)
                    return tte.Text;
            }
            return "";
        }

        public void RemoveTextEntryByStartTime(long starttime)
        {
            if (myTextTable.Count == 0)
                return;

            for (int i = (myTextTable.Count-1); i >= 0; i--)
            {
                TextTableEntry tte = myTextTable[i];
                if (tte.StartTime == starttime)
                    myTextTable.RemoveAt(i);
            }
        }

        private ImageTableEntry AddImageTableEntry(UInt32 trackid, long startTime, string filepath)
        {
            ImageTableEntry ite;
            ite = GetImageEntryByStartTime(startTime);
            if (ite == null)
            {
                ite = new ImageTableEntry();
                ite.TrackID = trackid;
                ite.StartTime = startTime;
                if (ite.SetFilePath(filepath))
                {
                    myImageTable.Add(ite);
                    return ite;
                }
                else
                {
                    return null;
                }
            }
            else //already an entry, just change details
            {
                if (ite.SetFilePath(filepath))
                {
                    return ite;
                }
                else
                {
                    return null;
                }

            }
        }


        public void AddChapterStop(UInt32 trackID, long startTime, string chapterName, TextTableEntry.CharTypes chartype, long totalDuration)
        {
            if (String.IsNullOrEmpty(chapterName))
                chapterName = " "; //don't want empty string as chapter

            try
            {
                //put chapter name into our text table 
                AddTextTableEntry(trackID,startTime,chapterName,chartype);

                //readjust durations of each chapter
                CalculateTextDurations(totalDuration);

            }
            catch (Exception ex)
            {
                Utilities.Reporter.DoSendReport(ActivityTypes.Error, 0, "AddChapterStop: " + ex.Message);
            }

        }


        private void AddTextTableEntry(TextTableEntry tte)
        {
            myTextTable.Add(tte);
        }

        public void UpdateChapterStop(long oldStartTime, long newStartTime, string chapterName, long totalDuration)
        {
            if (String.IsNullOrEmpty(chapterName))
                chapterName = " "; //don't want empty string as chapter


            TextTableEntry tte = GetTextEntryByStartTime(oldStartTime);
            if (tte != null)
            {
                ////check to see if we need to change encoding type
                //if ((tte.CharType == TextTableEntry.CharTypes.UTF_8) && (Needs16bit(chapterName)))
                //{
                //    tte.CharType = TextTableEntry.CharTypes.UTF_16_BigEndian;
                //}
                tte.StartTime = newStartTime;
                tte.Text = chapterName;

                //readjust durations of each chapter
                CalculateTextDurations(totalDuration);
            }
        }

        //private bool Needs16bit(string chapterName)
        //{
        //    for (int i = 0; i < chapterName.Length; i++)
        //    {
        //        char c = chapterName[i];
        //        int intchar = (int)c;
        //        if (intchar > 255)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        public void UpdateImage(long oldStartTime, long newStartTime, long totalDuration)
        {
            ImageTableEntry ite = GetImageEntryByStartTime(oldStartTime);
            if (ite != null)
            {
                ite.StartTime = newStartTime;
                //readjust durations of each image
                CalculateImageDurations(totalDuration);
            }
        }


        //private int AudioEntriesAtStart()
        //{
        //    int count = 0;
        //    if (Entries.Count == 0)
        //        return 0;

        //    while ((count < (Entries.Count-1)) && (Entries[count].ChunkSourceType == ChunkSourceTypes.SourceFile))
        //    {
        //        count++;
        //    }
        //    return count+1;
        //}


        private void PutChapterInChunkMap(TextTableEntry tte)
        {
            //now find the first chunk AFTER the startTime
            int chunkindex = 0;

            ////COMPLETE FUDGE!!!
            ////seems like text entries need to start AFTER some chunks of audio, if possible
            //if (Entries.Count > 6)
            //{
            //    chunkindex = 6; //if possible, start search 6 chunks in.
            //}

            while ((chunkindex < (Entries.Count - 1)) && (Entries[chunkindex].StartTime <= tte.StartTime))
            {
                chunkindex++;
            }
            //now backtrack to previous entry
            if (chunkindex > 0) 
            {
                chunkindex--; //now points to previous entry, if there was one.
            } //else it was the first entry, so we'll keep that.


            ChunkMapEntry prevEntry = Entries[chunkindex];

            ChunkMapEntry newEntry = new ChunkMapEntry(tte.TrackID, 0, prevEntry.DestOffset + prevEntry.ChunkSize);
            newEntry.ChunkSize = (UInt32)tte.DataSize(); 
            newEntry.ChunkSourceType = ChunkSourceTypes.TextIndex;
            newEntry.SourceOffset = 0; //this won't get used
            newEntry.StartTime = tte.StartTime;
            newEntry.NumSamples = 1;
            newEntry.StartSample = 1;

            //                AddOffsetToDest(chunkindex + 1, newEntry.ChunkSize); //shuffle everyone after us down.
            ShuffleDown(newEntry.DestOffset, newEntry.ChunkSize);

            Entries.Insert(chunkindex + 1, newEntry);
        }

        public void RebuildTextSampleTable(TrackAtom texttrack)
        {
            //now we'll rebuild the track TimeToSample list and SampleSize list
            try
            {
                MediaAtom media = texttrack.GetMedia();
                if (media != null)
                {
                    MediaInformationAtom minf = media.GetMediaInformation();
                    if (minf != null)
                    {
                        SampleTableAtom stbl = minf.GetSampleTable();
                        if (stbl != null)
                        {
                            TimeToSampleAtom stts = stbl.GetTimeToSamples();
                            stts.ClearTimeToSamples();
                            foreach (TextTableEntry tte in myTextTable)
                            {
                                UInt32 uDuration = Convert.ToUInt32(tte.Duration);
                                stts.AddTimeToSample(1, uDuration);
                            }

                            SampleSizeAtom stsz = stbl.GetSampleSizes();
                            stsz.ClearSampleSizes();
                            foreach (TextTableEntry tte in myTextTable)
                            {
                                UInt32 uSampleSize = (UInt32)tte.DataSize();
                                stsz.AddSampleSize(uSampleSize);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.Reporter.DoSendReport(ActivityTypes.Error, 0, "RebuildTextSampleTable: " + ex.Message);
            }
        }


        public void CalculateTextDurations(long totalDuration)
        {
            if (myTextTable.Count == 0)
                return; //nothing to do

            myTextTable.Sort();

            TextTableEntry lastTTE = myTextTable[myTextTable.Count - 1];
            long difference = totalDuration - lastTTE.StartTime;

            if (difference < 0)
            {
                Utilities.Reporter.DoSendReport(ActivityTypes.Error, 0, "Duration of track less than start time");
                throw new System.ArgumentOutOfRangeException();
            }
            else
            {
                lastTTE.Duration = difference;
            }


            if (myTextTable.Count > 1)
            {
                for (int i = (myTextTable.Count-2); i >= 0; i--)
                {
                    myTextTable[i].Duration = myTextTable[i + 1].StartTime - myTextTable[i].StartTime;
                }
            }
        }

        public long GetTotalDurationFromTextTable()
        {
            if (myTextTable.Count == 0)
                return 0;

            TextTableEntry tte = myTextTable[myTextTable.Count - 1];
            return tte.StartTime + tte.Duration;
        }

        public void CalculateImageDurations(long totalDuration)
        {
            if (myImageTable.Count == 0)
                return; //nothing to do

            myImageTable.Sort();

            ImageTableEntry lastITE = myImageTable[myImageTable.Count - 1];
            lastITE.Duration = totalDuration - lastITE.StartTime;

            if (myImageTable.Count > 1)
            {
                for (int i = (myImageTable.Count - 2); i >= 0; i--)
                {
                    myImageTable[i].Duration = myImageTable[i + 1].StartTime - myImageTable[i].StartTime;
                }
            }
        }


        public long CalculateTotalSize()
        {
            long totalsize = 0;
            foreach (ChunkMapEntry cme in Entries)
            {
                totalsize += cme.ChunkSize;
            }
            return totalsize;
        }



        //-----------------------------------------------------------------


        public void AddImage(UInt32 trackID, long startTime, string filepath, long totalDuration)
        {
            try
            {
                //put image info into our image table 
                ImageTableEntry ite = AddImageTableEntry(trackID, startTime, filepath);

                if (ite != null) //if it was null, we couldn't open the image file.
                {
                    //readjust durations of each chapter
                    CalculateImageDurations(totalDuration);

                }
                else
                {
                    Utilities.Reporter.DoSendReport(ActivityTypes.Error, 0, "AddImage: Unable to open image file " + filepath);

                }
            }
            catch (Exception ex)
            {
                Utilities.Reporter.DoSendReport(ActivityTypes.Error, 0, "AddImage: " + ex.Message);
            }


        }

        private void PutImageInChunkMap(ImageTableEntry ite)
        {
            //now find the first chunk AFTER the startTime
            int chunkindex = 0;

            ////COMPLETE FUDGE!!!
            ////seems like text entries need to start AFTER some chunks of audio, if possible
            //if (Entries.Count > 6)
            //{
            //    chunkindex = 6; //if possible, start search 6 chunks in.
            //}

            while ((chunkindex < (Entries.Count - 1)) && (Entries[chunkindex].StartTime <= ite.StartTime))
            {
                chunkindex++;
            }

            //now backtrack to previous entry
            if (chunkindex > 0)
            {
                chunkindex--; //now points to previous entry, if there was one.
            } //else it was the first entry, so we'll keep that.

            ChunkMapEntry prevEntry = Entries[chunkindex];

            ChunkMapEntry newEntry = new ChunkMapEntry(ite.TrackID, 0, prevEntry.DestOffset + prevEntry.ChunkSize);
            newEntry.ChunkSize = (UInt32)ite.FileSize;
            newEntry.ChunkSourceType = ChunkSourceTypes.ImageIndex;
            newEntry.SourceOffset = 0; //this won't get used
            newEntry.StartTime = ite.StartTime;
            newEntry.NumSamples = 1;
            newEntry.StartSample = 1;

            ShuffleDown(newEntry.DestOffset, newEntry.ChunkSize);

            Entries.Insert(chunkindex + 1, newEntry);
        }


        public void UpdateChunkMapWithChapters()
        {
            foreach (TextTableEntry tte in TextTable)
            {
                PutChapterInChunkMap(tte);
            }
        }

        public void UpdateChunkMapWithImages()
        {
            foreach (ImageTableEntry ite in ImageTable)
            {
                PutImageInChunkMap(ite);
            }
        }

        public void RebuildVideoSampleTable(TrackAtom videotrack)
        {
            //now we'll rebuild the track TimeToSample list and SampleSize list
            try
            {
                MediaAtom media = videotrack.GetMedia();
                if (media != null)
                {
                    MediaInformationAtom minf = media.GetMediaInformation();
                    if (minf != null)
                    {
                        SampleTableAtom stbl = minf.GetSampleTable();
                        if (stbl != null)
                        {

                            SampleDescriptionAtom stsd = stbl.GetSampleDescriptions();
                            SampleToChunkAtom stsc = stbl.GetSampleToChunks();
                            stsc.ClearSampleToChunkEntries();
                            for (int imgindex = 0; imgindex < myImageTable.Count; imgindex++)
                            {
                                ImageTableEntry ite = myImageTable[imgindex];
                                //we have to go through the description table, seeing if we can match.
                                int descripIndex = stsd.FindImageDescMatch(ite.FormatNum, ite.Width, ite.Height, ite.ColorDepth);
                                if (descripIndex == -1) //we didn't find a match - it's a NEW description, so have to add it.
                                {
                                    VideoDescriptionEntry vde = new VideoDescriptionEntry();
                                    vde.Format = ite.FormatNum;
                                    vde.Width = Convert.ToUInt16(ite.Width);
                                    vde.Height = Convert.ToUInt16(ite.Height);
                                    vde.HResolution = ite.HResolution;
                                    vde.VResolution = ite.VResolution;
                                    vde.ColorDepth = Convert.ToUInt16(ite.ColorDepth);
                                    vde.CompressorName = ite.CompressorName;
                                    stsd.AddSampleDescription(vde);
                                    descripIndex = (int)stsd.NumEntries;
                                    stsd.NumEntries++;
                                }
                                SampleToChunkEntry stce = new SampleToChunkEntry();
                                stce.FirstChunk = (UInt32)imgindex + 1; //change from zero-based to one-based index
                                stce.SampleDescriptionID = (UInt32)descripIndex + 1; //change from zero-based to one-based index
                                stce.SamplesPerChunk = 1;
                                stce.LastChunk = stce.FirstChunk + stce.SamplesPerChunk;
                                stsc.AddSampleToChunkEntry(stce);
                            }

                            stsc.ConsolidateEntries();

                            TimeToSampleAtom stts = stbl.GetTimeToSamples();
                            stts.ClearTimeToSamples();
                            foreach (ImageTableEntry ite in myImageTable)
                            {
                                UInt32 uDuration = Convert.ToUInt32(ite.Duration);
                                stts.AddTimeToSample(1, uDuration);
                            }

                            SampleSizeAtom stsz = stbl.GetSampleSizes();
                            stsz.ClearSampleSizes();
                            foreach (ImageTableEntry ite in myImageTable)
                            {
                                UInt32 uSampleSize = (UInt32)ite.FileSize;
                                stsz.AddSampleSize(uSampleSize);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.Reporter.DoSendReport(ActivityTypes.Error, 0, "RebuildVideoSampleTable: " + ex.Message);
            }
        }

        public string GetImagePathByStartTime(long startTime)
        {
            foreach (ImageTableEntry ite in myImageTable)
            {
                if (ite.StartTime == startTime)
                    return ite.FilePath;
            }
            return "";
        }

        //public void DumpEntries(string comment)
        //{
        //    Utilities.SaveLog();
        //    SortByDestination();
        //    Utilities.LogEvent("-----------------------" + comment + "------------------------");
        //    foreach (ChunkMapEntry cme in Entries)
        //    {
        //        Utilities.LogEvent(cme.ToString());
        //    }
        //    Utilities.SaveLog();
        //}

        public void SortByStartTime()
        {
            Entries.Sort(new CompareByStartTime());
        }

        public void SortByDestination()
        {
            Entries.Sort(new CompareByDest());
        }
        //---------------------------------------------------------------------------

        public long GetTotalDurationFromImageTable()
        {
            if (myImageTable.Count == 0)
                return 0;

            ImageTableEntry ite = myImageTable[myImageTable.Count - 1];
            return ite.StartTime + ite.Duration;
        }
    }
}

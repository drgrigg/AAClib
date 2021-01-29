using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AAClib
{
    public class TrackAtom:ContainerAtom //trak
    {

        public TrackAtom()
        {
            IsContainer = true;
            TypeString = "trak";
        }


        public TrackAtom(FileStream fileReader, long seekpos)
        {
            IsContainer = true;

            try
            {
                ReadData(fileReader, ref seekpos); //just calls base.ReadData
                myDataOK = true;
            }
            catch
            {
                myDataOK = false;
            }
        }

        public override void WriteData(FileStream fileWriter, ref long seekpos)
        {
            base.WriteData(fileWriter, ref seekpos);
            Utilities.Reporter.DoProcessing(seekpos);
        }

        public bool Enabled
        {
            get
            {
                TrackHeaderAtom trackHeader = GetTrackHeader();
                if (trackHeader == null)
                    return false;
                else
                {
                    return ((trackHeader.Flags & 1) == 1);
                }
            }
            set
            {
                TrackHeaderAtom trackHeader = GetTrackHeader();
                if (trackHeader == null)
                    return;
                if (value)
                {
                    trackHeader.Flags = (trackHeader.Flags | 1);
                }
                else
                {
                    trackHeader.Flags = (trackHeader.Flags & 0xFE);
                }
            }
        }

        public TrackHeaderAtom GetTrackHeader()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is TrackHeaderAtom)
                    return (TrackHeaderAtom)atom;
            }
            return null;
        }

        public UInt32 GetTrackID()
        {
            TrackHeaderAtom TrackHeader = GetTrackHeader();
            if (TrackHeader != null)
                return TrackHeader.TrackID;
            else
                return 0;
        }

        public EditAtom GetEditAtom()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is EditAtom)
                    return (EditAtom)atom;
            }
            return null;
        }

        public TrackReferenceAtom GetTrackReference()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is TrackReferenceAtom)
                    return (TrackReferenceAtom)atom;
            }
            return null;
        }

        public MediaAtom GetMedia()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is MediaAtom)
                    return (MediaAtom)atom;
            }
            return null;
        }

        public Utilities.MediaTypes GetTrackType()
        {
            MediaAtom media = GetMedia();
            if (media != null)
            {
                HandlerReferenceAtom handler = media.GetHandlerReference();
                if (handler != null)
                {
                    switch (handler.ComponentSubTypeString)
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
                else
                    return Utilities.MediaTypes.Unknown;
            }
            else
                return Utilities.MediaTypes.Unknown;
        }

        public override string ToString()
        {
            return base.ToString() + "Track Atom|";
        }


        public UInt32 GetSampleDuration()
        {
            UInt32 sampleduration = 0;
            MediaAtom media = GetMedia();
            if (media != null)
            {
                MediaInformationAtom mediaInfo = media.GetMediaInformation();
                if (mediaInfo != null)
                {
                    SampleTableAtom sampleTable = mediaInfo.GetSampleTable();
                    if (sampleTable != null)
                    {
                        TimeToSampleAtom timeToSamples = sampleTable.GetTimeToSamples();
                        if ((timeToSamples != null) && (timeToSamples.TimeToSampleCount > 0))
                        {
                            //we ASSUME the sample duration is constant
                            //and there's therefore only one entry in the TimeToSample table
                            //- otherwise this would become very messy.
                            sampleduration = timeToSamples.TimeToSample(0).SampleDuration;
                        }
                    }
                }
            }
            return sampleduration;
        }

        public UInt32 GetTimeUnitsPerSecond()
        {
            UInt32 unitspersecond = 0;

            MediaAtom media = GetMedia();
            if (media != null)
            {
                MediaHeaderAtom mhead = media.GetMediaHeader();
                if (mhead != null)
                {
                    unitspersecond = mhead.TimeUnitsPerSecond;
                }
            }
            return unitspersecond;
        }

        public UInt32 GetDurationInTimeUnits()
        {
            UInt32 timeunits = 0;

            MediaAtom media = GetMedia();
            if (media != null)
            {
                MediaHeaderAtom mhead = media.GetMediaHeader();
                if (mhead != null)
                {
                    timeunits = mhead.DurationInTimeUnits;
                }
            }
            return timeunits;
        }

        public void SetTrackHeaderDurationInTimeUnits(UInt32 duration)
        {
            UInt32 timeunits = 0;

            TrackHeaderAtom tkhd = GetTrackHeader();

            if (tkhd != null)
            {
                tkhd.Duration = duration;
            }
        }

        public void AddToChunkOffset(int chunkstart, long offset)
        {
            MediaAtom mdia = GetMedia();
            if (mdia != null)
            {
                MediaInformationAtom minf = mdia.GetMediaInformation();
                if (minf != null)
                {
                    SampleTableAtom stbl = minf.GetSampleTable();
                    if (stbl != null)
                    {
                        ChunkOffsetAtom stco = stbl.GetChunkOffsets();
                        if (stco != null)
                            stco.AddToOffset(chunkstart, offset);
                    }
                }
            }
        }

        public void SubtractFromChunkOffset(int chunkstart, long offset)
        {
            MediaAtom mdia = GetMedia();
            if (mdia != null)
            {
                MediaInformationAtom minf = mdia.GetMediaInformation();
                if (minf != null)
                {
                    SampleTableAtom stbl = minf.GetSampleTable();
                    if (stbl != null)
                    {
                        ChunkOffsetAtom stco = stbl.GetChunkOffsets();
                        if (stco != null)
                            stco.SubtractFromOffset(chunkstart, offset);
                    }
                }
            }
        }


        public ChunkOffsetAtom GetChunkOffsets()
        {
            MediaAtom media = GetMedia();
            if (media != null)
            {
                MediaInformationAtom minf = media.GetMediaInformation();
                if (minf != null)
                {
                    SampleTableAtom stbl = minf.GetSampleTable();
                    if (stbl != null)
                    {
                        return stbl.GetChunkOffsets();
                    }
                }
            }
            return null;
        }

        //-------------------------------------------------------------------------------

        public void SetTimeUnitsPerSecond(UInt32 unitspersecond)
        {
            MediaAtom media = GetMedia();
            if (media != null)
            {
                MediaHeaderAtom mhead = media.GetMediaHeader();
                if (mhead != null)
                {
                    mhead.TimeUnitsPerSecond = unitspersecond;
                }
            }
        }

        public void SetDurationInTimeUnits(uint timeunits)
        {
            MediaAtom media = GetMedia();
            if (media != null)
            {
                MediaHeaderAtom mhead = media.GetMediaHeader();
                if (mhead != null)
                {
                    mhead.DurationInTimeUnits = timeunits;
                }
            }
        }
    }
}

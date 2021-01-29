using System;
using System.Collections.Generic;
using System.IO;

namespace AAClib
{
    public class MovieAtom : ContainerAtom // moov
    {
        public MovieAtom()
        {
            IsContainer = true;
            TypeString = "moov";
        }

        public MovieAtom(FileStream fileReader, long seekpos)
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

        public override string ToString()
        {
            return base.ToString() + "Movie Atom|";
        }

        public MovieHeaderAtom GetMovieHeader()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is MovieHeaderAtom)
                    return (MovieHeaderAtom)atom;
            }
            return null;
        }

        public TrackAtom GetTrackAtomByID(UInt32 trackID)
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is TrackAtom)
                {
                    TrackAtom track = (TrackAtom)atom;
                    if (track.GetTrackID() == trackID)
                        return track;
                }
            }
            return null;
        }

        public TrackAtom GetTrackAtomByIndex(int index)
        {
            if (index < 0)
                return null;

            List<TrackAtom> tracks = GetTracks();
            if (index < tracks.Count)
                return tracks[index];
            return null;
        }

        public int GetTrackCount()
        {
            int trackCount = 0;
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is TrackAtom)
                {
                    trackCount++;
                }
            }
            return trackCount;
        }

        public List<TrackAtom> GetTracks()
        {
            List<TrackAtom> tracks = new List<TrackAtom>();
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is TrackAtom)
                {
                    tracks.Add((TrackAtom)atom);
                }
            }
            return tracks;
        }

        public TrackAtom GetAudioTrack()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is TrackAtom)
                {
                    TrackAtom track = (TrackAtom)atom;
                    if (track.GetTrackType() == Utilities.MediaTypes.Sound)
                        return track;
                }
            }
            return null;
        }

        public TrackAtom GetTextTrack()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is TrackAtom)
                {
                    TrackAtom track = (TrackAtom)atom;
                    if (track.GetTrackType() == Utilities.MediaTypes.Text)
                        return track;
                }
            }
            return null;
        }

        public TrackAtom GetVideoTrack()
        {
            foreach (SimpleAtom atom in ChildAtoms)
            {
                if (atom is TrackAtom)
                {
                    TrackAtom track = (TrackAtom)atom;
                    if (track.GetTrackType() == Utilities.MediaTypes.Video)
                        return track;
                }
            }
            return null;
        }

        public void RemoveVideoTracks()
        {
            for (int i = ChildAtoms.Count-1; i >= 0; i--)
            {
                SimpleAtom atom = ChildAtoms[i];
                if (atom is TrackAtom)
                {
                    var track = (TrackAtom)atom;
                    if (track.GetTrackType() == Utilities.MediaTypes.Video)
                    {
                        ChildAtoms.RemoveAt(i);
                        var mvhd = GetMovieHeader();
                        if (mvhd.NextTrackID > 1)
                            mvhd.NextTrackID--;
                    }
                }
            }
        }

        public void RemoveTextTracks()
        {
            for (int i = ChildAtoms.Count-1; i >= 0; i--)
            {
                SimpleAtom atom = ChildAtoms[i];
                if (atom is TrackAtom)
                {
                    var track = (TrackAtom)atom;
                    if (track.GetTrackType() == Utilities.MediaTypes.Text)
                    {
                        ChildAtoms.RemoveAt(i);
                        var mvhd = GetMovieHeader();
                        if (mvhd.NextTrackID > 1)
                            mvhd.NextTrackID--;
                    }
                }
            }
        }


//----------------------------------------------------------------------
    }
}

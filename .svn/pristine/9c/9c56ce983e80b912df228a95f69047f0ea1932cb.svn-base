using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace AAClib
{
    public delegate void ProcessingProgressHandler(double percentage, AACfile.ProcessingPhases phase);
    public delegate void ReportErrorHandler(string message);
    public delegate void MessageHandler(string activity, string message);

    public class AACfile:IDisposable
    {
        public event ProcessingProgressHandler ProcessingProgress;
        public event ReportErrorHandler ReportError;
        public event MessageHandler ReportMessage;

        private readonly FileAtom rootAtom = new FileAtom(); //this is used as the root container.

        protected ChunkMap myChunkMap = null;

        public bool InterpretMetaData = false;  //need to explictly set this to true to 'burrow in' to udta atom
        public int Level = 0;

        public string LastError = "No Error";

        public static int ImageNum = 0;
        //this determines what we think is a chapter time 'too close to the end' of a file
        public const double TooNearEndInSecs = 2.0;

        private FileStream fileReader = null;
        private FileStream fileWriter = null;

        private string mySourcePath = "";

        public string SourcePath
        {
            get { return mySourcePath; }
            set 
            { 
                if (rootAtom != null)
                {
                    rootAtom.SourceFilePath = value;
                    mySourcePath = value;
                }
            }
        }
        private string myDestPath = "";

        public string DestPath
        {
            get { return myDestPath; }
            set { myDestPath = value; }
        }

        private bool myNeedsSave = false;

        public bool NeedsSave
        {
            get { return myNeedsSave; }
            set { myNeedsSave = value; }
        }

        protected MovieAtom Movie
        {
            get { return (rootAtom!=null) ? rootAtom.GetMovie() : null; }
        }
        public bool LoadedOK = false;


        //private SimpleAtom currentAtom = null;
//        private readonly Stack<SimpleAtom> atomStack = new Stack<SimpleAtom>();

        private readonly ReporterEngine myReporter;

        private long myFileLength = 0;

        public AACfile()
        {
            myReporter = Utilities.Reporter;
            myReporter.SendReport += myReporter_ReportProgress;
            myReporter.Processing += myReporter_Processing;
        }


        public void ReadFile(string filename)
        {
            ReadFile(filename,true);
        }

        public void ReadFile(string filename, bool ImportExisting)
        {
            if (!File.Exists(filename))
            {
                LoadedOK = false;
                //throw new System.IO.FileNotFoundException();
                return;
            }

            try
            {
                using (fileReader = new FileStream(filename, FileMode.Open))
                {
                    myFileLength = fileReader.Length;
                    rootAtom.SourceFilePath = filename;

                    ReadAllAtoms();

                    AddSourceFileToChunkMap(filename);
                    fileReader.Close();
                    fileReader.Dispose();

                }

                LoadedOK = true;
                //following may set LoadedOK to false
                if (ImportExisting)
                {
                    BuildTextTableFromFile();
                    BuildImageTableFromFile();
                }

                //these will get changed so they are different before a save
                mySourcePath = filename;
                myDestPath = filename;
                myNeedsSave = false;
            }
            catch (System.Exception ex)
            {
                LoadedOK = false;
                mySourcePath = "";
                myDestPath = "";
                myNeedsSave = false;
                throw ex;
            }
        }




        public void RemoveAllTracksExceptAudio()
        {
            if (LoadedOK)
            {
                MovieAtom movie = Movie;
                movie.RemoveVideoTracks();
                movie.RemoveTextTracks();
            }
        }

        void myReporter_Processing(long currentpos)
        {
            if (myFileLength > 0)
            {
               double percent = (100 * currentpos) / myFileLength;
               ProcessingProgress(percent, Globals.CurrentPhase);
            }
        }

        void myReporter_ReportProgress(ActivityTypes activity, long currentpos, string message)
        {
            if (ReportingOn)
            {
                switch (activity)
                {
                    case ActivityTypes.Error:
                        ReportError(message);
                        break;
                    case ActivityTypes.Reading:
                        ReportMessage("Reading", message);
                        break;
                    case ActivityTypes.Writing:
                        ReportMessage("Writing", message);
                        break;
                    case ActivityTypes.Processing:
                        ReportMessage("Processing", message);
                        break;
                } 
            }
        }

        public void CloseFileReader()
        {
            if (fileReader != null)
            {
                fileReader.Close();
                fileReader.Dispose();
                fileReader = null;
            }
        }

        public void CloseFileWriter()
        {
            if (fileWriter != null)
            {
                fileWriter.Close();
                fileWriter.Dispose();
                fileWriter = null;
            }
        }

        private void GetAllAtoms()
        {

            //put the file atom container on the stack to contain all other atoms.
            ContainerAtom fileContainer = rootAtom;
            fileContainer.DataPos = 0;
            ReadChildren(fileContainer);

            return;// fileContainer.ChildAtoms[0];
        }

       public bool ReportingOn = false;


        private SimpleAtom GetAtomAt(ref long nextReadLocation, ContainerAtom parent)
        {
            try
            {
                SimpleAtom atom = GetAtomHeader(nextReadLocation);
                if (atom == null)
                {
                    return null;
                }

                SimpleAtom qualifiedAtom = QualifyAtom(atom, parent);
                if (qualifiedAtom == null)
                {
                    return null;
                }

                //have to test UserDataAtom to see if it's a valid iTunes meta data container
                if (InterpretMetaData)
                {
                    if (qualifiedAtom is UserDataAtom)
                    {
                        qualifiedAtom = CheckValidMetaDataAtom(qualifiedAtom);
                    }
                    if (qualifiedAtom is MetaDataAtom)
                    {
                        var metaAtom = (MetaDataAtom)qualifiedAtom;
                        rootAtom.MetaData = metaAtom;
                    }
                }

                DoReporting(qualifiedAtom);

                nextReadLocation = qualifiedAtom.IsContainer ? qualifiedAtom.DataPos : qualifiedAtom.NextLocation();

                return qualifiedAtom;
            }
            catch (Exception ex)
            {
                ReportError("GetAtomAt: " + ex.Message);
                return null;
            }
        }


        private void ReadChildren(ContainerAtom parent) //parent is the container atom we are parsing
        {
            var nextReadLocation = parent.DataPos;

            while ((nextReadLocation < fileReader.Length) //don't read past end of file
                && (nextReadLocation < parent.NextLocation()) //don't read past end of container
                && (nextReadLocation >= parent.DataPos)) //don't go backwards
            {
                SimpleAtom atom = GetAtomHeader(nextReadLocation);
                if (atom == null)
                {
                    return;
                }

                SimpleAtom qualifiedAtom = QualifyAtom(atom, parent);
                if (qualifiedAtom == null)
                {
                    return;
                }

                //have to test UserDataAtom to see if it's a valid iTunes meta data container, if not it gets replaced by UninterpretedAtom

                if (InterpretMetaData)
                {
                    if (qualifiedAtom is UserDataAtom)
                    {
                        qualifiedAtom = CheckValidMetaDataAtom(qualifiedAtom);
                    }
                    if (qualifiedAtom is MetaDataAtom)
                    {
                        var metaAtom = (MetaDataAtom)qualifiedAtom;
                        rootAtom.MetaData = metaAtom;
                    }
                }

                parent.AddChild(qualifiedAtom);
                qualifiedAtom.Depth = parent.Depth + 1;

                Level = qualifiedAtom.Depth;

                DoReporting(qualifiedAtom);

                if (qualifiedAtom.IsContainer)
                {
                    ReadChildren((ContainerAtom) qualifiedAtom); //recursive call
                }
                nextReadLocation = qualifiedAtom.NextLocation();
            }
        }

        private void DoReporting(SimpleAtom qualifiedAtom)
        {
            if (ReportingOn)
            {
                ReportMessage("Reading", "---------------------------------------");
                ReportMessage("Reading",
                              "ATOM: '" + qualifiedAtom.TypeString + "' at 0x" +
                              Convert.ToString(qualifiedAtom.Location, 16));
                ReportMessage("Reading", qualifiedAtom.ToString());
            }
        }

        private SimpleAtom CheckValidMetaDataAtom(SimpleAtom qualifiedAtom)
        {
            long temppos = qualifiedAtom.Location + 12; //should point to 'meta'
            var testType = Utilities.GetFourBytes(fileReader, ref temppos);

            if (Utilities.UInt32ToChars(testType) != "meta")
            {
                //not a valid iTunes metadata atom, so replace with uninterpreted (just copies all data)
                var replaceAtom = new UnInterpretedAtom();
                replaceAtom.SetActualSize(qualifiedAtom.GetActualSize());
                replaceAtom.Location = qualifiedAtom.Location;
                qualifiedAtom = replaceAtom;
            }
            return qualifiedAtom;
        }


        //private string StackContents()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (SimpleAtom atom in atomStack)
        //    {
        //        sb.Append(atom.ToString() + "|");
        //    }
        //    return sb.ToString();
        //}

        //private ContainerAtom GetContainer()
        //{
        //    if (atomStack.Count > 0)
        //    {
        //        SimpleAtom atom = atomStack.Peek();
        //        if (atom is ContainerAtom)
        //            return (ContainerAtom)atom;
        //        else
        //            return null; //shouldn't ever happen, but...
        //    }
        //    else
        //        return null;
        //}

        private Utilities.MediaTypes CurrentMediaType = Utilities.MediaTypes.Unknown;


        private SimpleAtom QualifyAtom(SimpleAtom atom, ContainerAtom parent)
        {
            long seekpos = atom.Location;

            switch (atom.TypeString) 
            {

                case "ftyp": //  ftyp
                    return new FileHeaderAtom(fileReader, seekpos);

                case "moov": //  moov
                    return new MovieAtom(fileReader, seekpos);

                case "stsd": //  stsd
                    return new SampleDescriptionAtom(fileReader, seekpos, CurrentMediaType);

                case "stts": //  stts
                    return new TimeToSampleAtom(fileReader, seekpos);

                case "stsc": //  stsc
                    return new SampleToChunkAtom(fileReader, seekpos);

                case "stsz": //  stsz
                    return new SampleSizeAtom(fileReader, seekpos);

                case "stco": //  stco
                    return new ChunkOffsetAtom(fileReader, seekpos);

                case "tkhd": //  tkhd
                    return new TrackHeaderAtom(fileReader, seekpos);

                case "hdlr": //  hdlr
                    var hdlr = new HandlerReferenceAtom(fileReader, seekpos);
                    if (hdlr.ComponentTypeString == "mhlr")
                        CurrentMediaType = Utilities.GetCurrentMediaType(hdlr.ComponentSubTypeString);
                    return hdlr;

                case "mdhd": //  mdhd
                    return new MediaHeaderAtom(fileReader, seekpos);

                case "mvhd": //  mvhd
                    return new MovieHeaderAtom(fileReader, seekpos);

                case "trak": //  trak
                    return new TrackAtom(fileReader, seekpos);

                case "edts": //  edts
                    return new EditAtom(fileReader, seekpos);

                case "elst": //  elst
                    return new EditListAtom(fileReader, seekpos);

                case "minf": //  minf
                    return new MediaInformationAtom(fileReader, seekpos);

                case "mdia": //  mdia
                    return new MediaAtom(fileReader, seekpos);

                case "mdat": //  mdat
                    return new MediaDataAtom(fileReader, seekpos);

                case "stbl": //  stbl
                    return new SampleTableAtom(fileReader, seekpos);

                case "tref": //  tref
                    return new TrackReferenceAtom(fileReader, seekpos);

                case "tmcd": //  tmcd
                case "chap": //  chap
                case "sync": //  sync
                case "scpt": //  scpt
                case "ssrc": //  ssrc
                case "hint": //  hint
                    //if (atomStack.Count > 0)
                    //{
                    //    SimpleAtom parent = GetContainer();
                    //    if ((parent != null) && (parent is TrackReferenceAtom))
                    //        return new TrackReferenceTypeAtom(fileReader, seekpos);
                    //    else
                    //        return atom;
                    //}
                    //else
                    //    return atom;
                    if (parent != null)
                    {
                        if (parent is TrackReferenceAtom)
                        {
                            return new TrackReferenceTypeAtom(fileReader, seekpos);
                        }
                    }
                    return atom;

                case "smhd": //  smhd
                    return new SoundMediaInformationAtom(fileReader, seekpos);

                case "vmhd": //  vmhd
                    return new VideoMediaInformationAtom(fileReader, seekpos);

                case "dinf": //  dinf
                    return new DataInformationAtom(fileReader, seekpos);

                case "dref": //  dref
                    return new DataReferenceAtom(fileReader, seekpos);

                case "gmhd": //  gmhd
                    return new BaseMediaInfoHeaderAtom(fileReader, seekpos);

                case "gmin": //  gmin
                    return new BaseMediaInfoAtom(fileReader, seekpos);

                case "text": //  text
                    //if (atomStack.Count > 0)
                    //{
                    //    SimpleAtom parent = GetContainer();
                    //    if ((parent != null) && (parent is BaseMediaInfoHeaderAtom))
                    //    {
                    //        return new BaseTextAtom(fileReader, seekpos);
                    //    }
                    //    return new UnInterpretedAtom(fileReader, seekpos);
                    //}
                    //return new UnInterpretedAtom(fileReader, seekpos);
                    if (parent != null)
                    {
                        if (parent is BaseMediaInfoHeaderAtom)
                        {
                            return new BaseTextAtom(fileReader, seekpos);
                        }
                    }
                    return new UnInterpretedAtom(fileReader, seekpos);

                case "udta": //  udta
                    if (InterpretMetaData)
                    {
                        return new UserDataAtom(fileReader, seekpos);
                    }    
                    return new UnInterpretedAtom(fileReader, seekpos);


                    //----- these are atoms which may be found INSIDE the UserDataAtom ------

                case "meta": //  meta
                    return new MetaDataAtom(fileReader, seekpos);

                case "ilst": //  ilst
                    return new IListAtom(fileReader, seekpos);

                case "©nam":
                case "©ART":
                case "©art":
                case "©alb":
                case "©cmt":
                case "©gen":
                case "covr":
                case "desc":
                    return new MetaDataItem(fileReader, seekpos);

                //----- ------------------------------------------------------------------------ ------


                default:
                    return new UnInterpretedAtom(fileReader, seekpos);
            }
        }

        public string MetaDataToString()
        {
            if (!MetaDataExists())
                return "No metadata";


            return BookTitle + ":" + Author + ":" + PartName + ":" + Genre;
        }

        public ChunkMap Chunks
        {
            get { return myChunkMap; }
            set
            {
                myChunkMap = value;
            }
        }

        public Image CoverArtwork
        {
            get
            {
                if (MetaDataExists())
                {
                    return rootAtom.MetaData.GetArtwork();
                }
                return null;
            }
            set
            {
                if (!MetaDataExists())
                {
                    CreateMetaData();
                }
                rootAtom.MetaData.PutArtwork(value);
            }
        }

        public string BookTitle
        {
            get
            {
                if (MetaDataExists())
                {
                    return rootAtom.MetaData.GetAlbum();
                }
                return "";
            }
            set
            {
                if (!MetaDataExists())
                {
                    CreateMetaData();
                }
                rootAtom.MetaData.PutAlbum(value);
            }
        }

        public string Author
        {
            get
            {
                if (MetaDataExists())
                {
                    return rootAtom.MetaData.GetArtist();
                }
                return "";
            }
            set
            {
                if (!MetaDataExists())
                {
                    CreateMetaData();
                }
                rootAtom.MetaData.PutArtist(value);
            }
        }

        public string PartName
        {
            get
            {
                if (MetaDataExists())
                {
                    return rootAtom.MetaData.GetTitle();
                }
                return "";
            }
            set
            {
                if (!MetaDataExists())
                {
                    CreateMetaData();
                }
                rootAtom.MetaData.PutTitle(value);
            }
        }

        public string Genre
        {
            get
            {
                if (MetaDataExists())
                {
                    return rootAtom.MetaData.GetGenre();
                }
                return "";
            }
            set
            {
                if (!MetaDataExists())
                {
                    CreateMetaData();
                }
                rootAtom.MetaData.PutGenre(value);
            }
        }

        public string Description
        {
            get
            {
                if (MetaDataExists())
                {
                    return rootAtom.MetaData.GetDescription();
                }
                return "";
            }
            set
            {
                if (!MetaDataExists())
                {
                    CreateMetaData();
                }
                rootAtom.MetaData.PutDescription(value);
            }
        }   

        private SimpleAtom GetAtomHeader(long SeekPos)
        {
            if (SeekPos > fileReader.Length)
            {
                // ReportMessage("Reading","End of file");
                return null;
            }

            SimpleAtom atom = new SimpleAtom(fileReader,SeekPos);
            if (!atom.DataOK)
            {
                // ReportMessage("Reading", "End of file");
                return null;
            }
            return atom;
        }

        public void ReadMovieHeader(string filename)
        {
            if (!File.Exists(filename))
            {
                LoadedOK = false;
                LastError = "File not found! " + filename;
                throw new FileNotFoundException();
            }
            try
            {
                using (fileReader = new FileStream(filename, FileMode.Open))
                {
                    myFileLength = fileReader.Length;
                    rootAtom.SourceFilePath = filename;

 //                   LoadedOK = ReadAtomsToMovieHeader();
                    MovieAtom movie = LocateMovieAtom();
                   
                    if (movie != null)
                    {
                        LoadedOK = true;
                        var movieheader = LocateMovieHeader(movie.DataPos,movie.NextLocation());
                        if (movieheader != null)
                        {
                            movie.AddChild(movieheader);
                        }
                        var metadata = LocateMetaData(movie.DataPos, movie.NextLocation());
                        if (metadata != null)
                        {
                            movie.AddChild(metadata);
                        }
                        rootAtom.AddChild(movie);
                    }

                    fileReader.Close();
                    fileReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                ReportError("ReadMovieHeader:" + ex.Message);
                LoadedOK = false;
            }
        }

        ////this shortcuts the read if all we want is the duration.
        //private bool ReadAtomsToMovieHeader()
        //{
        //    if (fileReader == null)
        //    {
        //        ReportError("ReadAtomsToMovieHeader: No file open for read");
        //        return false;
        //    }

        //    long nextReadLocation = 0;
        //    GetFirstAtom(out nextReadLocation);

        //    while (nextReadLocation < fileReader.Length) //we usually get out of this with a return
        //    {
        //        try
        //        {
        //            SimpleAtom atom = GetAtomAt(ref nextReadLocation);
        //            ProcessingProgress(rootAtom.ChildAtoms.Count,ProcessingPhases.ReadingFile);
        //            if (atom == null)
        //            {
        //                LastError = "Encountered null atom before movie header";
        //                ReportError("ReadAtomsToMovieHeader:" + "Encountered null atom before movie header");
        //                return false;
        //            }
        //            if (atom is MovieHeaderAtom)
        //                return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            LastError = ex.Message;
        //            ReportError("ReadAtomsToMovieHeader:" + ex.Message);
        //            return false;
        //        }
        //    }
        //    return false;
        //}

        //this shortcuts the read 
        private MovieAtom LocateMovieAtom()
        {
            if (fileReader == null)
            {
                ReportError("LocateMovieAtom: No file open for read");
                return null;
            }

            long nextReadLocation = 0;
//            GetFirstAtom(out nextReadLocation);

            while (nextReadLocation < fileReader.Length) //we usually get out of this with a return
            {
                try
                {
                    SimpleAtom atom = GetAtomAt(ref nextReadLocation, rootAtom);
                    if (atom == null)
                    {
                        LastError = "Encountered null atom before movie";
                        ReportError("LocateMovieAtom:" + "Encountered null atom before movie atom");
                        return null;
                    }
                    if (atom is MovieAtom)
                    {
                        return (MovieAtom)atom;
                    }
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    ReportError("LocateMovieAtom:" + ex.Message);
                    return null;
                }
            }
            return null;
        }

        private MetaDataAtom LocateMetaData(long nextReadLocation, long endContainer)
        {
            InterpretMetaData = true;

            SimpleAtom atom;

            while(nextReadLocation < endContainer)
            {             
                long currentLocation = nextReadLocation;
    
                try
                {
                    atom = GetAtomHeader(nextReadLocation);
                }
                catch
                {
                    return null;
                }

                nextReadLocation = atom.NextLocation();

                if (atom.TypeString == "udta")
                {
                    var thisAtomLocation = currentLocation;
                    var qualifiedAtom = GetAtomAt(ref currentLocation, Movie);
                    //have to test UserDataAtom to see if it's a valid iTunes meta data container
                    if (qualifiedAtom is UserDataAtom)
                    {
                        long temppos = thisAtomLocation + 12; //should point to 'meta'
                        var testType = Utilities.GetFourBytes(fileReader, ref temppos);

                        if (Utilities.UInt32ToChars(testType) == "meta")
                        {
                            temppos = thisAtomLocation + 8;
                            SimpleAtom tempAtom = GetAtomAt(ref temppos, (UserDataAtom)qualifiedAtom);
                            if (tempAtom is MetaDataAtom)
                            {
                                var metaAtom = (MetaDataAtom) tempAtom;
                                ReadChildren(metaAtom); //fill it up
                                rootAtom.MetaData = metaAtom;
                                return rootAtom.MetaData;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private MovieHeaderAtom LocateMovieHeader(long nextReadLocation, long endContainer)
        {
            SimpleAtom atom;

            while (nextReadLocation < endContainer)
            {
                long currentLocation = nextReadLocation;
    
                try
                {
                    atom = GetAtomHeader(nextReadLocation);
                }
                catch
                {
                    return null;
                }
                nextReadLocation = atom.NextLocation();

                if (atom.TypeString == "mvhd")
                {
                    var qualifiedAtom = GetAtomAt(ref currentLocation,Movie);
                    if (qualifiedAtom is MovieHeaderAtom)
                    {
                        return (MovieHeaderAtom)qualifiedAtom;
                    }
                }
            }
            return null;
        }

        private bool ReadAllAtoms()
        {
            Globals.CurrentPhase = ProcessingPhases.ReadingFile;

            if (fileReader == null)
            {
                ReportError("ReadAllAtoms: No file open for read");
                return false;
            }

            try
            {
//                long nextReadLocation = 0;
                GetAllAtoms(); //this fills rootAtom with all children and their data

                //now do some book-keeping
                MovieAtom movie = Movie;
                if (movie == null)
                {
                    ReportError("ReadAllAtoms: No movie atom found!");
                    return false;
                }
                rootAtom.OriginalMovieSize = movie.GetActualSize();
                MediaDataAtom mdat = rootAtom.GetMediaData();
                if (mdat == null)
                {
                    ReportError("ReadAllAtoms: No movie data found!");
                    return false;
                }
                rootAtom.OriginalMDATstart = mdat.Location;

                BuildChunkMap();

                if (MetaDataExists())
                {
                    BookTitle = rootAtom.MetaData.GetAlbum();
                    Author = rootAtom.MetaData.GetArtist();
                    PartName = rootAtom.MetaData.GetTitle();
                    Genre = rootAtom.MetaData.GetGenre();
                }

                ProcessingProgress(100,ProcessingPhases.ReadComplete);
                Globals.CurrentPhase = ProcessingPhases.ReadComplete;
                return true;
            }
            catch (Exception ex)
            {
                ReportError("ReadAllAtoms: " + ex.Message);
                return false;
            }
        }

        private void StripTextTrack()
        {

            try
            {

                Chunks.SortByDestination(); //sort by destination offset.

                MovieAtom movie = Movie;

                MovieHeaderAtom mvhd = movie.GetMovieHeader();

                TrackAtom textTrack = movie.GetTextTrack();

                //Utilities.LogEvent("-----------------------------------------------------------------------------------------");
                //Utilities.LogEvent("As read, before removal of text entries");
                //Chunks.DumpEntries();



                if (textTrack != null)
                {
                    UInt32 textTrackID = textTrack.GetTrackID();

                    for (int chunkIndex = 0; chunkIndex < Chunks.Entries.Count; chunkIndex++)
                    {
                        ChunkMapEntry cme = Chunks.Entries[chunkIndex];

                        if ((cme.ChunkSourceType == ChunkSourceTypes.SourceFile) && (cme.TrackID == textTrackID))
                        {
                            cme.ChunkSourceType = ChunkSourceTypes.None; //disable this entry
                        }
                    }

                    ClearNullChunkMapEntries();
                }

                TrackAtom audiotrack = movie.GetAudioTrack();
                if (audiotrack != null)
                {
                    TrackReferenceAtom tref = audiotrack.GetTrackReference();
                    if (tref != null)
                        audiotrack.ChildAtoms.Remove(tref);
                }

                //now remove the track itself
                movie.ChildAtoms.Remove(textTrack);
            }
            catch(Exception ex)
            {
                ReportError("StripTextTrack: " + ex.Message);
            }
        }

        private void ClearNullChunkMapEntries()
        {
            // get rid of nulled entries and close up the gaps
            try
            {
                for (int i = 0; i < (Chunks.Entries.Count - 2); i++)
                {
                    if (Chunks.Entries[i].ChunkSourceType == ChunkSourceTypes.None)
                    {
                        Chunks.ShuffleUp(Chunks.Entries[i].DestOffset, Chunks.Entries[i].ChunkSize);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError("ClearNullChunkMapEntries: " + ex.Message);
            }

            try
            {
                for (int i = (Chunks.Entries.Count - 1); i >= 0; i--)
                {
                    if (Chunks.Entries[i].ChunkSourceType == ChunkSourceTypes.None)
                    {
                        Chunks.Entries.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError("ClearNullChunkMapEntries: " + ex.Message);
            }
        }

        private void BuildTextTableFromFile()
        {
            try
            {
                //now have to build/rebuild the text table

                //                Chunks.DumpEntries("As read");

                MovieAtom movie = Movie;

                MovieHeaderAtom mvhd = movie.GetMovieHeader();

                TrackAtom textTrack = movie.GetTextTrack();
                if (textTrack != null)
                {
                    UInt32 textTrackID = textTrack.GetTrackID();
                    List<TextTableEntry> textTable = new List<TextTableEntry>();

                    using (FileStream fileReader = new FileStream(rootAtom.SourceFilePath, FileMode.Open, FileAccess.Read))
                    {
                        for (int chunkIndex = 0; chunkIndex < Chunks.Entries.Count; chunkIndex++)
                        {
                            ChunkMapEntry cme = Chunks.Entries[chunkIndex];

                            if ((cme.ChunkSourceType == ChunkSourceTypes.SourceFile) && (cme.TrackID == textTrackID))
                            {
                                long seekpos = cme.SourceOffset;
                                StringBuilder sb = new StringBuilder();
                                UInt16 strlen = Utilities.GetTwoBytes(fileReader, ref seekpos);

                                TextTableEntry tte = new TextTableEntry();
                                tte.TrackID = textTrackID;

                                if (strlen > 0)
                                {
                                    byte[] stringbytes = new byte[strlen];

                                    for (int onebyte = 0; onebyte < strlen; onebyte++)
                                    {
                                        stringbytes[onebyte] = Utilities.GetOneByte(fileReader, ref seekpos);
                                    }

                                    //need to check for 16 bit encoding
                                    UInt16 byteOrderMark = 0;
                                    if (strlen >= 2)
                                    {
                                        byteOrderMark = (UInt16) (stringbytes[1]*256 + stringbytes[0]);
                                    }

                                    string unistring = "";

                                    switch (byteOrderMark)
                                    {
                                        case 65279: //65534: //FFFE then it's 16 bit, smallendian

                                            unistring = Encoding.Unicode.GetString(stringbytes, 2, strlen - 2);
                                            sb.Append(unistring);
                                            tte.CharType = TextTableEntry.CharTypes.UTF_16_SmallEndian;
                                            break;

                                        case 65534: //65279: //FEFF then it's 16 bit, bigendian
                                            unistring = Encoding.BigEndianUnicode.GetString(stringbytes, 2,
                                                                                            strlen - 2);
                                            sb.Append(unistring);
                                            tte.CharType = TextTableEntry.CharTypes.UTF_16_BigEndian;
                                            break;

                                        default: //it's 8-bit encoded
                                            unistring = Encoding.UTF8.GetString(stringbytes);
                                            sb.Append(unistring);
                                            tte.CharType = TextTableEntry.CharTypes.UTF_8;
                                            break;
                                    }
                                    tte.Text = sb.ToString();
                                }
                                else //empty chapter text
                                {
                                    tte.Text = " ";
                                }
                                tte.StartTime = cme.StartTime;
                                textTable.Add(tte);

                                cme.ChunkSourceType = ChunkSourceTypes.None; //disable this entry
                            }
                        }
                        fileReader.Close();
                    }

                    if (textTable.Count > 0) //if there WERE previous chapter stops
                    {
                        //now get rid of nulled entries and close up the gaps
                        try
                        {
                            Chunks.SortByDestination(); //sort by destination offset.

                            for (int i = 0; i < (Chunks.Entries.Count - 2); i++)
                            {
                                if (Chunks.Entries[i].ChunkSourceType == ChunkSourceTypes.None)
                                {
                                    //                                    Chunks.SubractOffsetFromDest(i + 1, Chunks.Entries[i].ChunkSize);
                                    Chunks.ShuffleUp(Chunks.Entries[i].DestOffset, Chunks.Entries[i].ChunkSize);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            ReportError("BuildTextTableFromFile: " + ex.Message);
                        }

                        try
                        {
                            for (int i = (Chunks.Entries.Count - 1); i >= 0; i--)
                            {
                                if (Chunks.Entries[i].ChunkSourceType == ChunkSourceTypes.None)
                                {
                                    Chunks.Entries.RemoveAt(i);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ReportError("BuildTextTableFromFile: " + ex.Message);
                        }

                        //now put the chapters BACK, this time as references to text table instead of source file
                        try
                        {
//                            UInt32 trackDuration = GetAudioTrackDuration(movie);
                            UInt32 trackDuration = GetTextTrackDuration(movie);

                            foreach (TextTableEntry tte in textTable)
                            {
                                Chunks.AddChapterStop(textTrackID, tte.StartTime, tte.Text, tte.CharType, trackDuration);
                            }

                            Chunks.RebuildTextSampleTable(textTrack);

                        }
                        catch (Exception ex)
                        {
                            ReportError("BuildTextTableFromFile:" + ex.Message);
                            LoadedOK = false;
                        }
                    }
                }
            }
            catch (System.IO.EndOfStreamException)
            {
                ReportError("Unable to read the end of the file - is it corrupted?");
                LoadedOK = false;
            }
            catch (Exception ex)
            {
                ReportError("BuildTextTableFromFile:" + ex.Message);
                LoadedOK = false;
            }
        }


        private void DeleteImageTrack()
        {
            try
            {
                Chunks.Entries.Sort(new CompareByDest()); //sort by destination offset.

                MovieAtom movie = Movie;

                MovieHeaderAtom mvhd = movie.GetMovieHeader();

                TrackAtom imageTrack = movie.GetVideoTrack();
                if (imageTrack != null)
                {
                    movie.RemoveChild(imageTrack);
                }
            }
            catch
            {
                //Utilities.LogEvent("-----------------------------------------------------------------------------------------");
                //Utilities.LogEvent("As read, before removal of image entries");
            }
        }


        private void StripImageTrack()
        {

            try
            {
                Chunks.Entries.Sort(new CompareByDest()); //sort by destination offset.

                MovieAtom movie = Movie;

                MovieHeaderAtom mvhd = movie.GetMovieHeader();

                //Utilities.LogEvent("-----------------------------------------------------------------------------------------");
                //Utilities.LogEvent("As read, before removal of image entries");
                //Chunks.DumpEntries();


                TrackAtom imageTrack = movie.GetVideoTrack();
                if (imageTrack != null)
                {
                    UInt32 imgTrackID = imageTrack.GetTrackID();

                    for (int chunkIndex = 0; chunkIndex < Chunks.Entries.Count; chunkIndex++)
                    {
                        ChunkMapEntry cme = Chunks.Entries[chunkIndex];

                        if ((cme.ChunkSourceType == ChunkSourceTypes.SourceFile) && (cme.TrackID == imgTrackID))
                        {
                            cme.ChunkSourceType = ChunkSourceTypes.None; //disable this entry
                        }
                    }


                    //now get rid of nulled entries and close up the gaps
                    try
                    {

                        for (int i = 0; i < (Chunks.Entries.Count - 2); i++)
                        {
                            if (Chunks.Entries[i].ChunkSourceType == ChunkSourceTypes.None)
                            {
                                Chunks.ShuffleUp(Chunks.Entries[i].DestOffset, Chunks.Entries[i].ChunkSize);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        ReportError("StripImageTrack:" + ex.Message);
                    }

                    try
                    {
                        for (int i = (Chunks.Entries.Count - 1); i >= 0; i--)
                        {
                            if (Chunks.Entries[i].ChunkSourceType == ChunkSourceTypes.None)
                            {
                                Chunks.Entries.RemoveAt(i);
                            }
                        }

                        //Utilities.LogEvent("-----------------------------------------------------------------------------------------");
                        //Utilities.LogEvent("After image entry removal");
                        //Chunks.DumpEntries();

                    }
                    catch (Exception ex)
                    {
                        ReportError("StripImageTrack:" + ex.Message);
                    }

                    TrackAtom audiotrack = movie.GetAudioTrack();
                    if (audiotrack != null)
                    {
                        TrackReferenceAtom tref = audiotrack.GetTrackReference();
                        if (tref != null)
                            audiotrack.ChildAtoms.Remove(tref);
                    }

                    //now remove the track itself
                    movie.ChildAtoms.Remove(imageTrack);

                }
            }
            catch (Exception ex)
            {
                ReportError("StripImageTrack:" + ex.Message);
            }
        }

        private void BuildImageTableFromFile()
        {
            
            try
            {
                //now have to build/rebuild the image table
                MovieAtom movie = Movie;

                MovieHeaderAtom mvhd = movie.GetMovieHeader();

                TrackAtom imageTrack = movie.GetVideoTrack();
                if (imageTrack != null)
                {
                    UInt32 imgTrackID = imageTrack.GetTrackID();
                    List<ImageTableEntry> imgTable = new List<ImageTableEntry>();

                    using (FileStream fileReader = new FileStream(rootAtom.SourceFilePath, FileMode.Open, FileAccess.Read))
                    {
                        for (int chunkIndex = 0; chunkIndex < Chunks.Entries.Count; chunkIndex++)
                        {
                            ChunkMapEntry cme = Chunks.Entries[chunkIndex];

                            if ((cme.ChunkSourceType == ChunkSourceTypes.SourceFile) && (cme.TrackID == imgTrackID))
                            {
                                long seekpos = cme.SourceOffset;
                                string AppData = Utilities.GetAppDataPath();

                                if (!Directory.Exists(AppData))
                                {
                                    Directory.CreateDirectory(AppData);
                                }

                                string fname = AppData + @"\image" + AACfile.ImageNum.ToString("00000"); // note no extension - this gets worked out in following routine
                                AACfile.ImageNum++;

                                ExtractImage(fileReader, cme.SourceOffset, cme.ChunkSize, ref fname);
                                ImageTableEntry ite = new ImageTableEntry();
                                ite.StartTime = cme.StartTime;
                                ite.SetFilePath(fname);
                                imgTable.Add(ite);
                                cme.ChunkSourceType = ChunkSourceTypes.None; //disable this entry
                            }
                        }
                        fileReader.Close();
                    }

                    if (imgTable.Count > 0) //there WERE previous images
                    {
                        //now get rid of nulled entries and close up the gaps
                        try
                        {
                            Chunks.SortByDestination(); //sort by destination offset.

                            for (int i = 0; i < (Chunks.Entries.Count - 2); i++)
                            {
                                if (Chunks.Entries[i].ChunkSourceType == ChunkSourceTypes.None)
                                {
                                    Chunks.ShuffleUp(Chunks.Entries[i].DestOffset, Chunks.Entries[i].ChunkSize);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            ReportError("BuildImageTableFromFile:" + ex.Message);
                            LoadedOK = false;
                        }

                        try
                        {
                            for (int i = (Chunks.Entries.Count - 1); i >= 0; i--)
                            {
                                if (Chunks.Entries[i].ChunkSourceType == ChunkSourceTypes.None)
                                {
                                    Chunks.Entries.RemoveAt(i);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ReportError("BuildImageTableFromFile:" + ex.Message);
                            LoadedOK = false;
                        }

                        //now put the images BACK, this time as references to image table instead of source file
                        try
                        {
//                            UInt32 trackDuration = GetAudioTrackDuration(movie);
                            UInt32 trackDuration = GetVideoTrackDuration(movie);

                            foreach (ImageTableEntry ite in imgTable)
                            {
                                Chunks.AddImage(imgTrackID, ite.StartTime, ite.FilePath, trackDuration);
                            }
                            //Utilities.LogEvent("-----------------------------------------------------------------------------------------");
                            //Utilities.LogEvent("After reinsert of images");
                            //Chunks.DumpEntries();

                            Chunks.RebuildVideoSampleTable(imageTrack);

                            //Utilities.LogEvent("-----------------------------------------------------------------------------------------");
                            //Utilities.LogEvent("After rebuild of sample table");
                            //Chunks.DumpEntries();

                        }
                        catch (Exception ex)
                        {
                            ReportError("BuildImageTableFromFile:" + ex.Message);
                            LoadedOK = false;
                        }
                    }
                   
                }
            }
            catch (Exception ex)
            {
                ReportError("BuildImageTableFromFile:" + ex.Message);
                LoadedOK = false;
            }
        }

        private void ExtractImage(FileStream fileReader, long startpos, uint imagesize, ref string filename)
        {
            try
            {
                //have to work out image type from header of data.
                long temppos = startpos;
                UInt32 testType = Utilities.GetFourBytes(fileReader, ref temppos);

                string ext = ".xxx";

                if ((testType & 0xFFFF0000) == 0xFFD80000)
                {
                    ext = ".jpg";
                }
                if ((testType & 0x00FFFFFF) == 0x00504E47)
                {
                    ext = ".png";
                }
                if ((testType & 0xFFFFFF00) == 0x47494600)
                {
                    ext = ".gif";
                }
                if ((testType & 0xFFFF0000) == 0x424D0000)
                {
                    ext = ".bmp";
                }

                if (ext == ".xxx")
                {
                    //unknown, so return
                    filename = "Unknown image type.xxx";
                    return;
                }
                filename += ext;

                using (FileStream fileWriter = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {

                    int buffsize = 8096;
                    long inpos = startpos;
                    long outpos = 0;
                    long remaining = imagesize; //we'll decrement this as we go

                    byte[] buffer = new byte[buffsize];
                    while (remaining > buffsize)
                    {
                        Utilities.GetBuffer(fileReader, ref inpos, buffsize, ref buffer);
                        Utilities.WriteBuffer(fileWriter, ref outpos, buffsize, ref buffer);
                        remaining -= buffsize;
                    }

                    if (remaining > 0)
                    {  
                        int toRead = Convert.ToInt32(remaining);
                        byte[] imgbuffer = new byte[toRead];
                        Utilities.GetBuffer(fileReader, ref inpos, toRead, ref imgbuffer);
                        Utilities.WriteBuffer(fileWriter, ref outpos, toRead, ref imgbuffer);
                    }
                    fileWriter.Flush();
                    fileWriter.Close();
                    fileWriter.Dispose();
                }
            }
            catch (Exception ex)
            {
                ReportError("ExtractImage:" + ex.Message);
            }
        }

        private UInt32 GetAudioTrackDuration(MovieAtom movie)
        {
            TrackAtom audiotrack = movie.GetAudioTrack();
            if (audiotrack != null)
            {
                MediaAtom mdia = audiotrack.GetMedia();
                if (mdia != null)
                {
                    MediaHeaderAtom mdhd = mdia.GetMediaHeader();
                    return mdhd.DurationInTimeUnits;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        private UInt32 GetAudioTrackTimeUnitsPerSecond(MovieAtom movie)
        {
            TrackAtom audiotrack = movie.GetAudioTrack();
            if (audiotrack != null)
            {
                MediaAtom mdia = audiotrack.GetMedia();
                if (mdia != null)
                {
                    MediaHeaderAtom mdhd = mdia.GetMediaHeader();
                    return mdhd.TimeUnitsPerSecond;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public UInt32 GetAudioTrackTimeUnitsPerSecond()
        {
            if (LoadedOK)
            {
                MovieAtom movie = Movie;
                return GetAudioTrackTimeUnitsPerSecond(movie);
            }
            else
                return 0;
        }

        private double GetAudioTrackDurationInSecs(MovieAtom movie)
        {
            TrackAtom audiotrack = movie.GetAudioTrack();
            if (audiotrack != null)
            {
                MediaAtom mdia = audiotrack.GetMedia();
                if (mdia != null)
                {
                    MediaHeaderAtom mdhd = mdia.GetMediaHeader();
                    return mdhd.DurationInSecs;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }


        public bool SaveFile(string filepath)
        {
            if (OpenWriteFile(filepath) == true)
            {
                WriteAllAtoms();
                CloseFileWriter();
                NeedsSave = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SaveFile()
        {
            return SaveFile(DestPath);
        }

        public enum ProcessingPhases
        {
            None,
            ReadingFile,
            ConstructingHeader,
            WritingHeader,
            WritingAudioData,
            WriteComplete,
            ConstructingMergeHeader,
            WritingMergeHeader,
            WritingMergeAudioData,
            WriteMergeComplete,
            ReadComplete
        }

        private bool WriteAllAtoms()
        {
            try
            {
                Globals.CurrentPhase = ProcessingPhases.ConstructingHeader;

                if (fileWriter == null)
                {
                    ReportError("WriteAllAtoms: No file open for write");
                    return false;
                }

                if (Chunks == null)
                {
                    ReportError("WriteAllAtoms: No data map");
                    return false;
                }

                MovieAtom movie = Movie;
                if (movie == null)
                {
                    ReportError("WriteAllAtoms: No movie atom to write!");
                    return false;
                }


                //rebuild the chunk map with entries in TextTable
                TrackAtom textTrack = movie.GetTextTrack();
                if (textTrack != null)
                {
                    //Chunks.DumpEntries("With text and images stripped");
                    Chunks.UpdateChunkMapWithChapters();
                    Chunks.RebuildTextSampleTable(textTrack);
                    //Chunks.DumpEntries("After text reinserted");

                }

                //rebuild the chunk map with images in ImageTable
                TrackAtom videoTrack = movie.GetVideoTrack();
                if (videoTrack != null)
                {
                    Chunks.UpdateChunkMapWithImages();
                    Chunks.RebuildVideoSampleTable(videoTrack);
                }

                long seekpos = 0;
                try
                {
                    myFileLength = Chunks.CalculateTotalSize();
                    myFileLength += rootAtom.CalculateSize() + 20; //20 is just a fudge - we're just using this to roughly estimate and display progress.
                }
                catch (Exception ex)
                {
                    ReportError("CalculateSize: " + ex.Message);
                    return false;
                }
                FileHeaderAtom ftyp = rootAtom.GetFileHeader();
                ftyp.ReplaceBrandString("M4A ", "M4B ");

                PositionUserDataAtEndOfMovieAtom(movie);

                AdjustMovieSize();

                //calculate MDAT offset (this becomes first entry value in stco table)
                long mdatStart = CalculateDataStart();
                if (mdatStart == -1)
                {
                    ReportError("WriteMerged: No MDAT atom to write!");
                    return false;
                }

                //this gets the final offsets correct (we hope!)
                Chunks.SortByDestination();

                AdjustDataStarts(mdatStart);

                RebuildChunkOffsets();

                //write children only - but treat mdat atom separately.

                foreach (SimpleAtom atom in rootAtom.ChildAtoms)
                {

                    if (atom.TypeNum != 0x6d646174 /*NOT mdat*/)
                    {
                        Globals.CurrentPhase = ProcessingPhases.WritingHeader;

                        if (ReportingOn)
                        {
                            ReportMessage("Writing",
                                          "ATOM: '" + atom.TypeString + "' at 0x" + Convert.ToString(seekpos, 16));
                            ReportMessage("Writing", atom.ToString());
                        }

                        atom.WriteData(fileWriter, ref seekpos);
                    }
                    else
                    {
                        //WriteMDAT
                        Globals.CurrentPhase = ProcessingPhases.WritingAudioData;

                        long mdatSize = Chunks.CalculateTotalSize() + atom.CalculateSize();
                        atom.SetActualSize(mdatSize);

                        long nextWritePos = seekpos + mdatSize;

                        if (ReportingOn)
                        {
                            ReportMessage("Writing", "ATOM: '" + atom.TypeString + "' at 0x" + Convert.ToString(seekpos, 16));
                            ReportMessage("Writing", "MDAT size=0x" + Convert.ToString(mdatSize, 16));
                            ReportMessage("Next Write Pos", Convert.ToString(nextWritePos, 16));
                        }
                        //this writes 'mdat' header only


                        atom.WriteData(fileWriter, ref seekpos);

                        int cmeCount = 0;
                        bool OK = true;
                        //now get chunks
                        using (fileReader = new FileStream(rootAtom.SourceFilePath, FileMode.Open))
                        {
                            foreach (ChunkMapEntry cme in Chunks.Entries)
                            {
                                switch (cme.ChunkSourceType)
                                {
                                    case ChunkSourceTypes.SourceFile:
                                        OK = TransferData(cme);
                                        if (!OK)
                                        {
                                            CloseFiles();
                                            return false;
                                        }
                                        break;

                                    case ChunkSourceTypes.ImageIndex:
                                        //read in image file
                                        OutputImage(cme);
                                        break;

                                    case ChunkSourceTypes.TextIndex:
                                        //use text entry
                                        OutputTextEntry(cme);
                                        break;
                                }
                                cmeCount++;
                                ProcessingProgress((100 * cmeCount) / Chunks.Entries.Count, ProcessingPhases.WritingAudioData);
                            }
                            fileReader.Close();
                            fileReader.Dispose();
                            fileReader = null;
                        }
                        //readjust writing position for following atoms
                        seekpos = nextWritePos;
                    }
                    if (ReportingOn)
                        ReportMessage("Writing", atom.ToString());
                }

                ProcessingProgress(100,ProcessingPhases.WriteComplete);
                Globals.CurrentPhase = ProcessingPhases.WriteComplete;
                CloseFiles();
                return true;
            }
            catch (Exception ex)
            {
                ReportError("WriteAllAtoms: " + ex.Message);
                return false;
            }
        }

        private void RebuildChunkOffsets()
        {

            MovieAtom movie = Movie;
            if (movie == null)
                return; //nothing to do!

            List<TrackAtom> tracks = movie.GetTracks();
            if (tracks == null)
                return;

            foreach (TrackAtom track in tracks)
            {
                ChunkOffsetAtom chunkOffsets = track.GetChunkOffsets();

                if (chunkOffsets != null)
                {
                    //we'll rebuild from scratch
                    chunkOffsets.ClearChunkOffsets();

                    int numOffsets = Chunks.Entries.Count;
                    int thisOffset = 0;
                    ProcessingProgress(0, Globals.CurrentPhase);

                    foreach (ChunkMapEntry cme in Chunks.Entries)
                    {
                        thisOffset++;
                        if (cme.TrackID == track.GetTrackID())
                        {
                            UInt32 uOffset = Convert.ToUInt32(cme.DestOffset);
                            chunkOffsets.AddChunkOffset(uOffset);
                        }
                        ProcessingProgress(((double)thisOffset * 100) / (double)numOffsets, Globals.CurrentPhase);
                    }
                }
            }
        }
        private bool TransferData(ChunkMapEntry cme)
        {
            try
            {
                if (cme.SourceOffset > 0)
                {
                    byte[] buffer = new byte[cme.ChunkSize];
                    long inpos = cme.SourceOffset;
                    long outpos = cme.DestOffset;

                    //if (ReportingOn)
                    //    ReportMessage("Writing", "Writing sourcefile chunk at 0x" + Convert.ToString(outpos, 16) + " length " + Convert.ToString(cme.ChunkSize, 16));

                    int chunksize = (int)cme.ChunkSize;
                    Utilities.GetBuffer(fileReader, ref inpos, chunksize, ref buffer);
                    Utilities.WriteBuffer(fileWriter, ref outpos, chunksize, ref buffer);

                }
            }
            catch (Exception ex)
            {
                ReportError("WriteAllAtoms: Reading+writing sound chunks: " + ex.Message);
                return false;
            }
            return true;
        }

        private void OutputTextEntry(ChunkMapEntry cme)
        {
            Globals.CurrentPhase = ProcessingPhases.WritingHeader;

            try
            {
                TextTableEntry tte = Chunks.GetTextEntryByStartTime(cme.StartTime);
                string text = tte.Text;
                long outpos = cme.DestOffset;
                if (text != "")
                {
                    //ReportMessage("Writing", "Writing text chunk '" + text + "' at 0x" + Convert.ToString(outpos, 16));
                    UInt16 uLength = 0;

                    byte[] stringbytes;

                    switch (tte.CharType)
                    {
                        case TextTableEntry.CharTypes.UTF_8:
                            uLength = Convert.ToUInt16(tte.TextSize());
                            stringbytes = Encoding.UTF8.GetBytes(tte.Text);
                            Utilities.WriteTwoBytes(uLength, fileWriter, ref outpos);
                            for (int i = 0; i < stringbytes.Length; i++)
                                {
                                    Utilities.WriteOneByte(stringbytes[i], fileWriter, ref outpos);
                            }
                            break;

                        case TextTableEntry.CharTypes.UTF_16_SmallEndian:
                            uLength = Convert.ToUInt16(tte.TextSize()+ 2);
                            stringbytes = Encoding.Unicode.GetBytes(tte.Text);

                            Utilities.WriteTwoBytes(uLength, fileWriter, ref outpos);
                            Utilities.WriteTwoBytes(65534 , fileWriter, ref outpos);
                            for (int i = 0; i < stringbytes.Length; i++)
                            {
                                Utilities.WriteOneByte(stringbytes[i], fileWriter, ref outpos);
                            }
                            break;

                        case TextTableEntry.CharTypes.UTF_16_BigEndian:
                            uLength = Convert.ToUInt16(tte.TextSize() + 2);
                            stringbytes = Encoding.BigEndianUnicode.GetBytes(tte.Text);

                            Utilities.WriteTwoBytes(uLength, fileWriter, ref outpos);
                            Utilities.WriteTwoBytes(65279, fileWriter, ref outpos);
                            for (int i = 0; i < stringbytes.Length; i++)
                            {
                                Utilities.WriteOneByte(stringbytes[i], fileWriter, ref outpos);
                            }
                            break;
                    }

                    Utilities.WriteFourBytes(0x0000000C, fileWriter, ref outpos);
                    Utilities.WriteFourBytes(0x656e6364, fileWriter, ref outpos); //'encd'
                    Utilities.WriteFourBytes(0x00000100, fileWriter, ref outpos);
                    ProcessingProgress(((double)outpos * 100) / (double)myFileLength, ProcessingPhases.WritingHeader);

                }
            }
            catch (Exception ex)
            {
                ReportError("Reading+writing text chunks: " + ex.Message);
            }
        }

        private void OutputImage(ChunkMapEntry cme)
        {
            Globals.CurrentPhase = ProcessingPhases.WritingHeader;

            try
            {
                string fname = Chunks.GetImagePathByStartTime(cme.StartTime);
                if (File.Exists(fname))
                {
                    using (FileStream imageReader = new FileStream(fname, FileMode.Open, FileAccess.Read))
                    {
                        int buffsize = 8096;
                        long inpos = 0;
                        long outpos = cme.DestOffset;
                        long filelen = imageReader.Length;

                        byte[] buffer = new byte[buffsize];
                        while (inpos < (filelen - buffsize))
                        {
                            Utilities.GetBuffer(imageReader, ref inpos, buffsize, ref buffer);
                            Utilities.WriteBuffer(fileWriter, ref outpos, buffsize, ref buffer);
                        }
                        if (inpos < cme.ChunkSize)
                        {
                            int toRead = Convert.ToInt32(cme.ChunkSize - inpos);
                            byte[] imgbuffer = new byte[toRead];
                            Utilities.GetBuffer(imageReader, ref inpos, toRead, ref imgbuffer);
                            Utilities.WriteBuffer(fileWriter, ref outpos, toRead, ref imgbuffer);
                        }

                        imageReader.Close();
                        imageReader.Dispose();
                    }
                }
                else
                {
                    ReportError("WriteAllAtoms: File " + fname + " could not be opened");
                }
            }
            catch (Exception ex)
            {
                ReportError("WriteAllAtoms: Reading+writing image chunks: " + ex.Message);
            }
        }

        private bool OpenWriteFile(string filename)
        {
            try
            {
                fileWriter = new FileStream(filename, FileMode.Create);
                return true;
            }
            catch (Exception ex)
            {
                ReportError("OpenWriteFile: " + ex.Message);
                return false;
            }
        }



        public void DivideInto(int numdivisions)
        {
            MovieAtom movie = Movie;
            double totalLen = GetAudioTrackDurationInSecs(movie);
            double onePart = totalLen / numdivisions;
            long ticks = (long)(onePart * 1E7);
            
            for (int i = 0; i < 20; i++)
            {
                TimeSpan tspan = new TimeSpan(ticks*i);
                int chapnum = i+1;
                string chapname = "Part " + chapnum.ToString("00");
                AddChapterStop(tspan, chapname);
            }
        }


        public bool AddChapterStop(TimeSpan startTime, string chapterName)
        {
            if (String.IsNullOrEmpty(chapterName))
                chapterName = " "; //don't want empty string as chapter

            MovieAtom movie = Movie;

            if (movie != null)
            {
                MovieHeaderAtom mvhd = movie.GetMovieHeader();

                if (mvhd == null)
                    return false; //can't do anything if no movie header!

                TrackAtom textTrack = GetOrAddTextTrack();
                UInt32 trackID = textTrack.GetTrackID();
                UInt32 timeScale = GetTextTrackTimeUnitsPerSecond(movie);

                long startTimeInUnits = TimeSpanToUnits(startTime, timeScale);
                UInt32 textDuration = GetTextTrackDuration(movie);

                if (!FarEnoughFromEnd(movie,startTimeInUnits))
                    return false; //can't add it, too near or after the end of the movie

                if ((startTimeInUnits > 0) && (Chunks.TextTable.Count == 0))
                {
                    //need a start point
                    Chunks.AddChapterStop(trackID, 0, "Start", TextTableEntry.CharTypes.UTF_8, textDuration);
                }
                Chunks.AddChapterStop(trackID, startTimeInUnits, chapterName, TextTableEntry.CharTypes.UTF_8, textDuration);
                Chunks.RebuildTextSampleTable(textTrack);
                NeedsSave = true;
                return true;
            }
            else
                return false;
        }


        private bool FarEnoughFromEnd(MovieAtom movie, long startTimeInUnits)
        {
            UInt32 audioDuration = GetAudioTrackDuration(movie);
            if (ConvertTextTimeToMovieTime(startTimeInUnits) >= ConvertAudioTimeToMovieTime(audioDuration))
                return false; //can't add it, after the end of the movie

            //now we test for 'too close'
            double durationInSecs = GetAudioTrackDurationInSecs(movie);
            double startTimeInSecs = ConvertTextTimeToSecs(startTimeInUnits);
            if (Math.Abs(durationInSecs - startTimeInSecs) < TooNearEndInSecs)
                return false;
            else
                return true;
        }

        public double ConvertTextTimeToSecs(long timeInUnits)
        {
            UInt32 textTimescale = GetTextTrackTimeUnitsPerSecond();
            double startTimeInSecs = ((double)timeInUnits / (double)textTimescale);
            return startTimeInSecs;
        }


        private uint GetTextTrackDuration(MovieAtom movie)
        {
            TrackAtom texttrack = movie.GetTextTrack();
            if (texttrack != null)
            {
                MediaAtom mdia = texttrack.GetMedia();
                if (mdia != null)
                {
                    MediaHeaderAtom mdhd = mdia.GetMediaHeader();
                    return mdhd.DurationInTimeUnits;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return GetAudioTrackDuration(movie);
            }
        }

        private void AdjustMovieSize()
        {
            //have to do this now to ensure movie shows correct size, but we'll need to do it AGAIN just before write
            RebuildChunkOffsets();

            Movie.SetActualSize(Movie.CalculateSize());
        }

        private void AdjustDataStarts(long mdatStart)
        {
            //this determines distance from the original location of mdat
            long offset = mdatStart - rootAtom.OriginalMDATstart;

            if (offset == 0)
                return; //nothing to do!

            if (offset > 0)
                Chunks.AddOffsetToDest(0, offset);
            else
                Chunks.SubractOffsetFromDest(0, Math.Abs(offset));
        }

        //this version uses the start time already specified in timescale units.
        public bool AddChapterStop(long startTimeInUnits, string chapterName)
        {
            if (String.IsNullOrEmpty(chapterName))
                chapterName = " "; //don't want empty string as chapter

            MovieAtom movie = Movie;

            ////remember current size so we can work out new offset
            //long lastSize = movie.CalculateSize();

            if (movie != null)
            {
                MovieHeaderAtom mvhd = movie.GetMovieHeader();

                if (mvhd == null)
                    return false; //can't do anything if no movie header!

                TrackAtom textTrack = GetOrAddTextTrack();
                UInt32 trackID = textTrack.GetTrackID();
                UInt32 audioDuration = GetAudioTrackDuration(movie);
                UInt32 textDuration = GetTextTrackDuration(movie);

                if (!FarEnoughFromEnd(movie, startTimeInUnits))
                    return false; //can't add it, too near or after the end of the movie

                TextTableEntry startchap = Chunks.GetTextEntryByStartTime(0);

                if ((startTimeInUnits > 0) && (startchap == null))
                {
                    //need a start point
                    Chunks.AddChapterStop(trackID, 0, "Start", TextTableEntry.CharTypes.UTF_8, textDuration);
                }
                Chunks.AddChapterStop(trackID, startTimeInUnits, chapterName, TextTableEntry.CharTypes.UTF_8, textDuration);
                Chunks.RebuildTextSampleTable(textTrack);
                NeedsSave = true;
                return true;
            }
            else
                return false;
        }

        public bool AddImage(TimeSpan startTime, string imagepath)
        {
            MovieAtom movie = Movie;

            ////remember current size so we can work out new offset
            //long lastSize = movie.CalculateSize();

            if (movie != null)
            {
                MovieHeaderAtom mvhd = movie.GetMovieHeader();
//                UInt32 timescale = GetAudioTrackTimeUnitsPerSecond(movie);
                TrackAtom textTrack = GetOrAddTextTrack();
                UInt32 timeScale = GetTextTrackTimeUnitsPerSecond(movie);

                if (mvhd == null)
                    return false; //can't do anything if no movie header!

                TrackAtom videoTrack = GetOrAddVideoTrack();
                UInt32 trackID = videoTrack.GetTrackID();

                long startTimeInUnits = TimeSpanToUnits(startTime, timeScale);
                UInt32 audioDuration = GetAudioTrackDuration(movie);
                UInt32 videoDuration = GetVideoTrackDuration(movie);

                if (!FarEnoughFromEnd(movie, startTimeInUnits))
                    return false; //can't add it, too near or after the end of the movie

                ImageTableEntry firstImage = Chunks.GetImageEntryByStartTime(0);

                if ((startTimeInUnits > 0) && (firstImage == null)) //no first image
                {
                    //need a start point
                    if (File.Exists(Utilities.DefaultImage))
                    {
                        Chunks.AddImage(trackID, 0, Utilities.DefaultImage, videoDuration);
                    }
                }
                Chunks.AddImage(trackID, startTimeInUnits, imagepath, videoDuration);
                Chunks.RebuildVideoSampleTable(videoTrack);
                NeedsSave = true;

                return true;
            }
            else
                return false;
        }

        public bool AddImage(long startTimeInUnits, string imagepath)
        {
            MovieAtom movie = Movie;

            if (movie != null)
            {
                MovieHeaderAtom mvhd = movie.GetMovieHeader();

                if (mvhd == null)
                    return false; //can't do anything if no movie header!

                TrackAtom videoTrack = GetOrAddVideoTrack();
                UInt32 trackID = videoTrack.GetTrackID();

                UInt32 audioDuration = GetAudioTrackDuration(movie);
                UInt32 videoDuration = GetVideoTrackDuration(movie);

                if (!FarEnoughFromEnd(movie, startTimeInUnits))
                    return false; //can't add it, too near or after the end of the movie

                ImageTableEntry firstImage = Chunks.GetImageEntryByStartTime(0);

                if ((startTimeInUnits > 0) && (firstImage == null)) //no first image
                {
                    //need a start point
                    if (File.Exists(Utilities.DefaultImage))
                    {
                        Chunks.AddImage(trackID, 0, Utilities.DefaultImage, videoDuration);
                    }
                }
                Chunks.AddImage(trackID, startTimeInUnits, imagepath, videoDuration);
                Chunks.RebuildVideoSampleTable(videoTrack);
                NeedsSave = true;

                return true;
            }
            else
                return false;
        }

        private uint GetVideoTrackDuration(MovieAtom movie)
        {
            TrackAtom imagetrack = movie.GetVideoTrack();
            if (imagetrack != null)
            {
                MediaAtom mdia = imagetrack.GetMedia();
                if (mdia != null)
                {
                    MediaHeaderAtom mdhd = mdia.GetMediaHeader();
                    return mdhd.DurationInTimeUnits;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return GetAudioTrackDuration(movie);
            }
        }

        public void SetDefaultImage(string filepath)
        {
            if (File.Exists(filepath))
            {
                Utilities.DefaultImage = filepath;
            }
        }

        //ChunkMap is a convenient way to solve problems like finding the chunk which starts at a certain time.
        private void BuildChunkMap()
        {
            try
            {
                Chunks = new ChunkMap();

                MovieAtom movie = Movie;

                if (movie == null)
                    return;

                List<TrackAtom> tracks = movie.GetTracks();

                foreach (TrackAtom track in tracks)
                {
                    MediaAtom media = track.GetMedia();
                    if (media != null)
                    {
                        MediaHeaderAtom mediaHead = media.GetMediaHeader();
                        MediaInformationAtom mediaInfo = media.GetMediaInformation();
                        if (mediaInfo != null)
                        {
                            SampleTableAtom sampleTable = mediaInfo.GetSampleTable();
                            if (sampleTable != null)
                            {

                                TimeToSampleAtom timeToSamples = sampleTable.GetTimeToSamples();
                                SampleToChunkAtom sampleToChunks = sampleTable.GetSampleToChunks();
                                SampleSizeAtom sampleSizes = sampleTable.GetSampleSizes();
                                ChunkOffsetAtom chunkOffsets = sampleTable.GetChunkOffsets();

                                UInt32 trackid = track.GetTrackID();
                                UInt32 numChunks = chunkOffsets.NumEntries;
                                UInt32 sampletotal = 1;

                                sampleToChunks.CalculateLastChunks(numChunks);
                                int stcindex = 0; //index into sampletochunks table

                                UInt32 sampsinchunk = 0;
                                UInt32 lastchunk = 0;

                                var sample = sampleToChunks.GetSampleToChunk(stcindex);
                                if (sample != null )
                                {
                                    sampsinchunk = sample.SamplesPerChunk;
                                    lastchunk = sample.LastChunk;

                                for (int numchunk = 0; numchunk < numChunks; numchunk++)
                                {
                                        ChunkMapEntry chmap = new ChunkMapEntry(trackid, numchunk,
                                                                                chunkOffsets.ChunkOffset(numchunk));

                                    chmap.StartSample = sampletotal;
                                    chmap.StartTime = timeToSamples.GetSampleStartTime(chmap.StartSample);
                                    chmap.NumSamples = sampsinchunk;

                                    UInt32 lastsample = sampletotal + sampsinchunk - 1;

                                    if (sampleSizes.mySampleSizeList.Count == 0)
                                        chmap.ChunkSize = chmap.NumSamples * sampleSizes.DefaultSampleSize;
                                    else
                                    {
                                            for (int numsample = (int) sampletotal;
                                                 numsample <= (int) lastsample;
                                                 numsample++)
                                        {
                                                uint temp = sampleSizes.GetSampleSize(numsample - 1);
                                                    //convert to zero-based array
                                            chmap.ChunkSize += temp;
                                        }
                                    }
//                                    Utilities.LogEvent(chmap.ToString());

                                    sampletotal += chmap.NumSamples;

                                    if ((numchunk + 1) >= lastchunk)
                                    {
                                        stcindex++;
                                        if (stcindex < sampleToChunks.NumEntries)
                                        {
                                            sampsinchunk = sampleToChunks.GetSampleToChunk(stcindex).SamplesPerChunk;
                                            lastchunk = sampleToChunks.GetSampleToChunk(stcindex).LastChunk;
                                        }
                                    }
                                    Chunks.Entries.Add(chmap);
                                }
                            }
                        }
                        }

                    } //if media != null
                }
                if (Chunks != null)
                    Chunks.SortByDestination();
            }
            catch (Exception ex)
            {
                Utilities.Reporter.DoSendReport(ActivityTypes.Error, 0, ex.Message);
            }

        }

        private void BuildAudioChunkMap()
        {
            try
            {
                myChunkMap = new ChunkMap();

                MovieAtom movie = Movie;

                if (movie == null)
                    return;

                TrackAtom audiotrack = movie.GetAudioTrack();

                if (audiotrack != null)
                {
                    MediaAtom media = audiotrack.GetMedia();
                    if (media != null)
                    {
                        MediaHeaderAtom mediaHead = media.GetMediaHeader();
                        MediaInformationAtom mediaInfo = media.GetMediaInformation();
                        if (mediaInfo != null)
                        {
                            SampleTableAtom sampleTable = mediaInfo.GetSampleTable();
                            if (sampleTable != null)
                            {

                                TimeToSampleAtom timeToSamples = sampleTable.GetTimeToSamples();
                                SampleToChunkAtom sampleToChunks = sampleTable.GetSampleToChunks();
                                SampleSizeAtom sampleSizes = sampleTable.GetSampleSizes();
                                ChunkOffsetAtom chunkOffsets = sampleTable.GetChunkOffsets();

                                UInt32 trackid = audiotrack.GetTrackID();
                                UInt32 numChunks = chunkOffsets.NumEntries;
                                UInt32 sampletotal = 1;

                                sampleToChunks.CalculateLastChunks(numChunks);
                                int stcindex = 0; //index into sampletochunks table

                                UInt32 sampsinchunk = sampleToChunks.GetSampleToChunk(stcindex).SamplesPerChunk;
                                UInt32 lastchunk = sampleToChunks.GetSampleToChunk(stcindex).LastChunk;

                                for (int numchunk = 0; numchunk < numChunks; numchunk++)
                                {
                                    ChunkMapEntry chmap = new ChunkMapEntry(trackid, numchunk, chunkOffsets.ChunkOffset(numchunk));

                                    chmap.StartSample = sampletotal;
                                    chmap.StartTime = timeToSamples.GetSampleStartTime(chmap.StartSample);
                                    chmap.NumSamples = sampsinchunk;
                                    chmap.EndTime =
                                        timeToSamples.GetSampleEndTime(chmap.StartSample + chmap.NumSamples - 1);

                                    UInt32 lastsample = sampletotal + sampsinchunk - 1;

                                    if (sampleSizes.mySampleSizeList.Count == 0)
                                        chmap.ChunkSize = chmap.NumSamples * sampleSizes.DefaultSampleSize;
                                    else
                                    {
                                        for (int numsample = (int)sampletotal; numsample <= (int)lastsample; numsample++)
                                        {
                                            uint temp = sampleSizes.GetSampleSize(numsample - 1); //convert to zero-based array
                                            chmap.ChunkSize += temp;
                                        }
                                    }
//                                    Utilities.LogEvent(chmap.ToString());

                                    sampletotal += chmap.NumSamples;

                                    if ((numchunk + 1) >= lastchunk)
                                    {
                                        stcindex++;
                                        if (stcindex < sampleToChunks.NumEntries)
                                        {
                                            sampsinchunk = sampleToChunks.GetSampleToChunk(stcindex).SamplesPerChunk;
                                            lastchunk = sampleToChunks.GetSampleToChunk(stcindex).LastChunk;
                                        }
                                    }
                                    Chunks.Entries.Add(chmap);
                                }
                            }
                        }

                    } //if media != null
                }
                //if (Chunks != null)
                //    Chunks.SortByDestination();
            }
            catch (Exception ex)
            {
                Utilities.Reporter.DoSendReport(ActivityTypes.Error, 0, ex.Message);
            }

        }

        private int GetIndexOfChunkAtTime(UInt32 timeInTimeUnits)
        {
            if (Chunks == null)
                BuildChunkMap();

            if (Chunks == null)
                return -1;

            MovieAtom movie = Movie;
            if (movie == null)
                return -1;

            TrackAtom track1 = movie.GetTrackAtomByID(1);
            if (track1 == null)
                return -1;

            UInt32 sampleduration = track1.GetSampleDuration();

            if (sampleduration > 0)
            {
                UInt32 sampNumberAtTime = timeInTimeUnits / sampleduration;
                //now we have to find out which chunk contains this sample number.
                int mapindex = 0;
                bool found = false;
                ChunkMapEntry cme = null;
                while ((mapindex < Chunks.Entries.Count) && (!found))
                {
                    cme = Chunks.Entries[mapindex];
                    UInt32 firstsample = cme.StartSample;
                    UInt32 lastsample = cme.StartSample + cme.NumSamples - 1;
                    if ((sampNumberAtTime >= firstsample) && (sampNumberAtTime <= lastsample))
                    {
                        found = true;
                    }

                    mapindex++;
                }
                if (found)
                    return mapindex;
                else
                    return -1;
            }
            return -1;
        }

        private TrackAtom GetOrAddTextTrack()
        {
            MovieAtom movie = Movie;
            if (movie == null)
                return null;

            MovieHeaderAtom mvhd = movie.GetMovieHeader();
            if (mvhd == null)
                return null;

            TrackAtom textTrack = movie.GetTextTrack();
            if (textTrack == null)
            {
                UInt32 nextTrackID = mvhd.NextTrackID;

                UInt32 trackDuration = 0;
                UInt32 trackTimeScale = 0;

                //highly desirable for text track to use same timescale as image track, if there is one.
                TrackAtom videoTrack = movie.GetVideoTrack();
                if (videoTrack != null)
                {
                    trackDuration = GetVideoTrackDuration(movie);
                    trackTimeScale = GetVideoTrackTimeUnitsPerSecond(movie);
                }
                else //use the audio track timescale instead.
                {
                    trackDuration = GetAudioTrackDuration(movie);
                    trackTimeScale = GetAudioTrackTimeUnitsPerSecond(movie);
                }

                textTrack = MakeTrackAtom(Utilities.MediaTypes.Text, nextTrackID, trackDuration, trackTimeScale);
                textTrack.Enabled = false;

                int insertPoint = GetLastTrackChild(movie) + 1;
                if (insertPoint >= 1)
                {
                    movie.AddChildAt(insertPoint, textTrack);
                }
                else
                {
                    movie.AddChild(textTrack);
                }

                mvhd.NextTrackID++;
            }

            TrackReferenceAtom tref = GetOrAddTrackReference(movie);
            if (tref == null)
            {
                return null;
            }

            TrackReferenceTypeAtom chapref = tref.GetTrackReferenceType("chap");
            if (chapref == null)
            {
                chapref = new TrackReferenceTypeAtom("chap");
                tref.AddChild(chapref);
            }
            chapref.AddTrackReference(textTrack.GetTrackID());           
            return textTrack;
        }

        private uint GetVideoTrackTimeUnitsPerSecond(MovieAtom movie)
        {
            TrackAtom videotrack = movie.GetVideoTrack();
            if (videotrack != null)
            {
                MediaAtom mdia = videotrack.GetMedia();
                if (mdia != null)
                {
                    MediaHeaderAtom mdhd = mdia.GetMediaHeader();
                    return mdhd.TimeUnitsPerSecond;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return GetAudioTrackTimeUnitsPerSecond();
            }
        }


        private TrackAtom GetOrAddVideoTrack()
        {
            MovieAtom movie = Movie;
            if (movie == null)
                return null;

            MovieHeaderAtom mvhd = movie.GetMovieHeader();
            if (mvhd == null)
                return null;

            TrackAtom videoTrack = movie.GetVideoTrack();
            if (videoTrack == null)
            {
                UInt32 nextTrackID = mvhd.NextTrackID;
                UInt32 trackDuration = 0;
                UInt32 trackTimeScale = 0;

                //highly desirable for image track to use same timescale as text track, if there is one.
                TrackAtom textTrack = movie.GetTextTrack();
                if (textTrack != null)
                {
                    trackDuration = GetTextTrackDuration(movie);
                    trackTimeScale = GetTextTrackTimeUnitsPerSecond(movie);
                }
                else //use the audio track timescale instead.
                {
                    trackDuration = GetAudioTrackDuration(movie);
                    trackTimeScale = GetAudioTrackTimeUnitsPerSecond(movie);
                }
                videoTrack = MakeTrackAtom(Utilities.MediaTypes.Video, nextTrackID, trackDuration, trackTimeScale);
                videoTrack.Enabled = true;

                int insertPoint = GetLastTrackChild(movie) + 1;
                if (insertPoint >= 1)
                {
                    movie.AddChildAt(insertPoint,videoTrack);
                }
                else
                {
                   movie.AddChild(videoTrack);
                }

                
                mvhd.NextTrackID++;
            }

            TrackReferenceAtom tref = GetOrAddTrackReference(movie);
            if (tref == null)
            {
                return null;
            }

            TrackReferenceTypeAtom chapref = tref.GetTrackReferenceType("chap");
            if (chapref == null)
            {
                chapref = new TrackReferenceTypeAtom("chap");
                tref.AddChild(chapref);
            }
            chapref.AddTrackReference(videoTrack.GetTrackID());         
            return videoTrack;
        }

        private int GetLastTrackChild(MovieAtom movie)
        {
            //find existing child tracks to we can position ours neatly after them


            if (movie.ChildCount() > 0)
            {
                //first, find the first track child:

                int childNum = -1;
                SimpleAtom child = null;

                bool foundTrack = false;
                do
                {
                    childNum++;
                    child = movie.ChildAtoms[childNum];
                    if (child is TrackAtom)
                        foundTrack = true;
                }
                while ((!foundTrack) && (childNum < movie.ChildCount()));

                // now move through all tracks until we get to last

                if (foundTrack)
                {
                    bool stillTrack = true;
                    while (stillTrack && (childNum < (movie.ChildCount() - 1)))
                    {
                        childNum++;
                        child = movie.ChildAtoms[childNum];
                        if (!(child is TrackAtom))
                            stillTrack = false;
                    }
                    return childNum;
                }
                else
                    return -1;
            }
            else
                return -1;
        }

        private TrackReferenceAtom GetOrAddTrackReference(MovieAtom movie)
        {
            TrackAtom audiotrack = movie.GetAudioTrack();
            if (audiotrack == null)
                return null;

            TrackReferenceAtom tref = audiotrack.GetTrackReference();
            if (tref == null)
            {
                tref = new TrackReferenceAtom();
                //find the MediaAtom - we want to be immediately prior to this
                int insertPoint = -1;
                for (int i = 0; i < audiotrack.ChildAtoms.Count; i++)
                {
                    if (audiotrack.ChildAtoms[i] is MediaAtom)
                        insertPoint = i;
                }
                if (insertPoint >= 0)
                    audiotrack.ChildAtoms.Insert(insertPoint, tref);
                else
                    audiotrack.AddChild(tref);
            }
            return tref;
        }

        private TrackAtom MakeTrackAtom(Utilities.MediaTypes tracktype, UInt32 trackid, UInt32 trackDuration, UInt32 trackTimescale)
        {

            MovieAtom movie = Movie;
            if (movie == null)
                return null;

            MovieHeaderAtom mvhd = movie.GetMovieHeader();
            if (mvhd == null)
                return null;

            TrackAtom audiotrack = movie.GetAudioTrack();
            if (audiotrack == null)
                return null;

            TrackHeaderAtom audioHeader = audiotrack.GetTrackHeader();

            TrackAtom newTrack = new TrackAtom();

            TrackHeaderAtom trackHeader = new TrackHeaderAtom(); //tkhd
            trackHeader.TrackID = trackid;

            //note this! The track MUST have the MOVIE duration (in the movie time scale)
            trackHeader.Duration = mvhd.DurationInTimeUnits;
            
            trackHeader.Volume = 0;
            for (int i = 0; i < audioHeader.Matrix.Length; i++)
            {
                trackHeader.Matrix[i] = audioHeader.Matrix[i];
            }
            
            newTrack.AddChild(trackHeader);

            ////copy the existing edit list,if there is one.
            //EditAtom edts = audiotrack.GetEditAtom();
            //if (edts != null)
            //    newTrack.AddChild(edts);

            MediaAtom media = new MediaAtom(); //mdia
            newTrack.AddChild(media);
            MediaHeaderAtom mdhd = new MediaHeaderAtom(); //mdhd
            mdhd.DurationInTimeUnits = trackDuration;
            mdhd.TimeUnitsPerSecond = trackTimescale;
            media.AddChild(mdhd);

            HandlerReferenceAtom hdlr = new HandlerReferenceAtom(); //hdlr
            media.AddChild(hdlr);

            switch (tracktype)
            {
                case Utilities.MediaTypes.Text:
                    {
                        #region CreateTextTrack
                        trackHeader.Flags = 14;
                        hdlr.ComponentTypeString = "mhlr";
                        hdlr.ComponentSubTypeString = "text";
                        hdlr.ComponentName = "Apple Text Media Handler";
                        hdlr.ManufacturerString = "app2";
                        hdlr.ComponentFlags = 1;
                        hdlr.ComponentMask = 65860;

                        MediaInformationAtom minf = new MediaInformationAtom();  //minf
                        media.AddChild(minf);

                        BaseMediaInfoHeaderAtom gmhd = new BaseMediaInfoHeaderAtom(); //gmhd
                        minf.AddChild(gmhd);
                        BaseMediaInfoAtom gmin = new BaseMediaInfoAtom(); //gmin
                        gmhd.AddChild(gmin);

                        gmin.GraphicsMode = 64;
                        gmin.RedColor = 0x8000;
                        gmin.GreenColor = 0x8000;
                        gmin.BlueColor = 0x8000;
                        gmin.Balance = 0;

                        BaseTextAtom btext = new BaseTextAtom(); //text
                        gmhd.AddChild(btext);

                        HandlerReferenceAtom dhdlr = new HandlerReferenceAtom();
                        minf.AddChild(dhdlr);

                        dhdlr.ComponentTypeString = "dhlr";
                        dhdlr.ComponentSubTypeString = "alis";
                        dhdlr.ManufacturerString = "appl";
                        dhdlr.ComponentName = "Apple Alias Data Handler";
                        dhdlr.ComponentFlags = 268435457;
                        dhdlr.ComponentMask = 65881;

                        DataInformationAtom dinf = new DataInformationAtom();
                        minf.AddChild(dinf);
                        DataReferenceAtom dref = new DataReferenceAtom();
                        dinf.AddChild(dref);
                        dref.AddDataReferenceEntry("alis");

                        SampleTableAtom stbl = new SampleTableAtom();
                        minf.AddChild(stbl);
                        SampleDescriptionAtom stsd = new SampleDescriptionAtom();
                        stbl.AddChild(stsd);
                        TextDescriptionEntry tde = new TextDescriptionEntry();
                        stsd.AddSampleDescription(tde);
                        TimeToSampleAtom stts = new TimeToSampleAtom();
                        stbl.AddChild(stts);
                        SampleToChunkAtom stsc = new SampleToChunkAtom();
                        stbl.AddChild(stsc);
                        SampleToChunkEntry stce = new SampleToChunkEntry();
                        stsc.AddSampleToChunkEntry(stce);
                        stce.FirstChunk = 1;
                        stce.SamplesPerChunk = 1;
                        stce.LastChunk = stce.FirstChunk + stce.SamplesPerChunk - 1;
                        stce.SampleDescriptionID = 1;


                        SampleSizeAtom stsz = new SampleSizeAtom();
                        stbl.AddChild(stsz);
                        ChunkOffsetAtom stco = new ChunkOffsetAtom();
                        stbl.AddChild(stco);
                        #endregion
                    }

                    break;

                case Utilities.MediaTypes.Video:
                    {
                        #region CreateVideoTrack
                        trackHeader.Flags = 15;
                        trackHeader.Width = 10485760;
                        trackHeader.Height = 10485760;
                        hdlr.ComponentTypeString = "mhlr";
                        hdlr.ComponentSubTypeString = "vide";
                        hdlr.ComponentName = "Apple Video Media Handler";
                        hdlr.ManufacturerString = "appl"; //note this is an L not a one
                        hdlr.ComponentFlags = 268435456;
                        hdlr.ComponentMask = 65858;

                        MediaInformationAtom minf = new MediaInformationAtom();  //minf
                        media.AddChild(minf);

                        VideoMediaInformationAtom vmhd = new VideoMediaInformationAtom(); //vmhd
                        minf.AddChild(vmhd);

                        vmhd.GraphicsMode = 64;
                        vmhd.RedColor = 0x8000;
                        vmhd.GreenColor = 0x8000;
                        vmhd.BlueColor = 0x8000;
                        vmhd.Flags = 1;

                        HandlerReferenceAtom dhdlr = new HandlerReferenceAtom();
                        minf.AddChild(dhdlr);

                        dhdlr.ComponentTypeString = "dhlr";
                        dhdlr.ComponentSubTypeString = "alis";
                        dhdlr.ManufacturerString = "appl";
                        dhdlr.ComponentName = "Apple Alias Data Handler";
                        dhdlr.ComponentFlags = 268435457;
                        dhdlr.ComponentMask = 65881;

                        DataInformationAtom dinf = new DataInformationAtom();
                        minf.AddChild(dinf);
                        DataReferenceAtom dref = new DataReferenceAtom();
                        dinf.AddChild(dref);
                        dref.AddDataReferenceEntry("alis");

                        SampleTableAtom stbl = new SampleTableAtom();
                        minf.AddChild(stbl);
                        SampleDescriptionAtom stsd = new SampleDescriptionAtom();
                        stbl.AddChild(stsd);

                        ////this is a default jpeg entry - for the time being we'll force new images to fit
                        ////these specifications
                        //VideoDescriptionEntry vde = new VideoDescriptionEntry();

                        //stsd.AddSampleDescription(vde);

                        TimeToSampleAtom stts = new TimeToSampleAtom();
                        stbl.AddChild(stts);
                        SampleToChunkAtom stsc = new SampleToChunkAtom();
                        stbl.AddChild(stsc);
                        SampleToChunkEntry stce = new SampleToChunkEntry();
                        stsc.AddSampleToChunkEntry(stce);
                        stce.FirstChunk = 1;
                        stce.SamplesPerChunk = 1;
                        stce.LastChunk = stce.FirstChunk + stce.SamplesPerChunk - 1;
                        stce.SampleDescriptionID = 1;


                        SampleSizeAtom stsz = new SampleSizeAtom();
                        stbl.AddChild(stsz);
                        ChunkOffsetAtom stco = new ChunkOffsetAtom();
                        stbl.AddChild(stco);
                        #endregion

                    }
                    break;

                case Utilities.MediaTypes.Sound:
                    //hdlr.ComponentTypeString = "mhlr";
                    //hdlr.ComponentSubTypeString = "soun";
                    //hdlr.ComponentName = "Apple Sound Media Handler";
                    //hdlr.ManufacturerString = "appl"; //note this is an L not a one
                    //media.AddChild(hdlr);

                    //minf = new MediaInformationAtom();  //minf

                    break;
            }
            return newTrack;
        }

        private long TimeSpanToUnits(TimeSpan span, UInt32 timeScale)
        {
            double spansecs = 0;
            double spanmilli = span.TotalMilliseconds;
            spansecs += (spanmilli / 1000);
            double timeinunits = spansecs * (double)timeScale;
            long retVal = Convert.ToInt64(timeinunits);
            return retVal;
        }

        public long TimeSpanToTextUnits(TimeSpan span)
        {
            UInt32 timescale = GetTextTrackTimeUnitsPerSecond();
            return TimeSpanToUnits(span, timescale);
        }

        private uint GetTextTrackTimeUnitsPerSecond()
        {
            if (LoadedOK)
            {
                MovieAtom movie = Movie;
                return GetTextTrackTimeUnitsPerSecond(movie);
            }
            else
                return 0;
        }

        private uint GetTextTrackTimeUnitsPerSecond(MovieAtom movie)
        {
            TrackAtom texttrack = movie.GetTextTrack();
            if (texttrack != null)
            {
                MediaAtom mdia = texttrack.GetMedia();
                if (mdia != null)
                {
                    MediaHeaderAtom mdhd = mdia.GetMediaHeader();
                    return mdhd.TimeUnitsPerSecond;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return GetAudioTrackTimeUnitsPerSecond();
            }
        }

        private TimeSpan UnitsToTimeSpan(long timeInUnits, UInt32 timeScale)
        {
            TimeSpan retSpan = new TimeSpan(0,0,0);
            if (timeInUnits == 0)
                return retSpan;

            if (timeScale == 0)
                return retSpan;

            long spanticks = Convert.ToInt64(((double)timeInUnits / (double) timeScale) * TimeSpan.TicksPerSecond);

            return new TimeSpan(spanticks);
        }

        public TimeSpan UnitsToTextTimeSpan(long timeInUnits)
        {
            UInt32 timescale = GetTextTrackTimeUnitsPerSecond();
            return UnitsToTimeSpan(timeInUnits, timescale);
        }

        public List<ChapterInfo> GetChapters()
        {
            if (Chunks == null)
                return null;

            if (Chunks.TextTable == null)
                return null;

            List<ChapterInfo> chapList = new List<ChapterInfo>();

            UInt32 timeScale = GetTextTrackTimeUnitsPerSecond();

            foreach (TextTableEntry tte in Chunks.TextTable)
            {
                ChapterInfo chap = new ChapterInfo();
                chap.StartTime = UnitsToTimeSpan(tte.StartTime, timeScale);
                chap.StartTimeInUnits = tte.StartTime;
                chap.Text = tte.Text;

                //try for an image
                chap.ImagePath = Chunks.GetImagePathByStartTime(tte.StartTime);
                chapList.Add(chap);
            }
            return chapList;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (fileReader != null)
            {
                fileReader.Close();
                fileReader.Dispose();
            }
            if (fileWriter != null)
            {
                fileWriter.Close();
                fileWriter.Dispose();
            }
        }

        #endregion


        public void ClearAllChaptersAndImages()
        {
            Chunks.TextTable.Clear();
            Chunks.ImageTable.Clear();
            NeedsSave = true;
        }

        public void RemoveChapterStop(long startTimeInUnits)
        {
            TextTableEntry tte = Chunks.GetTextEntryByStartTime(startTimeInUnits);
            if (tte != null)
            {

                long totalDuration = Chunks.GetTotalDurationFromTextTable();
                Chunks.TextTable.Remove(tte);
                //readjust durations of each chapter
                Chunks.CalculateTextDurations(totalDuration);
                NeedsSave = true;
            }
        }

        public void RemoveImage(long startTimeInUnits)
        {
            ImageTableEntry img = Chunks.GetImageEntryByStartTime(startTimeInUnits);
            if (img != null)
            {
                long totalDuration = Chunks.GetTotalDurationFromImageTable();
                Chunks.ImageTable.Remove(img);
                //readjust durations of each chapter
                Chunks.CalculateImageDurations(totalDuration);

                NeedsSave = true;
            }
            if (Chunks.ImageTable.Count == 0)
            {
                DeleteImageTrack();
            }
        }


        public TimeSpan GetMovieDurationAsTimeSpan()
        {
            if (LoadedOK)
            {
                MovieAtom movie = Movie;
                if (movie != null)
                {
                    MovieHeaderAtom mvhd = movie.GetMovieHeader();
                    if (mvhd != null)
                    {
                        long durationTicks = Convert.ToInt64(mvhd.DurationInSeconds * TimeSpan.TicksPerSecond);
                        TimeSpan duration = new TimeSpan(durationTicks);
                        return duration;
                    }
                    else
                        return new TimeSpan(0);
                }
                else
                    return new TimeSpan(0);
            }
            else
                return new TimeSpan(0);
        }

        public UInt32 GetMovieDurationInTimeUnits()
        {
            if (LoadedOK)
            {
                MovieAtom movie = Movie;
                if (movie != null)
                {
                    MovieHeaderAtom mvhd = movie.GetMovieHeader();
                    if (mvhd != null)
                    {
                        return mvhd.DurationInTimeUnits;
                    }
                    else
                        return 0;
                }
                else
                    return 0;
            }
            else
                return 0;
        }

        public void SetMovieDurationInTimeUnits(UInt32 duration)
        {
            if (LoadedOK)
            {
                MovieAtom movie = Movie;
                if (movie != null)
                {
                    MovieHeaderAtom mvhd = movie.GetMovieHeader();
                    if (mvhd != null)
                    {
                        mvhd.DurationInTimeUnits = duration;
                    }
                }
            }
        }



        public UInt32 GetMovieTimeUnitsPerSecond()
        {
            //movie time scale can differ from audio track time scale (darn it!)
            if (LoadedOK)
            {
                MovieAtom movie = Movie;
                if (movie != null)
                {
                    MovieHeaderAtom mvhd = movie.GetMovieHeader();
                    if (mvhd != null)
                    {
                        return mvhd.TimeScale;
                    }
                    else
                        return 0;

                }
                else
                    return 0;
            }
            else
                return 0;
        }

        public void SetMovieTimeUnitsPerSecond(UInt32 timescale)
        {
            //movie time scale can differ from audio track time scale (darn it!)
            if (LoadedOK)
            {
                MovieAtom movie = Movie;
                if (movie != null)
                {
                    MovieHeaderAtom mvhd = movie.GetMovieHeader();
                    if (mvhd != null)
                    {
                        mvhd.TimeScale = timescale;
                    }
                }
            }
        }

        public void ChangeMovieTimeScale(UInt32 timescale)
        {
            if (LoadedOK)
            {
                MovieAtom movie = Movie;
                if (movie != null)
                {
                    MovieHeaderAtom mvhd = movie.GetMovieHeader();
                    if (mvhd != null)
                    {
                        double durinsecs = (double)mvhd.DurationInTimeUnits/(double)mvhd.TimeScale;
                        mvhd.TimeScale = timescale;
                        mvhd.DurationInTimeUnits = Convert.ToUInt32(durinsecs*timescale);
                    }

                    EnforceMovieDuration();
                }
            }
        }

        public void EnforceMovieDuration()
        {
            if (LoadedOK)
            {
                MovieAtom movie = Movie;
                if (movie != null)
                {
                    MovieHeaderAtom mvhd = movie.GetMovieHeader();
                    if (mvhd != null)
                    {
                        UInt32 duration = mvhd.DurationInTimeUnits;
                        List<TrackAtom> tracks = movie.GetTracks();
                        foreach (TrackAtom track in tracks)
                        {
                            track.SetTrackHeaderDurationInTimeUnits(duration);
                            EditAtom edts = track.GetEditAtom();
                            if (edts != null)
                            {
                                EditListAtom elst = edts.GetEditList();
                                for (int i = 0; i < elst.EditListCount; i++)
                                {
                                    EditListEntry elent = elst.EditListItem(i);
                                    elent.TrackDuration = duration;
                                }
                            }

                        }
                    }
                }
            }
        }

        public static AACfile CreateNewFile(AACfile template)
        {
            var newFile = new AACfile {LoadedOK = true};

            var audioTrack = template.GetAudioTrack();

            var ftyp = new FileHeaderAtom {BrandString = "M4B "};
            ftyp.AddBrandType("M4B ");
            newFile.rootAtom.AddChild(ftyp);

            newFile.rootAtom.OriginalMDATstart = template.rootAtom.OriginalMDATstart;
            newFile.rootAtom.OriginalMovieSize = template.rootAtom.OriginalMovieSize;

            var moov = new MovieAtom();
            newFile.rootAtom.AddChild(moov);

            uint audioTimescale = audioTrack.GetTimeUnitsPerSecond();
            uint audioDuration = audioTrack.GetDurationInTimeUnits();


            var mvhd = new MovieHeaderAtom
                           {
                               CreationDate = DateTime.Today,
                               ModifiedDate = DateTime.Today,
                               TimeScale = audioTimescale / 4,
                               DurationInTimeUnits = audioDuration / 4 //this should fit it into a SIGNED int32, for quicktime.
                           };
            moov.AddChild(mvhd);
            moov.AddChild(audioTrack);

            var mdat = new MediaDataAtom();
            newFile.rootAtom.AddChild(mdat);
            newFile.EnforceMovieDuration();
            newFile.Chunks = template.Chunks;
            return newFile;
        }

        public UInt32 ConvertAudioTimeToMovieTime(long aTime)
        {
            UInt32 retVal = 0;
            UInt32 audioTimescale = GetAudioTrackTimeUnitsPerSecond();
            UInt32 movieTimescale = GetMovieTimeUnitsPerSecond();
            if (audioTimescale == movieTimescale)
            {
                try
                {
                    return Convert.ToUInt32(aTime);
                }
                catch (System.OverflowException)
                {
//                    ReportError("Audio file is too long!");
                    return UInt32.MaxValue - 1;
                }
                catch (System.Exception ex)
                {
//                    ReportError(ex.Message);
                    return 0;
                }
            }

            if (audioTimescale == 0)
                return 0;

            double ratio = (double)movieTimescale/(double)audioTimescale;

            try
            {
                retVal = Convert.ToUInt32(ratio * (double)aTime);
            }
            catch (System.OverflowException)
            {
//                ReportError("Audio file is too long!");
                return UInt32.MaxValue - 1; 
            }
            catch (System.Exception ex)
            {
//                ReportError(ex.Message);
                return 0;
            }
            return retVal;
        }

        public UInt32 ConvertTextTimeToMovieTime(long aTime)
        {
            UInt32 retVal = 0;
            UInt32 textTimescale = GetTextTrackTimeUnitsPerSecond();
            UInt32 movieTimescale = GetMovieTimeUnitsPerSecond();
            if (textTimescale == movieTimescale)
            {
                try
                {
                    return Convert.ToUInt32(aTime);
                }
                catch (System.OverflowException)
                {
//                    ReportError("Audio file is too long!");
                    return UInt32.MaxValue - 1; 
                }
                catch (System.Exception ex)
                {
//                    ReportError(ex.Message);
                    return 0;
                }
            }

            if (textTimescale == 0)
                return 0;

            double ratio = (double)movieTimescale / (double)textTimescale;

            try
            {
                retVal = Convert.ToUInt32(ratio * (double)aTime);
            }
            catch (System.OverflowException)
            {
//                ReportError("Audio file is too long!");
                return UInt32.MaxValue - 1;
            }
            catch (System.Exception ex)
            {
//                ReportError(ex.Message);
                return 0;
            }
            return retVal;
        }


        public long ConvertMovieTimeToAudioTime(UInt32 aTime)
        {
            UInt32 audioTimescale = GetAudioTrackTimeUnitsPerSecond();
            UInt32 movieTimescale = GetMovieTimeUnitsPerSecond();
            if (audioTimescale == movieTimescale)
                return aTime;

            if (movieTimescale == 0)
                return 0;
            
            double ratio = (double)audioTimescale / (double)movieTimescale;
            return Convert.ToInt64(ratio * (double)aTime);
        }

        public long ConvertMovieTimeToTextTime(UInt32 aTime)
        {
            UInt32 textTimescale = GetTextTrackTimeUnitsPerSecond();
            UInt32 movieTimescale = GetMovieTimeUnitsPerSecond();
            if (textTimescale == movieTimescale)
                return aTime;

            if (movieTimescale == 0)
                return 0;

            double ratio = (double)textTimescale / (double)movieTimescale;
            return Convert.ToInt64(ratio * (double)aTime);
        }

        public void UpdateChapterStop(long oldStartTime, long newStartTime, string newText)
        {
            MovieAtom movie = Movie;
            if (movie == null)
                return;
            //long duration = GetAudioTrackDuration(movie);
            long duration = GetTextTrackDuration(movie);
            Chunks.UpdateChapterStop(oldStartTime, newStartTime, newText, duration);
            NeedsSave = true;
        }

        public void UpdateImage(long oldStartTime, long newStartTime)
        {
            MovieAtom movie = Movie;
            if (movie == null)
                return;
            //long duration = GetAudioTrackDuration(movie);
            long duration = GetVideoTrackDuration(movie);

            Chunks.UpdateImage(oldStartTime, newStartTime, duration);
            NeedsSave = true;
        }
        //------------------------------------------------------------------------




        public int ConvertToMovieTime(long chaptime)
        {
            try
            {
                return Convert.ToInt32(chaptime);
            }
            catch
            {
                return 0;
            }
        }




        public ChapterInfo GetChapter(TimeSpan startTime)
        {
            if (Chunks == null)
                return null;

            if (Chunks.TextTable == null)
                return null;

            UInt32 timeScale = GetTextTrackTimeUnitsPerSecond();
            long startTimeInUnits = TimeSpanToUnits(startTime, timeScale);

            TextTableEntry tte = Chunks.GetTextEntryByStartTime(startTimeInUnits);

            if (tte == null)
                return null;

            ChapterInfo chap = new ChapterInfo();
            chap.StartTime = UnitsToTimeSpan(tte.StartTime, timeScale);
            chap.StartTimeInUnits = tte.StartTime;
            chap.Text = tte.Text;

            //try for an image
            chap.ImagePath = Chunks.GetImagePathByStartTime(tte.StartTime);
            return chap;
        }

        public ChapterInfo GetChapter(long startTimeInUnits)
        {
            if (Chunks == null)
                return null;

            if (Chunks.TextTable == null)
                return null;

            TextTableEntry tte = Chunks.GetTextEntryByStartTime(startTimeInUnits);

            if (tte == null)
                return null;

            UInt32 timeScale = GetTextTrackTimeUnitsPerSecond();

            ChapterInfo chap = new ChapterInfo();
            chap.StartTime = UnitsToTimeSpan(tte.StartTime, timeScale);
            chap.StartTimeInUnits = tte.StartTime;
            chap.Text = tte.Text;

            //try for an image
            chap.ImagePath = Chunks.GetImagePathByStartTime(tte.StartTime);
            return chap;
        }

        public enum MergeResults
        {
            Success,
            NoFilePath,
            BadStructure,
            EmptyFile,
            MismatchTimeScale,
            MismatchPreferredRate,
            MismatchSampleRate
        }


        public MergeResults MergeWith(AACfile otherFile)
        {
            MovieAtom movie1 = Movie;
            MovieAtom movie2 = otherFile.Movie;

            try
            {
                if (otherFile.SourcePath == null)
                    return MergeResults.NoFilePath;   

                int filenum = AddSourceFileToChunkMap(otherFile.SourcePath);

                var mvhd1 = movie1.GetMovieHeader();
                var mvhd2 = movie2.GetMovieHeader();

                otherFile.ChangeMovieTimeScale(mvhd1.TimeScale);

                //if (mvhd1.TimeScale != mvhd2.TimeScale)
                //    return MergeResults.MismatchTimeScale;

                if (mvhd1.PreferredRate != mvhd2.PreferredRate)
                    return MergeResults.MismatchPreferredRate;

                mvhd1.DurationInTimeUnits += mvhd2.DurationInTimeUnits;
                EnforceMovieDuration();

                var audio1 = movie1.GetAudioTrack();
                var audio2 = movie2.GetAudioTrack();

                var media1 = audio1.GetMedia();
                var mdhd1 = media1.GetMediaHeader();
                var media2 = audio2.GetMedia();
                var mdhd2 = media2.GetMediaHeader();

                if (mdhd1.TimeUnitsPerSecond != mdhd2.TimeUnitsPerSecond)
                    return MergeResults.MismatchSampleRate;

                mdhd1.DurationInTimeUnits += mdhd2.DurationInTimeUnits;

                var minf1 = media1.GetMediaInformation();
                var minf2 = media2.GetMediaInformation();

                var stbl1 = minf1.GetSampleTable();
                var stbl2 = minf2.GetSampleTable();

                var stts1 = stbl1.GetTimeToSamples();
                TimeToSampleEntry ttse = stts1.TimeToSample((int)stts1.NumEntries - 1);
                uint sampleDuration = ttse.SampleDuration;

                var stts2 = stbl2.GetTimeToSamples();

                for (int i = 0; i < stts2.TimeToSampleCount; i++)
                {
                    stts1.AddTimeToSample(stts2.TimeToSample(i));
                }
                stts1.ConsolidateEntries(true);

                var stsc1 = stbl1.GetSampleToChunks();
                var stsc2 = stbl2.GetSampleToChunks();

                var stco1 = stbl1.GetChunkOffsets();
                var stco2 = stbl2.GetChunkOffsets();

                for (int i = 0; i < stsc2.NumEntries; i++)
                {
                    SampleToChunkEntry stce = stsc2.GetSampleToChunk(i);
                    stce.FirstChunk += stco1.NumEntries;
                    stsc1.AddSampleToChunkEntry(stce);
                }
                stsc1.ConsolidateEntries();

                for (int i = 0; i < stco2.NumEntries; i++)
                {
                    //these are just fillers, will get overwritten later.
                    stco1.AddChunkOffset(stco2.ChunkOffset(i));
                }
                stco1.NumEntries += stco2.NumEntries;

                var stsz1 = stbl1.GetSampleSizes();
                var stsz2 = stbl2.GetSampleSizes();
                for (int i = 0; i < stsz2.NumEntries; i++)
                {
                    stsz1.AddSampleSize(stsz2.GetSampleSize(i));
                }
                stsz1.ResetNumEntries();

                ChunkMapEntry lastChunk = Chunks.Entries[Chunks.Entries.Count - 1];

                long nextStartTime = lastChunk.EndTime;
                long lastOffset = lastChunk.DestOffset + lastChunk.ChunkSize;
                uint startSample = lastChunk.StartSample + lastChunk.NumSamples;

                if (otherFile.Chunks.Entries.Count <= 0) 
                    return MergeResults.EmptyFile;
                
                long otherOffset = otherFile.Chunks.Entries[0].DestOffset;

                //append other file's chunk map, marked with fileid.
                for (int i = 0; i < otherFile.Chunks.Entries.Count; i++)
                {
                    ChunkMapEntry cme = otherFile.Chunks.Entries[i];
                    cme.FileID = filenum;
                    cme.StartTime += nextStartTime;
                    cme.DestOffset += (lastOffset - otherOffset);
                    cme.StartSample += startSample;
                    Chunks.Entries.Add(cme);
                }

                return MergeResults.Success;
            }
            catch
            {
                return MergeResults.BadStructure;
            }
        }


        private int AddSourceFileToChunkMap(string filename)
        {
            if (Chunks != null)
            {
                Chunks.FileTable.Add(filename);
                return Chunks.FileTable.Count - 1;
            }

            return -1;
        }

        public bool WriteMergedFile(string filename)
        {
            try
            {
                Globals.CurrentPhase = ProcessingPhases.ConstructingMergeHeader;

                if (fileWriter == null)
                {
                    if (!OpenWriteFile(filename))
                    {
                        ReportError("Unable to open " + filename + " for write!");
                        return false;
                    }
                }

                if (Chunks == null)
                {
                    ReportError("WriteMerged: No data map");
                    return false;
                }

                MovieAtom movie = Movie;
                if (movie == null)
                {
                    ReportError("WriteMerged: No movie atom to write!");
                    return false;
                }

                long seekpos = 0;
                myFileLength = rootAtom.CalculateSize() + Chunks.CalculateTotalSize() + 20; //20 is just a fudge - we're just using this to roughly estimate and display progress.

                FileHeaderAtom ftyp = rootAtom.GetFileHeader();
                ftyp.ReplaceBrandString("M4A ", "M4B ");

                PositionUserDataAtEndOfMovieAtom(movie);

                AdjustMovieSize();

                //calculate MDAT offset (this becomes first entry value in stco table)
                long mdatStart = CalculateDataStart();
                if (mdatStart == -1)
                {
                    ReportError("WriteMerged: No MDAT atom to write!");
                    return false;
                }

                AdjustDataStarts(mdatStart);

                RebuildChunkOffsets();

                //write children only - but treat mdat atom separately.
                foreach (SimpleAtom atom in rootAtom.ChildAtoms)
                {

                    if (atom.TypeNum != 0x6d646174 /*NOT mdat*/)
                    {
                        Globals.CurrentPhase = ProcessingPhases.WritingMergeHeader;

                        if (ReportingOn)
                        {
                            ReportMessage("Writing", "ATOM: '" + atom.TypeString + "' at 0x" + Convert.ToString(seekpos, 16));
                            ReportMessage("Writing", atom.ToString());
                        }

                        atom.WriteData(fileWriter, ref seekpos);
                    }
                    else //this is the MDAT
                    {
                        Globals.CurrentPhase = ProcessingPhases.WritingMergeAudioData;

                        long mdatSize = Chunks.CalculateTotalSize() + atom.CalculateSize(); //size of the MDAT header, only.
                        atom.SetActualSize(mdatSize);

                        long nextWritePos = seekpos + mdatSize; //next position AFTER writing out MDAT data

                        if (ReportingOn)
                        {
                            ReportMessage("Writing", "ATOM: '" + atom.TypeString + "' at 0x" + Convert.ToString(seekpos, 16));
                            ReportMessage("Writing", "MDAT size=0x" + Convert.ToString(mdatSize, 16));
                            ReportMessage("Next Write Pos", Convert.ToString(nextWritePos, 16));
                        }
                        //this writes 'mdat' header only


                        atom.WriteData(fileWriter, ref seekpos);

                        int lastFileId = -1;

                        int cmeCount = 0;
                        bool OK = true;

                        foreach (ChunkMapEntry cme in Chunks.Entries)
                        {
                            cmeCount++;
                            if (cme.ChunkSourceType == ChunkSourceTypes.SourceFile)
                            {
                                if (cme.FileID != lastFileId)
                                {
                                    lastFileId = cme.FileID;
                                    string sourcename = Chunks.FileTable[cme.FileID];
                                    if (!File.Exists(sourcename))
                                    {
                                        ReportError("Source file " + sourcename + " does not exist!");
                                        return false;
                                    }
                                    try
                                    {
                                        if (fileReader != null)
                                        {
                                            fileReader.Close();
                                        }
                                        fileReader = new FileStream(sourcename, FileMode.Open);
                                    }
                                    catch (Exception ex)
                                    {
                                        ReportError(ex.Message);
                                        return false;
                                    }
                                }

                                OK = TransferData(cme);
                                if (!OK)
                                {
                                    CloseFiles();
                                    return false;
                                }
                            }
                            ProcessingProgress((100*seekpos)/myFileLength, ProcessingPhases.WritingMergeAudioData);

                        }

                        if (ReportingOn)
                            ReportMessage("WriteMergedFile","MDAT written");

                        //readjust writing position for following atoms
                        seekpos = nextWritePos;

                    }
                    if (ReportingOn)
                        ReportMessage("Writing", atom.ToString());
                }

                ProcessingProgress(100, ProcessingPhases.WriteMergeComplete);
                Globals.CurrentPhase = ProcessingPhases.WriteMergeComplete;

                CloseFiles();

                return true;
            }
            catch (Exception ex)
            {
                ReportError("WriteAllAtoms: " + ex.Message);
                return false;
            }
        }

        //udta has to be last item in movie atom for iTunes to work properly
        private void PositionUserDataAtEndOfMovieAtom(MovieAtom movie)
        {
            if (MetaDataExists())
            {
                UserDataAtom tempUserDataAtom = null;

                foreach(SimpleAtom child in movie.ChildAtoms)
                {
                    if (child is UserDataAtom)
                    {
                        tempUserDataAtom = (UserDataAtom)child;
                    }
                }
                if (tempUserDataAtom != null)
                {
                    movie.ChildAtoms.Remove(tempUserDataAtom);
                    movie.ChildAtoms.Add(tempUserDataAtom);
                }
            }
        }

        private void CloseFiles()
        {
            if (fileReader != null)
                fileReader.Close();

            if (fileWriter != null)
            {
                fileWriter.Flush();
                fileWriter.Close();
                fileWriter.Dispose();
            }
        }


        private long CalculateDataStart()
        {
            long MDAToffset = 0;
            foreach (SimpleAtom atom in rootAtom.ChildAtoms)
            {
                if (atom.TypeNum == 0x6d646174)
                {
                    return MDAToffset;
                }
                MDAToffset += atom.CalculateSize();
            }
            return -1; //no MDAT atom
        }

        public TrackAtom GetAudioTrack()
        {
            if (LoadedOK)
            {
                return Movie.GetAudioTrack();
            }
            return null;
        }

        public TrackAtom GetVideoTrack()
        {
            if (LoadedOK)
            {
                return Movie.GetVideoTrack();
            }
            return null;
        }

        //this removes all atoms with given trackid (mostly used to remove iTunes udta atoms)
        public void SearchAndDestroy(string trackid)
        {
            if (rootAtom != null) 
                rootAtom.SearchAndDestroy(trackid);
        }

        public Image GetArtwork()
        {
            if (rootAtom != null)
            {
                if (rootAtom.MetaData != null)
                {
                    return rootAtom.MetaData.GetArtwork();
                }
            }
            return null;
        }

        public void PutArtwork(Image anImage)
        {
            if (rootAtom != null)
            {
                if (rootAtom.MetaData != null)
                {
                    rootAtom.MetaData.PutArtwork(anImage);
                }
                else
                {
                    CreateMetaData();
                }
            }
        }

        public void ClearArtwork()
        {
            if (rootAtom != null)
            {
                if (rootAtom.MetaData != null)
                {
                    rootAtom.MetaData.ClearArtwork();
                }
                else
                {
                    CreateMetaData();
                }
            }
        }

        private void CreateMetaData()
        {
            var udta = UserDataAtom.CreateUserDataAtom();

            //position of this new atom is vital.
            var moov = rootAtom.GetMovie();

            if (moov == null)
            {
                return;
            }

            //udta atom has to be the LAST atom in the movie atom for iTunes to recognise it.
            moov.AddChild(udta);
            rootAtom.MetaData = udta.GetMetaDataAtom();

            //and has to have a FREE atom after it

            rootAtom.EnsureFreeAfterMovie();
        }

        public bool MetaDataExists()
        {
            if (!LoadedOK)
                return false;

            return (rootAtom.MetaData != null);
        }


    }
}

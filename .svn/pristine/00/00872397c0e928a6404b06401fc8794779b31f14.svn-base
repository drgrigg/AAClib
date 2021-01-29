using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using AAClib;

namespace TestAAC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private AACfile qreader = null;

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void butDoIt_Click(object sender, EventArgs e)
        {
            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {
                OpenFile(dlgOpen.FileName);
            }
        }

        private void OpenFile(string filename)
        {
            statusLabel.Text = "Opening file " + filename;
            statusLabel.Invalidate();

            ClearProgress();
            try
            {
                this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
                qreader = new AACfile {InterpretMetaData = true};
                qreader.ReportMessage += new MessageHandler(qreader_ReportMessage);
                qreader.ReportError += new ReportErrorHandler(qreader_ReportError);
                qreader.ProcessingProgress += new ProcessingProgressHandler(qreader_ProcessingProgress);

                qreader.SetDefaultImage(Application.StartupPath + @"\book.jpg");
                qreader.ReportingOn = true;
                qreader.ReadFile(filename);

            }
            catch (System.Exception ex)
            {
                this.Cursor = System.Windows.Forms.Cursors.Default;
                MessageBox.Show("Unable to open file " + filename, "Error", MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                ShowProgress(ex.Message);
                qreader = null;
                return;
            }
            finally
            {
                this.Cursor = System.Windows.Forms.Cursors.Default;
            }
            barProgress.Value = 100;
            statusLabel.Text = "File loaded!";
        }

        void qreader_ProcessingProgress(double percentage, AACfile.ProcessingPhases phase)
        {
            if (percentage < 100.0)
            {
                int intpercent = Convert.ToInt32(percentage);
                barProgress.Value = intpercent;
            }
            Application.DoEvents();
        }

        void qreader_ReportError(string message)
        {
            ShowProgress(message);
        }

        void qreader_ReportMessage(string activity, string message)
        {
            string[] lines = message.Split('|');
            if (lines.Length > 0)
            {
                foreach (string line in lines)
                    if (line != "")
                        ShowProgress(Indent(qreader.Level) + line);
            }

            Application.DoEvents();
        }


        private string Indent(int level)
        {
            string indent = "";

            for (int i = 0; i < level; i++)
            {
                indent += "--- ";
            }
            return indent;
        }


        private void ClearProgress()
        {
            lstResults.Items.Clear();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (qreader != null)
            {
                qreader.Dispose();
                qreader = null;
            }

        }

        private void ShowProgress(string aMessage)
        {
            lstResults.Items.Add(aMessage);
        }


        private void butSave_Click(object sender, EventArgs e)
        {
            if (qreader != null)
            {
                dlgSaveLog.InitialDirectory = Global.ExtractPath(qreader.SourcePath);
                string temp = qreader.SourcePath.Replace(".m4b", ".txt");
                temp = temp.Replace(".m4a", ".txt");
                dlgSaveLog.FileName = temp;
            }

            if (dlgSaveLog.ShowDialog() == DialogResult.OK)
            {
                using (TextWriter tw = new StreamWriter(dlgSaveLog.FileName))
                {
                    foreach (string s in lstResults.Items)
                    {
                        tw.WriteLine(s);
                    }
                    tw.Flush();
                    tw.Close();
                    tw.Dispose();
                }
            }
        }

        private void butCopyFile_Click(object sender, EventArgs e)
        {
            if (qreader == null)
                return;

            if (dlgCopyData.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    lstResults.Items.Clear();
                    this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
                    qreader.SaveFile(dlgCopyData.FileName);
                    this.Cursor = System.Windows.Forms.Cursors.Default;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    this.Cursor = System.Windows.Forms.Cursors.Default;
                }
            }

            qreader.CloseFileWriter();
            barProgress.Value = 100;
            MessageBox.Show("File copied","All atoms read");
        }

        private void butTemp_Click(object sender, EventArgs e)
        {
            if (qreader != null)
            {

                //qreader.BuildTextTableFromFile();
                //qreader.BuildImageTableFromFile();

                qreader.AddChapterStop(new TimeSpan(0, 0, 47), "J.S.Bach");

                if (dlgOpenImage.ShowDialog() == DialogResult.OK)
                {
                    qreader.AddImage(new TimeSpan(0, 0, 47), dlgOpenImage.FileName);
                }

                qreader.AddChapterStop(new TimeSpan(0, 1, 34), "Some other pic");

                if (dlgOpenImage.ShowDialog() == DialogResult.OK)
                {
                    qreader.AddImage(new TimeSpan(0, 1, 34), dlgOpenImage.FileName);
                }

                ////if (dlgOpenImage.ShowDialog() == DialogResult.OK)
                ////{
                ////    qreader.AddImage(new TimeSpan(1, 30, 0), dlgOpenImage.FileName);
                ////}

                //qreader.StripTextTrack();

                //qreader.StripImageTrack();

                MessageBox.Show("Done");
            }
        }


        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length == 0)
                return;

            string file = files[0];
            ProcessDroppedFile(file);
        }

        private void ProcessDroppedFile(string file)
        {
            string ext = Global.ExtractExtension(file);
            switch (ext)
            {
                case "m4a":
                case "m4b":
                    OpenFile(file);
                    break;
            }
        }

    }
}
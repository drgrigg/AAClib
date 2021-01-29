namespace TestAAC
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.butDoIt = new System.Windows.Forms.Button();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
            this.butSave = new System.Windows.Forms.Button();
            this.dlgSaveLog = new System.Windows.Forms.SaveFileDialog();
            this.dlgCopyData = new System.Windows.Forms.SaveFileDialog();
            this.barProgress = new System.Windows.Forms.ProgressBar();
            this.dlgOpenImage = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butDoIt
            // 
            this.butDoIt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butDoIt.Location = new System.Drawing.Point(29, 362);
            this.butDoIt.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.butDoIt.Name = "butDoIt";
            this.butDoIt.Size = new System.Drawing.Size(87, 28);
            this.butDoIt.TabIndex = 0;
            this.butDoIt.Text = "Open File";
            this.butDoIt.UseVisualStyleBackColor = true;
            this.butDoIt.Click += new System.EventHandler(this.butDoIt_Click);
            // 
            // lstResults
            // 
            this.lstResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstResults.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstResults.FormattingEnabled = true;
            this.lstResults.ItemHeight = 15;
            this.lstResults.Location = new System.Drawing.Point(29, 20);
            this.lstResults.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lstResults.Name = "lstResults";
            this.lstResults.ScrollAlwaysVisible = true;
            this.lstResults.Size = new System.Drawing.Size(684, 334);
            this.lstResults.TabIndex = 1;
            // 
            // dlgOpen
            // 
            this.dlgOpen.Filter = "MP4 files|*.mp4;*.m4a;*.m4b|All files|*.*";
            // 
            // butSave
            // 
            this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSave.Location = new System.Drawing.Point(565, 363);
            this.butSave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.butSave.Name = "butSave";
            this.butSave.Size = new System.Drawing.Size(131, 28);
            this.butSave.TabIndex = 3;
            this.butSave.Text = "Save Log";
            this.butSave.UseVisualStyleBackColor = true;
            this.butSave.Click += new System.EventHandler(this.butSave_Click);
            // 
            // dlgSaveLog
            // 
            this.dlgSaveLog.DefaultExt = "txt";
            this.dlgSaveLog.Filter = "Text files|*.txt|All files|*.*";
            // 
            // dlgCopyData
            // 
            this.dlgCopyData.DefaultExt = "m4b";
            this.dlgCopyData.Filter = "AAC files|*.m4b|All files|*.*";
            // 
            // barProgress
            // 
            this.barProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.barProgress.Location = new System.Drawing.Point(29, 407);
            this.barProgress.Name = "barProgress";
            this.barProgress.Size = new System.Drawing.Size(678, 19);
            this.barProgress.TabIndex = 6;
            // 
            // dlgOpenImage
            // 
            this.dlgOpenImage.Filter = "Image files|*.jpg";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 465);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(725, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(725, 487);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.barProgress);
            this.Controls.Add(this.butSave);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.butDoIt);
            this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.Text = "AAC Analyser";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button butDoIt;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.OpenFileDialog dlgOpen;
        private System.Windows.Forms.Button butSave;
        private System.Windows.Forms.SaveFileDialog dlgSaveLog;
        private System.Windows.Forms.SaveFileDialog dlgCopyData;
        private System.Windows.Forms.ProgressBar barProgress;
        private System.Windows.Forms.OpenFileDialog dlgOpenImage;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
namespace MangaReader
{
    partial class ChapterSelectionForm
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
            this.components = new System.ComponentModel.Container();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.picCover = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblAuthorYear = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.listViewChapters = new System.Windows.Forms.ListView();
            this.btnPrevManga = new System.Windows.Forms.Button();
            this.btnNextManga = new System.Windows.Forms.Button();
            this.btnRead = new System.Windows.Forms.Button();
            this.Chapter = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pages = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lastRead = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCover)).BeginInit();
            this.SuspendLayout();
            // 
            // bindingSource1
            // 
            this.bindingSource1.CurrentChanged += new System.EventHandler(this.bindingSource1_CurrentChanged);
            // 
            // picCover
            // 
            this.picCover.Location = new System.Drawing.Point(28, 22);
            this.picCover.Name = "picCover";
            this.picCover.Size = new System.Drawing.Size(186, 243);
            this.picCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCover.TabIndex = 0;
            this.picCover.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(278, 22);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(44, 16);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "label1";
            // 
            // lblAuthorYear
            // 
            this.lblAuthorYear.AutoSize = true;
            this.lblAuthorYear.Location = new System.Drawing.Point(278, 76);
            this.lblAuthorYear.Name = "lblAuthorYear";
            this.lblAuthorYear.Size = new System.Drawing.Size(44, 16);
            this.lblAuthorYear.TabIndex = 2;
            this.lblAuthorYear.Text = "label2";
            // 
            // txtDescription
            // 
            this.txtDescription.BackColor = System.Drawing.Color.Silver;
            this.txtDescription.Location = new System.Drawing.Point(281, 114);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(577, 151);
            this.txtDescription.TabIndex = 3;
            this.txtDescription.TabStop = false;
            // 
            // listViewChapters
            // 
            this.listViewChapters.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.listViewChapters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Chapter,
            this.pages,
            this.lastRead});
            this.listViewChapters.FullRowSelect = true;
            this.listViewChapters.HideSelection = false;
            this.listViewChapters.Location = new System.Drawing.Point(28, 315);
            this.listViewChapters.Name = "listViewChapters";
            this.listViewChapters.Size = new System.Drawing.Size(752, 300);
            this.listViewChapters.TabIndex = 5;
            this.listViewChapters.UseCompatibleStateImageBehavior = false;
            this.listViewChapters.View = System.Windows.Forms.View.Details;
            // 
            // btnPrevManga
            // 
            this.btnPrevManga.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnPrevManga.Location = new System.Drawing.Point(705, 69);
            this.btnPrevManga.Name = "btnPrevManga";
            this.btnPrevManga.Size = new System.Drawing.Size(75, 23);
            this.btnPrevManga.TabIndex = 6;
            this.btnPrevManga.Text = "Prev";
            this.btnPrevManga.UseVisualStyleBackColor = false;
            // 
            // btnNextManga
            // 
            this.btnNextManga.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnNextManga.Location = new System.Drawing.Point(798, 69);
            this.btnNextManga.Name = "btnNextManga";
            this.btnNextManga.Size = new System.Drawing.Size(75, 23);
            this.btnNextManga.TabIndex = 7;
            this.btnNextManga.Text = "Next";
            this.btnNextManga.UseVisualStyleBackColor = false;
            // 
            // btnRead
            // 
            this.btnRead.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnRead.Location = new System.Drawing.Point(798, 315);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(75, 23);
            this.btnRead.TabIndex = 8;
            this.btnRead.Text = "Read";
            this.btnRead.UseVisualStyleBackColor = false;
            // 
            // Chapter
            // 
            this.Chapter.Width = 300;
            // 
            // pages
            // 
            this.pages.Width = 100;
            // 
            // lastRead
            // 
            this.lastRead.Width = 150;
            // 
            // ChapterSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(900, 650);
            this.Controls.Add(this.btnRead);
            this.Controls.Add(this.btnNextManga);
            this.Controls.Add(this.btnPrevManga);
            this.Controls.Add(this.listViewChapters);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblAuthorYear);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.picCover);
            this.Name = "ChapterSelectionForm";
            this.Text = "ChapterSelectionForm";
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCover)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.PictureBox picCover;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblAuthorYear;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.ListView listViewChapters;
        private System.Windows.Forms.Button btnPrevManga;
        private System.Windows.Forms.Button btnNextManga;
        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.ColumnHeader Chapter;
        private System.Windows.Forms.ColumnHeader pages;
        private System.Windows.Forms.ColumnHeader lastRead;
    }
}
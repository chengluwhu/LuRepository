namespace ExtractionTFL_Prj
{
    partial class FeaPtsFiltration
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
            this.saveButton = new System.Windows.Forms.Button();
            this.savefpts = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.fptOpenButton = new System.Windows.Forms.Button();
            this.tbFpts = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.oKButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.contourOpenButton = new System.Windows.Forms.Button();
            this.contourTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(500, 117);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 27;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // savefpts
            // 
            this.savefpts.Location = new System.Drawing.Point(174, 114);
            this.savefpts.Name = "savefpts";
            this.savefpts.Size = new System.Drawing.Size(277, 21);
            this.savefpts.TabIndex = 26;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(32, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 12);
            this.label3.TabIndex = 25;
            this.label3.Text = "SaveFpts:";
            // 
            // fptOpenButton
            // 
            this.fptOpenButton.Location = new System.Drawing.Point(500, 13);
            this.fptOpenButton.Name = "fptOpenButton";
            this.fptOpenButton.Size = new System.Drawing.Size(75, 23);
            this.fptOpenButton.TabIndex = 23;
            this.fptOpenButton.Text = "Open";
            this.fptOpenButton.UseVisualStyleBackColor = true;
            this.fptOpenButton.Click += new System.EventHandler(this.fptOpenButton_Click);
            // 
            // tbFpts
            // 
            this.tbFpts.Location = new System.Drawing.Point(174, 18);
            this.tbFpts.Name = "tbFpts";
            this.tbFpts.Size = new System.Drawing.Size(277, 21);
            this.tbFpts.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(13, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 12);
            this.label1.TabIndex = 20;
            this.label1.Text = "ImportingFeaturePts:";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(324, 177);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 28;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // oKButton
            // 
            this.oKButton.Location = new System.Drawing.Point(151, 177);
            this.oKButton.Name = "oKButton";
            this.oKButton.Size = new System.Drawing.Size(75, 23);
            this.oKButton.TabIndex = 29;
            this.oKButton.Text = "OK";
            this.oKButton.UseVisualStyleBackColor = true;
            this.oKButton.Click += new System.EventHandler(this.oKButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // contourOpenButton
            // 
            this.contourOpenButton.Location = new System.Drawing.Point(500, 64);
            this.contourOpenButton.Name = "contourOpenButton";
            this.contourOpenButton.Size = new System.Drawing.Size(75, 23);
            this.contourOpenButton.TabIndex = 32;
            this.contourOpenButton.Text = "Open";
            this.contourOpenButton.UseVisualStyleBackColor = true;
            this.contourOpenButton.Click += new System.EventHandler(this.contourOpenButton_Click);
            // 
            // contourTextBox
            // 
            this.contourTextBox.Location = new System.Drawing.Point(174, 61);
            this.contourTextBox.Name = "contourTextBox";
            this.contourTextBox.Size = new System.Drawing.Size(277, 21);
            this.contourTextBox.TabIndex = 31;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(17, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 12);
            this.label2.TabIndex = 30;
            this.label2.Text = "ImportingContourLines:";
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            // 
            // FeaPtsFiltration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 243);
            this.Controls.Add(this.contourOpenButton);
            this.Controls.Add(this.contourTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.oKButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.savefpts);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.fptOpenButton);
            this.Controls.Add(this.tbFpts);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FeaPtsFiltration";
            this.Text = "FeaPtsFiltration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.TextBox savefpts;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button fptOpenButton;
        private System.Windows.Forms.TextBox tbFpts;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button oKButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button contourOpenButton;
        private System.Windows.Forms.TextBox contourTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
    }
}
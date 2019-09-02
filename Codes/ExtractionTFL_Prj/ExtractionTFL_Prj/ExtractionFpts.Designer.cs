namespace ExtractionTFL_Prj
{
    partial class ExtractionFpts
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
            this.saveTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.contourOpenButton = new System.Windows.Forms.Button();
            this.contourTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.oKButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.tbBoundary = new System.Windows.Forms.TextBox();
            this.btnOpenBoundary = new System.Windows.Forms.Button();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(474, 86);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 16;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // saveTextBox
            // 
            this.saveTextBox.Location = new System.Drawing.Point(174, 86);
            this.saveTextBox.Name = "saveTextBox";
            this.saveTextBox.Size = new System.Drawing.Size(277, 21);
            this.saveTextBox.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(31, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 12);
            this.label3.TabIndex = 14;
            this.label3.Text = "SaveFpts:";
            // 
            // contourOpenButton
            // 
            this.contourOpenButton.Location = new System.Drawing.Point(474, 12);
            this.contourOpenButton.Name = "contourOpenButton";
            this.contourOpenButton.Size = new System.Drawing.Size(75, 23);
            this.contourOpenButton.TabIndex = 13;
            this.contourOpenButton.Text = "Open";
            this.contourOpenButton.UseVisualStyleBackColor = true;
            this.contourOpenButton.Click += new System.EventHandler(this.contourOpenButton_Click);
            // 
            // contourTextBox
            // 
            this.contourTextBox.Location = new System.Drawing.Point(174, 14);
            this.contourTextBox.Name = "contourTextBox";
            this.contourTextBox.Size = new System.Drawing.Size(277, 21);
            this.contourTextBox.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 12);
            this.label1.TabIndex = 11;
            this.label1.Text = "ImportingContourLines:";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(319, 148);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 17;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // oKButton
            // 
            this.oKButton.Location = new System.Drawing.Point(146, 148);
            this.oKButton.Name = "oKButton";
            this.oKButton.Size = new System.Drawing.Size(75, 23);
            this.oKButton.TabIndex = 18;
            this.oKButton.Text = "OK";
            this.oKButton.UseVisualStyleBackColor = true;
            this.oKButton.Click += new System.EventHandler(this.oKButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(9, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "ImportingBoundary:";
            // 
            // tbBoundary
            // 
            this.tbBoundary.Location = new System.Drawing.Point(174, 50);
            this.tbBoundary.Name = "tbBoundary";
            this.tbBoundary.Size = new System.Drawing.Size(277, 21);
            this.tbBoundary.TabIndex = 12;
            // 
            // btnOpenBoundary
            // 
            this.btnOpenBoundary.Location = new System.Drawing.Point(474, 50);
            this.btnOpenBoundary.Name = "btnOpenBoundary";
            this.btnOpenBoundary.Size = new System.Drawing.Size(75, 23);
            this.btnOpenBoundary.TabIndex = 19;
            this.btnOpenBoundary.Text = "Open";
            this.btnOpenBoundary.UseVisualStyleBackColor = true;
            this.btnOpenBoundary.Click += new System.EventHandler(this.btnOpenBoundary_Click);
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            // 
            // ExtractionFpts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 211);
            this.Controls.Add(this.btnOpenBoundary);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.oKButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.saveTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.contourOpenButton);
            this.Controls.Add(this.tbBoundary);
            this.Controls.Add(this.contourTextBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ExtractionFpts";
            this.Text = "ExtractionFpts";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.TextBox saveTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button contourOpenButton;
        private System.Windows.Forms.TextBox contourTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button oKButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbBoundary;
        private System.Windows.Forms.Button btnOpenBoundary;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
    }
}
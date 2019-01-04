namespace DA_BlockAttributesBrush
{
    partial class AttsSelection
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
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.buttonCancelAll = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.checkedListBoxAtts = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Location = new System.Drawing.Point(21, 282);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(86, 30);
            this.buttonSelectAll.TabIndex = 2;
            this.buttonSelectAll.Text = "全选(&A)";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // buttonCancelAll
            // 
            this.buttonCancelAll.Location = new System.Drawing.Point(113, 282);
            this.buttonCancelAll.Name = "buttonCancelAll";
            this.buttonCancelAll.Size = new System.Drawing.Size(82, 30);
            this.buttonCancelAll.TabIndex = 3;
            this.buttonCancelAll.Text = "撤销(&C)";
            this.buttonCancelAll.UseVisualStyleBackColor = true;
            this.buttonCancelAll.Click += new System.EventHandler(this.buttonCancelAll_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(201, 282);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(83, 30);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "确定(&O)";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // checkedListBoxAtts
            // 
            this.checkedListBoxAtts.CheckOnClick = true;
            this.checkedListBoxAtts.FormattingEnabled = true;
            this.checkedListBoxAtts.Location = new System.Drawing.Point(21, 12);
            this.checkedListBoxAtts.Name = "checkedListBoxAtts";
            this.checkedListBoxAtts.Size = new System.Drawing.Size(263, 264);
            this.checkedListBoxAtts.TabIndex = 1;
            // 
            // AttsSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(305, 324);
            this.Controls.Add(this.checkedListBoxAtts);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancelAll);
            this.Controls.Add(this.buttonSelectAll);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AttsSelection";
            this.Text = "选择属性";
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.CheckedListBox checkedListBoxAtts;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.Button buttonCancelAll;
        private System.Windows.Forms.Button buttonOK;
    }
}
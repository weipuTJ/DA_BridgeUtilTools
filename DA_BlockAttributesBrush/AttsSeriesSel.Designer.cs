namespace DA_BlockAttributesBrush
{
    partial class AttsSeriesSel
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
            this.comboBoxAtts = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxIsSeries = new System.Windows.Forms.CheckBox();
            this.textBoxPrefix = new System.Windows.Forms.TextBox();
            this.comboBoxNumType = new System.Windows.Forms.ComboBox();
            this.textBoxStartNum = new System.Windows.Forms.TextBox();
            this.textBoxSuffix = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.listBoxAtts = new System.Windows.Forms.ListBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxAtts
            // 
            this.comboBoxAtts.FormattingEnabled = true;
            this.comboBoxAtts.Location = new System.Drawing.Point(83, 17);
            this.comboBoxAtts.Name = "comboBoxAtts";
            this.comboBoxAtts.Size = new System.Drawing.Size(186, 20);
            this.comboBoxAtts.TabIndex = 0;
            this.comboBoxAtts.SelectedIndexChanged += new System.EventHandler(this.comboBoxAtts_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "选择属性";
            // 
            // checkBoxIsSeries
            // 
            this.checkBoxIsSeries.AutoSize = true;
            this.checkBoxIsSeries.Enabled = false;
            this.checkBoxIsSeries.Location = new System.Drawing.Point(300, 19);
            this.checkBoxIsSeries.Name = "checkBoxIsSeries";
            this.checkBoxIsSeries.Size = new System.Drawing.Size(60, 16);
            this.checkBoxIsSeries.TabIndex = 2;
            this.checkBoxIsSeries.Text = "序列化";
            this.checkBoxIsSeries.UseVisualStyleBackColor = true;
            this.checkBoxIsSeries.CheckedChanged += new System.EventHandler(this.checkBoxIsSeries_CheckedChanged);
            // 
            // textBoxPrefix
            // 
            this.textBoxPrefix.Enabled = false;
            this.textBoxPrefix.Location = new System.Drawing.Point(6, 50);
            this.textBoxPrefix.Name = "textBoxPrefix";
            this.textBoxPrefix.Size = new System.Drawing.Size(100, 21);
            this.textBoxPrefix.TabIndex = 3;
            // 
            // comboBoxNumType
            // 
            this.comboBoxNumType.Enabled = false;
            this.comboBoxNumType.FormattingEnabled = true;
            this.comboBoxNumType.Items.AddRange(new object[] {
            "1",
            "(1)",
            "一",
            "（一）"});
            this.comboBoxNumType.Location = new System.Drawing.Point(116, 50);
            this.comboBoxNumType.Name = "comboBoxNumType";
            this.comboBoxNumType.Size = new System.Drawing.Size(51, 20);
            this.comboBoxNumType.TabIndex = 4;
            this.comboBoxNumType.Text = "1";
            // 
            // textBoxStartNum
            // 
            this.textBoxStartNum.Enabled = false;
            this.textBoxStartNum.Location = new System.Drawing.Point(178, 50);
            this.textBoxStartNum.Name = "textBoxStartNum";
            this.textBoxStartNum.Size = new System.Drawing.Size(51, 21);
            this.textBoxStartNum.TabIndex = 5;
            this.textBoxStartNum.Text = "1";
            // 
            // textBoxSuffix
            // 
            this.textBoxSuffix.Enabled = false;
            this.textBoxSuffix.Location = new System.Drawing.Point(240, 49);
            this.textBoxSuffix.Name = "textBoxSuffix";
            this.textBoxSuffix.Size = new System.Drawing.Size(100, 21);
            this.textBoxSuffix.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "序列前缀";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(262, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "序列后缀";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.comboBoxNumType);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxPrefix);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxStartNum);
            this.groupBox1.Controls.Add(this.textBoxSuffix);
            this.groupBox1.Location = new System.Drawing.Point(14, 55);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(346, 80);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "序列设置";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(177, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "起始编号";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(114, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "编号类别";
            // 
            // listBoxAtts
            // 
            this.listBoxAtts.AccessibleRole = System.Windows.Forms.AccessibleRole.Cursor;
            this.listBoxAtts.FormattingEnabled = true;
            this.listBoxAtts.ItemHeight = 12;
            this.listBoxAtts.Location = new System.Drawing.Point(14, 151);
            this.listBoxAtts.Name = "listBoxAtts";
            this.listBoxAtts.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxAtts.Size = new System.Drawing.Size(255, 112);
            this.listBoxAtts.TabIndex = 10;
            this.listBoxAtts.SelectedIndexChanged += new System.EventHandler(this.listBoxAtts_SelectedIndexChanged);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Enabled = false;
            this.buttonAdd.Location = new System.Drawing.Point(285, 151);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 11;
            this.buttonAdd.Text = "添加";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Enabled = false;
            this.buttonRemove.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonRemove.Location = new System.Drawing.Point(285, 185);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonRemove.TabIndex = 12;
            this.buttonRemove.Text = "删除";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonOK.Location = new System.Drawing.Point(285, 240);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 13;
            this.buttonOK.Text = "确认(&O)";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // AttsSeriesSel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 279);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.listBoxAtts);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBoxIsSeries);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxAtts);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AttsSeriesSel";
            this.Text = "选择序列化属性";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxIsSeries;
        private System.Windows.Forms.TextBox textBoxPrefix;
        private System.Windows.Forms.ComboBox comboBoxNumType;
        private System.Windows.Forms.TextBox textBoxStartNum;
        private System.Windows.Forms.TextBox textBoxSuffix;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox listBoxAtts;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonOK;
        internal System.Windows.Forms.ComboBox comboBoxAtts;
    }
}
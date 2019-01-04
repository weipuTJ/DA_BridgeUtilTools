namespace DA_TendonTools
{
    partial class TendonInfo
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxScale = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxWorkLen = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxCtrlStress = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxEp = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxMiu = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxKii = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dataGridViewTendons = new System.Windows.Forms.DataGridView();
            this.ColumnTendonName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnTedonType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColumnTendonNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPipeDia = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnIsLeftDraw = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnIsRightDraw = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnLeftDrawAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnRightDrawAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLenEffective = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLenTotal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnHandle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonTendonSel = new System.Windows.Forms.Button();
            this.buttonConfirm = new System.Windows.Forms.Button();
            this.buttonExportTbl = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTendons)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBoxScale);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBoxWorkLen);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBoxCtrlStress);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxEp);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxMiu);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxKii);
            this.groupBox1.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(588, 82);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "总体参数";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(423, 51);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "绘图比例：";
            // 
            // textBoxScale
            // 
            this.textBoxScale.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxScale.Location = new System.Drawing.Point(518, 48);
            this.textBoxScale.Name = "textBoxScale";
            this.textBoxScale.Size = new System.Drawing.Size(55, 21);
            this.textBoxScale.TabIndex = 10;
            this.textBoxScale.Text = "100";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(423, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "工作长度(mm)：";
            // 
            // textBoxWorkLen
            // 
            this.textBoxWorkLen.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxWorkLen.Location = new System.Drawing.Point(518, 21);
            this.textBoxWorkLen.Name = "textBoxWorkLen";
            this.textBoxWorkLen.Size = new System.Drawing.Size(55, 21);
            this.textBoxWorkLen.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(228, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(119, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "张拉控制应力(MPa)：";
            // 
            // textBoxCtrlStress
            // 
            this.textBoxCtrlStress.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxCtrlStress.Location = new System.Drawing.Point(353, 47);
            this.textBoxCtrlStress.Name = "textBoxCtrlStress";
            this.textBoxCtrlStress.Size = new System.Drawing.Size(55, 21);
            this.textBoxCtrlStress.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(228, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "钢束弹模(MPa)：";
            // 
            // textBoxEp
            // 
            this.textBoxEp.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxEp.Location = new System.Drawing.Point(353, 20);
            this.textBoxEp.Name = "textBoxEp";
            this.textBoxEp.Size = new System.Drawing.Size(55, 21);
            this.textBoxEp.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(33, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "摩阻系数(1/rad)：";
            // 
            // textBoxMiu
            // 
            this.textBoxMiu.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxMiu.Location = new System.Drawing.Point(157, 49);
            this.textBoxMiu.Name = "textBoxMiu";
            this.textBoxMiu.Size = new System.Drawing.Size(55, 21);
            this.textBoxMiu.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(32, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "管道偏差系数(1/m)：";
            // 
            // textBoxKii
            // 
            this.textBoxKii.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxKii.Location = new System.Drawing.Point(157, 22);
            this.textBoxKii.Name = "textBoxKii";
            this.textBoxKii.Size = new System.Drawing.Size(55, 21);
            this.textBoxKii.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dataGridViewTendons);
            this.groupBox2.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.Location = new System.Drawing.Point(12, 100);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(794, 241);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "钢束参数";
            // 
            // dataGridViewTendons
            // 
            this.dataGridViewTendons.AllowUserToAddRows = false;
            this.dataGridViewTendons.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle8.NullValue = null;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTendons.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle8;
            this.dataGridViewTendons.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTendons.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnTendonName,
            this.ColumnTedonType,
            this.ColumnTendonNum,
            this.ColumnPipeDia,
            this.ColumnIsLeftDraw,
            this.ColumnIsRightDraw,
            this.ColumnLeftDrawAmount,
            this.ColumnRightDrawAmount,
            this.ColumnLenEffective,
            this.ColumnLenTotal,
            this.ColumnHandle});
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTendons.DefaultCellStyle = dataGridViewCellStyle13;
            this.dataGridViewTendons.EnableHeadersVisualStyles = false;
            this.dataGridViewTendons.Location = new System.Drawing.Point(6, 19);
            this.dataGridViewTendons.Name = "dataGridViewTendons";
            dataGridViewCellStyle14.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dataGridViewTendons.RowsDefaultCellStyle = dataGridViewCellStyle14;
            this.dataGridViewTendons.RowTemplate.Height = 23;
            this.dataGridViewTendons.Size = new System.Drawing.Size(782, 215);
            this.dataGridViewTendons.TabIndex = 0;
            this.dataGridViewTendons.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewTendons_CellContentClick);
            this.dataGridViewTendons.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridViewTendons_EditingControlShowing);
            // 
            // ColumnTendonName
            // 
            this.ColumnTendonName.Frozen = true;
            this.ColumnTendonName.HeaderText = "钢束编号";
            this.ColumnTendonName.Name = "ColumnTendonName";
            this.ColumnTendonName.Width = 80;
            // 
            // ColumnTedonType
            // 
            this.ColumnTedonType.Frozen = true;
            this.ColumnTedonType.HeaderText = "钢束规格";
            this.ColumnTedonType.Items.AddRange(new object[] {
            "Φ15-7",
            "Φ15-9",
            "Φ15-12",
            "Φ15-15"});
            this.ColumnTedonType.Name = "ColumnTedonType";
            this.ColumnTedonType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ColumnTedonType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // ColumnTendonNum
            // 
            this.ColumnTendonNum.Frozen = true;
            this.ColumnTendonNum.HeaderText = "钢束根数";
            this.ColumnTendonNum.Name = "ColumnTendonNum";
            this.ColumnTendonNum.Width = 80;
            // 
            // ColumnPipeDia
            // 
            this.ColumnPipeDia.Frozen = true;
            this.ColumnPipeDia.HeaderText = "管道直径(mm)";
            this.ColumnPipeDia.Name = "ColumnPipeDia";
            this.ColumnPipeDia.Width = 80;
            // 
            // ColumnIsLeftDraw
            // 
            this.ColumnIsLeftDraw.Frozen = true;
            this.ColumnIsLeftDraw.HeaderText = "左侧张拉";
            this.ColumnIsLeftDraw.Name = "ColumnIsLeftDraw";
            this.ColumnIsLeftDraw.ReadOnly = true;
            this.ColumnIsLeftDraw.Width = 40;
            // 
            // ColumnIsRightDraw
            // 
            this.ColumnIsRightDraw.Frozen = true;
            this.ColumnIsRightDraw.HeaderText = "右侧张拉";
            this.ColumnIsRightDraw.Name = "ColumnIsRightDraw";
            this.ColumnIsRightDraw.ReadOnly = true;
            this.ColumnIsRightDraw.Width = 40;
            // 
            // ColumnLeftDrawAmount
            // 
            dataGridViewCellStyle9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ColumnLeftDrawAmount.DefaultCellStyle = dataGridViewCellStyle9;
            this.ColumnLeftDrawAmount.Frozen = true;
            this.ColumnLeftDrawAmount.HeaderText = "左侧引伸量(mm)";
            this.ColumnLeftDrawAmount.Name = "ColumnLeftDrawAmount";
            this.ColumnLeftDrawAmount.ReadOnly = true;
            this.ColumnLeftDrawAmount.Width = 90;
            // 
            // ColumnRightDrawAmount
            // 
            dataGridViewCellStyle10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ColumnRightDrawAmount.DefaultCellStyle = dataGridViewCellStyle10;
            this.ColumnRightDrawAmount.Frozen = true;
            this.ColumnRightDrawAmount.HeaderText = "右侧引伸量(mm)";
            this.ColumnRightDrawAmount.Name = "ColumnRightDrawAmount";
            this.ColumnRightDrawAmount.ReadOnly = true;
            this.ColumnRightDrawAmount.Width = 90;
            // 
            // ColumnLenEffective
            // 
            dataGridViewCellStyle11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ColumnLenEffective.DefaultCellStyle = dataGridViewCellStyle11;
            this.ColumnLenEffective.Frozen = true;
            this.ColumnLenEffective.HeaderText = "钢束净长(mm)";
            this.ColumnLenEffective.Name = "ColumnLenEffective";
            this.ColumnLenEffective.ReadOnly = true;
            this.ColumnLenEffective.Width = 80;
            // 
            // ColumnLenTotal
            // 
            dataGridViewCellStyle12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ColumnLenTotal.DefaultCellStyle = dataGridViewCellStyle12;
            this.ColumnLenTotal.Frozen = true;
            this.ColumnLenTotal.HeaderText = "钢束总长(mm)";
            this.ColumnLenTotal.Name = "ColumnLenTotal";
            this.ColumnLenTotal.ReadOnly = true;
            this.ColumnLenTotal.Width = 80;
            // 
            // ColumnHandle
            // 
            this.ColumnHandle.Frozen = true;
            this.ColumnHandle.HeaderText = "ColumnHandle";
            this.ColumnHandle.Name = "ColumnHandle";
            this.ColumnHandle.ReadOnly = true;
            this.ColumnHandle.Visible = false;
            // 
            // buttonTendonSel
            // 
            this.buttonTendonSel.Location = new System.Drawing.Point(661, 23);
            this.buttonTendonSel.Name = "buttonTendonSel";
            this.buttonTendonSel.Size = new System.Drawing.Size(81, 64);
            this.buttonTendonSel.TabIndex = 2;
            this.buttonTendonSel.Text = "选择钢束";
            this.buttonTendonSel.UseVisualStyleBackColor = true;
            this.buttonTendonSel.Click += new System.EventHandler(this.buttonTendonSel_Click);
            // 
            // buttonConfirm
            // 
            this.buttonConfirm.Location = new System.Drawing.Point(589, 347);
            this.buttonConfirm.Name = "buttonConfirm";
            this.buttonConfirm.Size = new System.Drawing.Size(103, 23);
            this.buttonConfirm.TabIndex = 3;
            this.buttonConfirm.Text = "更新图形信息(&A)";
            this.buttonConfirm.UseVisualStyleBackColor = true;
            this.buttonConfirm.Click += new System.EventHandler(this.buttonConfirm_Click);
            // 
            // buttonExportTbl
            // 
            this.buttonExportTbl.Location = new System.Drawing.Point(713, 347);
            this.buttonExportTbl.Name = "buttonExportTbl";
            this.buttonExportTbl.Size = new System.Drawing.Size(85, 23);
            this.buttonExportTbl.TabIndex = 4;
            this.buttonExportTbl.Text = "输出表格(&O)";
            this.buttonExportTbl.UseVisualStyleBackColor = true;
            this.buttonExportTbl.Click += new System.EventHandler(this.buttonExportTbl_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(481, 347);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(102, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "取消并退出(&C)";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // TendonInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(810, 378);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonExportTbl);
            this.Controls.Add(this.buttonConfirm);
            this.Controls.Add(this.buttonTendonSel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TendonInfo";
            this.Text = "钢束表信息";
            this.Load += new System.EventHandler(this.TendonInfo_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTendons)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonTendonSel;
        private System.Windows.Forms.Button buttonConfirm;
        private System.Windows.Forms.Button buttonExportTbl;
        private System.Windows.Forms.Button buttonCancel;
        internal System.Windows.Forms.DataGridView dataGridViewTendons;
        internal System.Windows.Forms.TextBox textBoxKii;
        internal System.Windows.Forms.TextBox textBoxCtrlStress;
        internal System.Windows.Forms.TextBox textBoxEp;
        internal System.Windows.Forms.TextBox textBoxMiu;
        private System.Windows.Forms.Label label5;
        internal System.Windows.Forms.TextBox textBoxWorkLen;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTendonName;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColumnTedonType;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTendonNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPipeDia;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnIsLeftDraw;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnIsRightDraw;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLeftDrawAmount;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnRightDrawAmount;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLenEffective;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLenTotal;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnHandle;
        private System.Windows.Forms.Label label6;
        internal System.Windows.Forms.TextBox textBoxScale;
    }
}
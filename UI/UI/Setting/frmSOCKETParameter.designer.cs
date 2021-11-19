namespace iJedha.Automation.EAP.UI
{
    partial class frmSOCKETParameter
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

        #region [Windows Form Designer generated code]

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSOCKETParameter));
            this.gbSetting = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtRemotePort = new System.Windows.Forms.TextBox();
            this.lblMsg = new System.Windows.Forms.Label();
            this.gbNetStat = new System.Windows.Forms.GroupBox();
            this.btnNetStatusReflash = new System.Windows.Forms.Button();
            this.lblNetStatus = new System.Windows.Forms.Label();
            this.gbTime = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtTime = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLocalIP = new System.Windows.Forms.TextBox();
            this.txtRemoteIP = new System.Windows.Forms.TextBox();
            this.txtLocalPort = new System.Windows.Forms.TextBox();
            this.cboEncodeType = new System.Windows.Forms.ComboBox();
            this.cboConnectMode = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOpen = new System.Windows.Forms.Button();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.pictureBoxEnable = new System.Windows.Forms.PictureBox();
            this.label14 = new System.Windows.Forms.Label();
            this.comboBoxEQList = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnReopen = new System.Windows.Forms.Button();
            this.gbSetting.SuspendLayout();
            this.gbNetStat.SuspendLayout();
            this.gbTime.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEnable)).BeginInit();
            this.SuspendLayout();
            // 
            // gbSetting
            // 
            this.gbSetting.Controls.Add(this.label13);
            this.gbSetting.Controls.Add(this.txtRemotePort);
            this.gbSetting.Controls.Add(this.lblMsg);
            this.gbSetting.Controls.Add(this.gbNetStat);
            this.gbSetting.Controls.Add(this.gbTime);
            this.gbSetting.Controls.Add(this.label4);
            this.gbSetting.Controls.Add(this.label3);
            this.gbSetting.Controls.Add(this.label2);
            this.gbSetting.Controls.Add(this.label6);
            this.gbSetting.Controls.Add(this.label1);
            this.gbSetting.Controls.Add(this.txtLocalIP);
            this.gbSetting.Controls.Add(this.txtRemoteIP);
            this.gbSetting.Controls.Add(this.txtLocalPort);
            this.gbSetting.Controls.Add(this.cboEncodeType);
            this.gbSetting.Controls.Add(this.cboConnectMode);
            this.gbSetting.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gbSetting.Location = new System.Drawing.Point(1, 67);
            this.gbSetting.Name = "gbSetting";
            this.gbSetting.Size = new System.Drawing.Size(590, 243);
            this.gbSetting.TabIndex = 16;
            this.gbSetting.TabStop = false;
            this.gbSetting.Text = "SOCKET Parameter:";
            // 
            // label13
            // 
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(18, 187);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(84, 19);
            this.label13.TabIndex = 11;
            this.label13.Text = "Remote Port";
            // 
            // txtRemotePort
            // 
            this.txtRemotePort.Location = new System.Drawing.Point(139, 184);
            this.txtRemotePort.Name = "txtRemotePort";
            this.txtRemotePort.ReadOnly = true;
            this.txtRemotePort.Size = new System.Drawing.Size(139, 21);
            this.txtRemotePort.TabIndex = 5;
            this.txtRemotePort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtLocalPort_KeyPress);
            // 
            // lblMsg
            // 
            this.lblMsg.AutoSize = true;
            this.lblMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblMsg.ForeColor = System.Drawing.Color.Blue;
            this.lblMsg.Location = new System.Drawing.Point(37, 185);
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Size = new System.Drawing.Size(0, 18);
            this.lblMsg.TabIndex = 13;
            // 
            // gbNetStat
            // 
            this.gbNetStat.Controls.Add(this.btnNetStatusReflash);
            this.gbNetStat.Controls.Add(this.lblNetStatus);
            this.gbNetStat.Location = new System.Drawing.Point(315, 71);
            this.gbNetStat.Name = "gbNetStat";
            this.gbNetStat.Size = new System.Drawing.Size(257, 62);
            this.gbNetStat.TabIndex = 12;
            this.gbNetStat.TabStop = false;
            this.gbNetStat.Text = "Net Status";
            this.gbNetStat.Visible = false;
            // 
            // btnNetStatusReflash
            // 
            this.btnNetStatusReflash.FlatAppearance.BorderSize = 0;
            this.btnNetStatusReflash.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNetStatusReflash.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnNetStatusReflash.Image = global::iJedha.Automation.EAP.UI.Properties.Resources._default;
            this.btnNetStatusReflash.Location = new System.Drawing.Point(189, 18);
            this.btnNetStatusReflash.Margin = new System.Windows.Forms.Padding(5);
            this.btnNetStatusReflash.Name = "btnNetStatusReflash";
            this.btnNetStatusReflash.Size = new System.Drawing.Size(40, 29);
            this.btnNetStatusReflash.TabIndex = 24;
            this.btnNetStatusReflash.UseVisualStyleBackColor = true;
            this.btnNetStatusReflash.Click += new System.EventHandler(this.BtnNetStatusReflash_Click);
            // 
            // lblNetStatus
            // 
            this.lblNetStatus.BackColor = System.Drawing.Color.White;
            this.lblNetStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblNetStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblNetStatus.Location = new System.Drawing.Point(10, 23);
            this.lblNetStatus.Name = "lblNetStatus";
            this.lblNetStatus.Size = new System.Drawing.Size(105, 24);
            this.lblNetStatus.TabIndex = 6;
            this.lblNetStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gbTime
            // 
            this.gbTime.Controls.Add(this.label8);
            this.gbTime.Controls.Add(this.txtTime);
            this.gbTime.Location = new System.Drawing.Point(315, 144);
            this.gbTime.Name = "gbTime";
            this.gbTime.Size = new System.Drawing.Size(257, 62);
            this.gbTime.TabIndex = 12;
            this.gbTime.TabStop = false;
            this.gbTime.Text = "Time Out";
            this.gbTime.Visible = false;
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(16, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 19);
            this.label8.TabIndex = 6;
            this.label8.Text = "Time";
            // 
            // txtTime
            // 
            this.txtTime.Location = new System.Drawing.Point(57, 21);
            this.txtTime.Name = "txtTime";
            this.txtTime.ReadOnly = true;
            this.txtTime.Size = new System.Drawing.Size(48, 21);
            this.txtTime.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(18, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 19);
            this.label4.TabIndex = 8;
            this.label4.Text = "Local IP";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(18, 149);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 19);
            this.label3.TabIndex = 7;
            this.label3.Text = "Remote IP";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(18, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 19);
            this.label2.TabIndex = 6;
            this.label2.Text = "Local Port";
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(312, 36);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(118, 17);
            this.label6.TabIndex = 5;
            this.label6.Text = "Encode Type";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(18, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Connect Mode";
            // 
            // txtLocalIP
            // 
            this.txtLocalIP.Location = new System.Drawing.Point(139, 70);
            this.txtLocalIP.Name = "txtLocalIP";
            this.txtLocalIP.ReadOnly = true;
            this.txtLocalIP.Size = new System.Drawing.Size(139, 21);
            this.txtLocalIP.TabIndex = 2;
            this.txtLocalIP.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtIPAddress_KeyPress);
            // 
            // txtRemoteIP
            // 
            this.txtRemoteIP.Location = new System.Drawing.Point(139, 149);
            this.txtRemoteIP.Name = "txtRemoteIP";
            this.txtRemoteIP.ReadOnly = true;
            this.txtRemoteIP.Size = new System.Drawing.Size(139, 21);
            this.txtRemoteIP.TabIndex = 4;
            this.txtRemoteIP.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtIPAddress_KeyPress);
            // 
            // txtLocalPort
            // 
            this.txtLocalPort.Location = new System.Drawing.Point(139, 111);
            this.txtLocalPort.Name = "txtLocalPort";
            this.txtLocalPort.ReadOnly = true;
            this.txtLocalPort.Size = new System.Drawing.Size(139, 21);
            this.txtLocalPort.TabIndex = 3;
            this.txtLocalPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtLocalPort_KeyPress);
            // 
            // cboEncodeType
            // 
            this.cboEncodeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEncodeType.Enabled = false;
            this.cboEncodeType.FormattingEnabled = true;
            this.cboEncodeType.Items.AddRange(new object[] {
            "Ascii ",
            "Unicode"});
            this.cboEncodeType.Location = new System.Drawing.Point(433, 32);
            this.cboEncodeType.Name = "cboEncodeType";
            this.cboEncodeType.Size = new System.Drawing.Size(139, 23);
            this.cboEncodeType.TabIndex = 1;
            // 
            // cboConnectMode
            // 
            this.cboConnectMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboConnectMode.Enabled = false;
            this.cboConnectMode.FormattingEnabled = true;
            this.cboConnectMode.Items.AddRange(new object[] {
            "ACTIVE",
            "PASSIVE"});
            this.cboConnectMode.Location = new System.Drawing.Point(139, 32);
            this.cboConnectMode.Name = "cboConnectMode";
            this.cboConnectMode.Size = new System.Drawing.Size(139, 23);
            this.cboConnectMode.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnOpen);
            this.panel1.Controls.Add(this.txtPath);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.pictureBoxEnable);
            this.panel1.Controls.Add(this.label14);
            this.panel1.Controls.Add(this.comboBoxEQList);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(603, 60);
            this.panel1.TabIndex = 21;
            // 
            // btnOpen
            // 
            this.btnOpen.FlatAppearance.BorderSize = 0;
            this.btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.Location = new System.Drawing.Point(533, 67);
            this.btnOpen.Margin = new System.Windows.Forms.Padding(5);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(40, 29);
            this.btnOpen.TabIndex = 24;
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Visible = false;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click_1);
            // 
            // txtPath
            // 
            this.txtPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtPath.Location = new System.Drawing.Point(137, 71);
            this.txtPath.Margin = new System.Windows.Forms.Padding(5);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(386, 21);
            this.txtPath.TabIndex = 23;
            this.txtPath.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(6, 74);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(91, 17);
            this.label9.TabIndex = 22;
            this.label9.Text = "Pattern Name:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label9.Visible = false;
            // 
            // pictureBoxEnable
            // 
            this.pictureBoxEnable.Image = global::iJedha.Automation.EAP.UI.Properties.Resources.BitOff;
            this.pictureBoxEnable.Location = new System.Drawing.Point(140, 113);
            this.pictureBoxEnable.Name = "pictureBoxEnable";
            this.pictureBoxEnable.Size = new System.Drawing.Size(19, 18);
            this.pictureBoxEnable.TabIndex = 21;
            this.pictureBoxEnable.TabStop = false;
            this.pictureBoxEnable.Visible = false;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label14.Location = new System.Drawing.Point(6, 114);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(50, 17);
            this.label14.TabIndex = 18;
            this.label14.Text = "Enable:";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label14.Visible = false;
            // 
            // comboBoxEQList
            // 
            this.comboBoxEQList.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBoxEQList.FormattingEnabled = true;
            this.comboBoxEQList.Location = new System.Drawing.Point(137, 22);
            this.comboBoxEQList.Name = "comboBoxEQList";
            this.comboBoxEQList.Size = new System.Drawing.Size(142, 25);
            this.comboBoxEQList.TabIndex = 0;
            this.comboBoxEQList.SelectedIndexChanged += new System.EventHandler(this.comboBoxEQList_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(6, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 17);
            this.label5.TabIndex = 0;
            this.label5.Text = "Equipment Name:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnCancel
            // 
            this.btnCancel.Enabled = false;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.Location = new System.Drawing.Point(413, 319);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(86, 38);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.White;
            this.btnSave.Enabled = false;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Image = global::iJedha.Automation.EAP.UI.Properties.Resources.button;
            this.btnSave.Location = new System.Drawing.Point(319, 319);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(86, 38);
            this.btnSave.TabIndex = 18;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.FlatAppearance.BorderSize = 0;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEdit.Image = global::iJedha.Automation.EAP.UI.Properties.Resources.button;
            this.btnEdit.Location = new System.Drawing.Point(225, 319);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(86, 38);
            this.btnEdit.TabIndex = 19;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnExit
            // 
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit.Image = ((System.Drawing.Image)(resources.GetObject("btnExit.Image")));
            this.btnExit.Location = new System.Drawing.Point(505, 319);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(86, 38);
            this.btnExit.TabIndex = 20;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnReopen
            // 
            this.btnReopen.FlatAppearance.BorderSize = 0;
            this.btnReopen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReopen.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReopen.Image = global::iJedha.Automation.EAP.UI.Properties.Resources.button;
            this.btnReopen.Location = new System.Drawing.Point(133, 319);
            this.btnReopen.Name = "btnReopen";
            this.btnReopen.Size = new System.Drawing.Size(86, 38);
            this.btnReopen.TabIndex = 22;
            this.btnReopen.Text = "Reopen";
            this.btnReopen.UseVisualStyleBackColor = true;
            this.btnReopen.Visible = false;
            this.btnReopen.Click += new System.EventHandler(this.btnReopen_Click);
            // 
            // frmSOCKETParameter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Azure;
            this.ClientSize = new System.Drawing.Size(603, 379);
            this.Controls.Add(this.btnReopen);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.gbSetting);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnExit);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSOCKETParameter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SOCKET Setting";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmHSMSParameter_FormClosed);
            this.Load += new System.EventHandler(this.frmHSMSParameter_Load);
            this.gbSetting.ResumeLayout(false);
            this.gbSetting.PerformLayout();
            this.gbNetStat.ResumeLayout(false);
            this.gbTime.ResumeLayout(false);
            this.gbTime.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEnable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbSetting;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtRemotePort;
        private System.Windows.Forms.Label lblMsg;
        private System.Windows.Forms.GroupBox gbTime;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLocalIP;
        private System.Windows.Forms.TextBox txtRemoteIP;
        private System.Windows.Forms.TextBox txtLocalPort;
        private System.Windows.Forms.ComboBox cboConnectMode;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox comboBoxEQList;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.PictureBox pictureBoxEnable;
        private System.Windows.Forms.Button btnReopen;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cboEncodeType;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox gbNetStat;
        private System.Windows.Forms.Label lblNetStatus;
        private System.Windows.Forms.Button btnNetStatusReflash;
    }
}
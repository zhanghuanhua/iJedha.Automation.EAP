namespace iJedha.Automation.EAP.UI
{
    partial class frmEquipment
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEquipment));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.PPTEQInfo = new System.Windows.Forms.PropertyGrid();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.treeViewEQ = new System.Windows.Forms.TreeView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.66666F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(820, 659);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.PPTEQInfo, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(276, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 655F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(541, 653);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // PPTEQInfo
            // 
            this.PPTEQInfo.BackColor = System.Drawing.Color.Azure;
            this.PPTEQInfo.CategorySplitterColor = System.Drawing.Color.Black;
            this.PPTEQInfo.CommandsDisabledLinkColor = System.Drawing.Color.Red;
            this.PPTEQInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PPTEQInfo.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.PPTEQInfo.Location = new System.Drawing.Point(1, 1);
            this.PPTEQInfo.Margin = new System.Windows.Forms.Padding(1);
            this.PPTEQInfo.Name = "PPTEQInfo";
            this.PPTEQInfo.SelectedItemWithFocusForeColor = System.Drawing.Color.Black;
            this.PPTEQInfo.Size = new System.Drawing.Size(539, 651);
            this.PPTEQInfo.TabIndex = 39;
            this.PPTEQInfo.ViewBackColor = System.Drawing.Color.White;
            this.PPTEQInfo.ViewForeColor = System.Drawing.Color.Black;
            this.PPTEQInfo.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.PPTEQInfo_PropertyValueChanged);
            this.PPTEQInfo.Click += new System.EventHandler(this.PPTEQInfo_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.treeViewEQ, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 655F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(267, 653);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // treeViewEQ
            // 
            this.treeViewEQ.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewEQ.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.treeViewEQ.Location = new System.Drawing.Point(3, 3);
            this.treeViewEQ.Name = "treeViewEQ";
            this.treeViewEQ.Size = new System.Drawing.Size(261, 647);
            this.treeViewEQ.TabIndex = 1;
            this.treeViewEQ.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewEQ_AfterSelect);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 659);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(820, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // frmEquipment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Azure;
            this.ClientSize = new System.Drawing.Size(820, 681);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmEquipment";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Equipment";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmEquipment_FormClosed);
            this.Load += new System.EventHandler(this.frmEquipment_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TreeView treeViewEQ;
        private System.Windows.Forms.PropertyGrid PPTEQInfo;
        private System.Windows.Forms.StatusStrip statusStrip1;
    }
}
namespace DMRDataBridge
{
    partial class MainDisplay
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
            this.btnConnect = new System.Windows.Forms.Button();
            this.tmrPing = new System.Windows.Forms.Timer(this.components);
            this.toolStripMain = new System.Windows.Forms.ToolStrip();
            this.toolStripLabelConStatus = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelStatus = new System.Windows.Forms.ToolStripLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.labelPacketCount = new System.Windows.Forms.Label();
            this.lblPacket = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblRptrId = new System.Windows.Forms.Label();
            this.listBoxCalls = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.labelProcPacketCount = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.toolStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(18, 18);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(112, 35);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // tmrPing
            // 
            this.tmrPing.Interval = 5000;
            this.tmrPing.Tick += new System.EventHandler(this.tmrPing_Tick);
            // 
            // toolStripMain
            // 
            this.toolStripMain.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStripMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelConStatus,
            this.toolStripSeparator1,
            this.toolStripLabelStatus});
            this.toolStripMain.Location = new System.Drawing.Point(0, 391);
            this.toolStripMain.Name = "toolStripMain";
            this.toolStripMain.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.toolStripMain.Size = new System.Drawing.Size(1242, 30);
            this.toolStripMain.TabIndex = 2;
            this.toolStripMain.Text = "toolStrip1";
            // 
            // toolStripLabelConStatus
            // 
            this.toolStripLabelConStatus.Name = "toolStripLabelConStatus";
            this.toolStripLabelConStatus.Size = new System.Drawing.Size(119, 25);
            this.toolStripLabelConStatus.Text = "Disconnected";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 30);
            // 
            // toolStripLabelStatus
            // 
            this.toolStripLabelStatus.Name = "toolStripLabelStatus";
            this.toolStripLabelStatus.Size = new System.Drawing.Size(84, 25);
            this.toolStripLabelStatus.Text = "Waiting...";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 65);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "DMRD Packets: ";
            // 
            // labelPacketCount
            // 
            this.labelPacketCount.AutoSize = true;
            this.labelPacketCount.Location = new System.Drawing.Point(229, 65);
            this.labelPacketCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPacketCount.Name = "labelPacketCount";
            this.labelPacketCount.Size = new System.Drawing.Size(18, 20);
            this.labelPacketCount.TabIndex = 4;
            this.labelPacketCount.Text = "0";
            // 
            // lblPacket
            // 
            this.lblPacket.AutoSize = true;
            this.lblPacket.Location = new System.Drawing.Point(537, 94);
            this.lblPacket.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPacket.Name = "lblPacket";
            this.lblPacket.Size = new System.Drawing.Size(18, 20);
            this.lblPacket.TabIndex = 5;
            this.lblPacket.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(326, 94);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(146, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Packet Frame Type";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(326, 65);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 20);
            this.label3.TabIndex = 3;
            this.label3.Text = "Repeater ID";
            // 
            // lblRptrId
            // 
            this.lblRptrId.AutoSize = true;
            this.lblRptrId.Location = new System.Drawing.Point(537, 65);
            this.lblRptrId.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRptrId.Name = "lblRptrId";
            this.lblRptrId.Size = new System.Drawing.Size(18, 20);
            this.lblRptrId.TabIndex = 5;
            this.lblRptrId.Text = "0";
            // 
            // listBoxCalls
            // 
            this.listBoxCalls.FormattingEnabled = true;
            this.listBoxCalls.ItemHeight = 20;
            this.listBoxCalls.Location = new System.Drawing.Point(18, 134);
            this.listBoxCalls.Name = "listBoxCalls";
            this.listBoxCalls.ScrollAlwaysVisible = true;
            this.listBoxCalls.Size = new System.Drawing.Size(1212, 244);
            this.listBoxCalls.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 94);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(206, 20);
            this.label4.TabIndex = 3;
            this.label4.Text = "DMRD Packets Processed: ";
            // 
            // labelProcPacketCount
            // 
            this.labelProcPacketCount.AutoSize = true;
            this.labelProcPacketCount.Location = new System.Drawing.Point(229, 94);
            this.labelProcPacketCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelProcPacketCount.Name = "labelProcPacketCount";
            this.labelProcPacketCount.Size = new System.Drawing.Size(18, 20);
            this.labelProcPacketCount.TabIndex = 4;
            this.labelProcPacketCount.Text = "0";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(1124, 95);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(106, 33);
            this.btnClear.TabIndex = 7;
            this.btnClear.Text = "Clear List";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // MainDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1242, 421);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.listBoxCalls);
            this.Controls.Add(this.lblRptrId);
            this.Controls.Add(this.lblPacket);
            this.Controls.Add(this.labelProcPacketCount);
            this.Controls.Add(this.labelPacketCount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.toolStripMain);
            this.Controls.Add(this.btnConnect);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainDisplay";
            this.Text = "DMR Data Bridge";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainDisplay_FormClosed);
            this.toolStripMain.ResumeLayout(false);
            this.toolStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Timer tmrPing;
        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripLabel toolStripLabelConStatus;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabelStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelPacketCount;
        private System.Windows.Forms.Label lblPacket;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblRptrId;
        private System.Windows.Forms.ListBox listBoxCalls;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelProcPacketCount;
        private System.Windows.Forms.Button btnClear;
    }
}


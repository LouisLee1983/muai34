namespace WF_MUAI_34
{
    partial class B3BForm
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
            components = new System.ComponentModel.Container();
            splitContainer1 = new SplitContainer();
            textBoxUpperPriceAdd = new TextBox();
            textBoxAddLowerPrice = new TextBox();
            checkBoxUsePrice = new CheckBox();
            buttonPostAllPolicy = new Button();
            buttonShowDBData = new Button();
            dataGridView1 = new DataGridView();
            buttonPostPolicy = new Button();
            buttonCloseB3B = new Button();
            buttonSaveSession = new Button();
            buttonOpenB3B = new Button();
            label1 = new Label();
            buttonDelAllPolicy = new Button();
            tabControl1 = new TabControl();
            tabPageB3B = new TabPage();
            tabPage2 = new TabPage();
            webViewB3B = new Microsoft.Web.WebView2.WinForms.WebView2();
            richTextBoxLog = new RichTextBox();
            dateTimePickerDeleteTime = new DateTimePicker();
            checkBoxAutoDelete = new CheckBox();
            labelDeleteStatus = new Label();
            contextMenuStripStatus = new ContextMenuStrip(components);
            toolStripMenuItemShowStatus = new ToolStripMenuItem();
            toolStripMenuItemTestDelete = new ToolStripMenuItem();
            toolStripMenuItemResetStatus = new ToolStripMenuItem();
            buttonToggleTimer = new Button();
            toolTip1 = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tabControl1.SuspendLayout();
            tabPageB3B.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webViewB3B).BeginInit();
            contextMenuStripStatus.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(richTextBoxLog);
            splitContainer1.Panel1.Controls.Add(label1);
            splitContainer1.Panel1.Controls.Add(textBoxUpperPriceAdd);
            splitContainer1.Panel1.Controls.Add(textBoxAddLowerPrice);
            splitContainer1.Panel1.Controls.Add(checkBoxUsePrice);
            splitContainer1.Panel1.Controls.Add(buttonPostAllPolicy);
            splitContainer1.Panel1.Controls.Add(buttonShowDBData);
            splitContainer1.Panel1.Controls.Add(dataGridView1);
            splitContainer1.Panel1.Controls.Add(buttonPostPolicy);
            splitContainer1.Panel1.Controls.Add(buttonCloseB3B);
            splitContainer1.Panel1.Controls.Add(buttonSaveSession);
            splitContainer1.Panel1.Controls.Add(buttonOpenB3B);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tabControl1);
            splitContainer1.Size = new Size(1344, 801);
            splitContainer1.SplitterDistance = 295;
            splitContainer1.TabIndex = 0;
            // 
            // textBoxUpperPriceAdd
            // 
            textBoxUpperPriceAdd.Location = new Point(153, 30);
            textBoxUpperPriceAdd.Name = "textBoxUpperPriceAdd";
            textBoxUpperPriceAdd.Size = new Size(47, 23);
            textBoxUpperPriceAdd.TabIndex = 9;
            textBoxUpperPriceAdd.Text = "500";
            // 
            // textBoxAddLowerPrice
            // 
            textBoxAddLowerPrice.Location = new Point(89, 30);
            textBoxAddLowerPrice.Name = "textBoxAddLowerPrice";
            textBoxAddLowerPrice.Size = new Size(47, 23);
            textBoxAddLowerPrice.TabIndex = 9;
            textBoxAddLowerPrice.Text = "40";
            // 
            // checkBoxUsePrice
            // 
            checkBoxUsePrice.AutoSize = true;
            checkBoxUsePrice.Checked = true;
            checkBoxUsePrice.CheckState = CheckState.Checked;
            checkBoxUsePrice.Location = new Point(3, 32);
            checkBoxUsePrice.Name = "checkBoxUsePrice";
            checkBoxUsePrice.Size = new Size(87, 21);
            checkBoxUsePrice.TabIndex = 8;
            checkBoxUsePrice.Text = "用价格控制";
            checkBoxUsePrice.UseVisualStyleBackColor = true;
            // 
            // buttonPostAllPolicy
            // 
            buttonPostAllPolicy.Location = new Point(202, 31);
            buttonPostAllPolicy.Name = "buttonPostAllPolicy";
            buttonPostAllPolicy.Size = new Size(90, 23);
            buttonPostAllPolicy.TabIndex = 7;
            buttonPostAllPolicy.Text = "发全部政策";
            buttonPostAllPolicy.UseVisualStyleBackColor = true;
            buttonPostAllPolicy.Click += buttonPostAllPolicy_Click;
            // 
            // buttonShowDBData
            // 
            buttonShowDBData.Location = new Point(110, 60);
            buttonShowDBData.Name = "buttonShowDBData";
            buttonShowDBData.Size = new Size(96, 23);
            buttonShowDBData.TabIndex = 6;
            buttonShowDBData.Text = "刷新数据库";
            buttonShowDBData.UseVisualStyleBackColor = true;
            buttonShowDBData.Click += buttonShowDBData_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(3, 108);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(289, 358);
            dataGridView1.TabIndex = 5;
            // 
            // buttonPostPolicy
            // 
            buttonPostPolicy.Location = new Point(3, 59);
            buttonPostPolicy.Name = "buttonPostPolicy";
            buttonPostPolicy.Size = new Size(101, 23);
            buttonPostPolicy.TabIndex = 4;
            buttonPostPolicy.Text = "发送选中政策";
            buttonPostPolicy.UseVisualStyleBackColor = true;
            buttonPostPolicy.Click += buttonPostPolicy_Click;
            // 
            // buttonCloseB3B
            // 
            buttonCloseB3B.Location = new Point(193, 3);
            buttonCloseB3B.Name = "buttonCloseB3B";
            buttonCloseB3B.Size = new Size(75, 23);
            buttonCloseB3B.TabIndex = 2;
            buttonCloseB3B.Text = "关闭B3B";
            buttonCloseB3B.UseVisualStyleBackColor = true;
            buttonCloseB3B.Click += buttonCloseB3B_Click;
            // 
            // buttonSaveSession
            // 
            buttonSaveSession.Location = new Point(86, 3);
            buttonSaveSession.Name = "buttonSaveSession";
            buttonSaveSession.Size = new Size(101, 23);
            buttonSaveSession.TabIndex = 1;
            buttonSaveSession.Text = "保存当前会话";
            buttonSaveSession.UseVisualStyleBackColor = true;
            buttonSaveSession.Click += buttonSaveSession_Click;
            // 
            // buttonOpenB3B
            // 
            buttonOpenB3B.Location = new Point(3, 3);
            buttonOpenB3B.Name = "buttonOpenB3B";
            buttonOpenB3B.Size = new Size(75, 23);
            buttonOpenB3B.TabIndex = 0;
            buttonOpenB3B.Text = "打开B3B";
            buttonOpenB3B.UseVisualStyleBackColor = true;
            buttonOpenB3B.Click += buttonOpenB3B_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(137, 33);
            label1.Name = "label1";
            label1.Size = new Size(13, 17);
            label1.TabIndex = 10;
            label1.Text = "-";
            // 
            // buttonDelAllPolicy
            // 
            buttonDelAllPolicy.Location = new Point(6, 5);
            buttonDelAllPolicy.Name = "buttonDelAllPolicy";
            buttonDelAllPolicy.Size = new Size(87, 23);
            buttonDelAllPolicy.TabIndex = 11;
            buttonDelAllPolicy.Text = "删除B3B政策";
            buttonDelAllPolicy.UseVisualStyleBackColor = true;
            buttonDelAllPolicy.Click += buttonDelAllPolicy_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPageB3B);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(3, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1039, 795);
            tabControl1.TabIndex = 0;
            // 
            // tabPageB3B
            // 
            tabPageB3B.Controls.Add(buttonToggleTimer);
            tabPageB3B.Controls.Add(labelDeleteStatus);
            tabPageB3B.Controls.Add(dateTimePickerDeleteTime);
            tabPageB3B.Controls.Add(checkBoxAutoDelete);
            tabPageB3B.Controls.Add(buttonDelAllPolicy);
            tabPageB3B.Controls.Add(webViewB3B);
            tabPageB3B.Location = new Point(4, 26);
            tabPageB3B.Name = "tabPageB3B";
            tabPageB3B.Padding = new Padding(3);
            tabPageB3B.Size = new Size(1031, 765);
            tabPageB3B.TabIndex = 0;
            tabPageB3B.Text = "B3B";
            tabPageB3B.UseVisualStyleBackColor = true;
            // 
            // buttonToggleTimer
            // 
            buttonToggleTimer.Location = new Point(350, 5);
            buttonToggleTimer.Name = "buttonToggleTimer";
            buttonToggleTimer.Size = new Size(80, 23);
            buttonToggleTimer.TabIndex = 15;
            buttonToggleTimer.Text = "启动定时";
            toolTip1.SetToolTip(buttonToggleTimer, "点击启动或关闭定时删除任务");
            buttonToggleTimer.UseVisualStyleBackColor = true;
            buttonToggleTimer.Click += buttonToggleTimer_Click;
            // 
            // labelDeleteStatus
            // 
            labelDeleteStatus.AutoSize = true;
            labelDeleteStatus.ContextMenuStrip = contextMenuStripStatus;
            labelDeleteStatus.ForeColor = Color.Gray;
            labelDeleteStatus.Location = new Point(266, 9);
            labelDeleteStatus.Name = "labelDeleteStatus";
            labelDeleteStatus.Size = new Size(56, 17);
            labelDeleteStatus.TabIndex = 14;
            labelDeleteStatus.Text = "状态：未启用";
            toolTip1.SetToolTip(labelDeleteStatus, "双击查看详细状态，右键打开菜单");
            labelDeleteStatus.DoubleClick += labelDeleteStatus_DoubleClick;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 26);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(192, 70);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // webViewB3B
            // 
            webViewB3B.AllowExternalDrop = true;
            webViewB3B.CreationProperties = null;
            webViewB3B.DefaultBackgroundColor = Color.White;
            webViewB3B.Location = new Point(3, 31);
            webViewB3B.Name = "webViewB3B";
            webViewB3B.Size = new Size(1022, 728);
            webViewB3B.TabIndex = 0;
            webViewB3B.ZoomFactor = 1D;
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Location = new Point(4, 472);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.Size = new Size(288, 322);
            richTextBoxLog.TabIndex = 11;
            richTextBoxLog.Text = "";
            // 
            // dateTimePickerDeleteTime
            // 
            dateTimePickerDeleteTime.Format = DateTimePickerFormat.Time;
            dateTimePickerDeleteTime.Location = new Point(180, 5);
            dateTimePickerDeleteTime.Name = "dateTimePickerDeleteTime";
            dateTimePickerDeleteTime.ShowUpDown = true;
            dateTimePickerDeleteTime.Size = new Size(80, 23);
            dateTimePickerDeleteTime.TabIndex = 13;
            toolTip1.SetToolTip(dateTimePickerDeleteTime, "设置每天执行删除的时间");
            dateTimePickerDeleteTime.Value = new DateTime(2024, 1, 1, 23, 58, 0);
            dateTimePickerDeleteTime.ValueChanged += dateTimePickerDeleteTime_ValueChanged;
            // 
            // checkBoxAutoDelete
            // 
            checkBoxAutoDelete.AutoSize = true;
            checkBoxAutoDelete.Location = new Point(99, 7);
            checkBoxAutoDelete.Name = "checkBoxAutoDelete";
            checkBoxAutoDelete.Size = new Size(75, 21);
            checkBoxAutoDelete.TabIndex = 12;
            checkBoxAutoDelete.Text = "定时删除";
            toolTip1.SetToolTip(checkBoxAutoDelete, "勾选以启用定时删除功能");
            checkBoxAutoDelete.UseVisualStyleBackColor = true;
            checkBoxAutoDelete.CheckedChanged += checkBoxAutoDelete_CheckedChanged;
            // 
            // contextMenuStripStatus
            // 
            contextMenuStripStatus.Items.AddRange(new ToolStripItem[] { toolStripMenuItemShowStatus, toolStripMenuItemTestDelete, toolStripMenuItemResetStatus });
            contextMenuStripStatus.Name = "contextMenuStripStatus";
            contextMenuStripStatus.Size = new Size(139, 70);
            // 
            // toolStripMenuItemShowStatus
            // 
            toolStripMenuItemShowStatus.Name = "toolStripMenuItemShowStatus";
            toolStripMenuItemShowStatus.Size = new Size(138, 22);
            toolStripMenuItemShowStatus.Text = "显示详细状态";
            toolStripMenuItemShowStatus.Click += toolStripMenuItemShowStatus_Click;
            // 
            // toolStripMenuItemTestDelete
            // 
            toolStripMenuItemTestDelete.Name = "toolStripMenuItemTestDelete";
            toolStripMenuItemTestDelete.Size = new Size(138, 22);
            toolStripMenuItemTestDelete.Text = "测试删除功能";
            toolStripMenuItemTestDelete.Click += toolStripMenuItemTestDelete_Click;
            // 
            // toolStripMenuItemResetStatus
            // 
            toolStripMenuItemResetStatus.Name = "toolStripMenuItemResetStatus";
            toolStripMenuItemResetStatus.Size = new Size(138, 22);
            toolStripMenuItemResetStatus.Text = "重置状态";
            toolStripMenuItemResetStatus.Click += toolStripMenuItemResetStatus_Click;
            // 
            // B3BForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1344, 801);
            Controls.Add(splitContainer1);
            Name = "B3BForm";
            Text = "B3BForm";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tabControl1.ResumeLayout(false);
            tabPageB3B.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webViewB3B).EndInit();
            contextMenuStripStatus.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private Button buttonOpenB3B;
        private Button buttonCloseB3B;
        private Button buttonSaveSession;
        private Button buttonPostPolicy;
        private DataGridView dataGridView1;
        private Button buttonShowDBData;
        private Button buttonPostAllPolicy;
        private CheckBox checkBoxUsePrice;
        private TextBox textBoxUpperPriceAdd;
        private TextBox textBoxAddLowerPrice;
        private Label label1;
        private Button buttonDelAllPolicy;
        private TabControl tabControl1;
        private TabPage tabPageB3B;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewB3B;
        private TabPage tabPage2;
        private RichTextBox richTextBoxLog;
        private DateTimePicker dateTimePickerDeleteTime;
        private CheckBox checkBoxAutoDelete;
        private Label labelDeleteStatus;
        private ContextMenuStrip contextMenuStripStatus;
        private ToolStripMenuItem toolStripMenuItemShowStatus;
        private ToolStripMenuItem toolStripMenuItemTestDelete;
        private ToolStripMenuItem toolStripMenuItemResetStatus;
        private Button buttonToggleTimer;
        private ToolTip toolTip1;
    }
}
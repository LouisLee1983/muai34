
namespace WF_MUAI_34
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            contextMenuStrip1 = new ContextMenuStrip(components);
            设置ToolStripMenuItem = new ToolStripMenuItem();
            网址ToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1 = new MenuStrip();
            设置ToolStripMenuItem1 = new ToolStripMenuItem();
            日志ToolStripMenuItem = new ToolStripMenuItem();
            计划任务ToolStripMenuItem = new ToolStripMenuItem();
            buttonImportMuExcel = new Button();
            splitContainer1 = new SplitContainer();
            buttonClearTable = new Button();
            buttonEditAll = new Button();
            buttonOpenB3BForm = new Button();
            buttonEditSelectedItem = new Button();
            dataGridViewMuOrgExcel = new DataGridView();
            contextMenuStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewMuOrgExcel).BeginInit();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { 设置ToolStripMenuItem, 网址ToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(101, 48);
            // 
            // 设置ToolStripMenuItem
            // 
            设置ToolStripMenuItem.Name = "设置ToolStripMenuItem";
            设置ToolStripMenuItem.Size = new Size(100, 22);
            设置ToolStripMenuItem.Text = "设置";
            // 
            // 网址ToolStripMenuItem
            // 
            网址ToolStripMenuItem.Name = "网址ToolStripMenuItem";
            网址ToolStripMenuItem.Size = new Size(100, 22);
            网址ToolStripMenuItem.Text = "网址";
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { 设置ToolStripMenuItem1, 日志ToolStripMenuItem, 计划任务ToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1090, 25);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // 设置ToolStripMenuItem1
            // 
            设置ToolStripMenuItem1.Name = "设置ToolStripMenuItem1";
            设置ToolStripMenuItem1.Size = new Size(44, 21);
            设置ToolStripMenuItem1.Text = "设置";
            // 
            // 日志ToolStripMenuItem
            // 
            日志ToolStripMenuItem.Name = "日志ToolStripMenuItem";
            日志ToolStripMenuItem.Size = new Size(44, 21);
            日志ToolStripMenuItem.Text = "日志";
            // 
            // 计划任务ToolStripMenuItem
            // 
            计划任务ToolStripMenuItem.Name = "计划任务ToolStripMenuItem";
            计划任务ToolStripMenuItem.Size = new Size(68, 21);
            计划任务ToolStripMenuItem.Text = "计划任务";
            // 
            // buttonImportMuExcel
            // 
            buttonImportMuExcel.Location = new Point(3, 12);
            buttonImportMuExcel.Name = "buttonImportMuExcel";
            buttonImportMuExcel.Size = new Size(100, 23);
            buttonImportMuExcel.TabIndex = 0;
            buttonImportMuExcel.Text = "1导入闪惠excel";
            buttonImportMuExcel.Click += buttonImportMuExcel_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 25);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(buttonClearTable);
            splitContainer1.Panel1.Controls.Add(buttonEditAll);
            splitContainer1.Panel1.Controls.Add(buttonOpenB3BForm);
            splitContainer1.Panel1.Controls.Add(buttonEditSelectedItem);
            splitContainer1.Panel1.Controls.Add(buttonImportMuExcel);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(dataGridViewMuOrgExcel);
            splitContainer1.Size = new Size(1090, 641);
            splitContainer1.SplitterDistance = 206;
            splitContainer1.TabIndex = 3;
            // 
            // buttonClearTable
            // 
            buttonClearTable.Location = new Point(109, 70);
            buttonClearTable.Name = "buttonClearTable";
            buttonClearTable.Size = new Size(94, 23);
            buttonClearTable.TabIndex = 4;
            buttonClearTable.Text = "2清楚旧数据表";
            buttonClearTable.UseVisualStyleBackColor = true;
            buttonClearTable.Click += buttonClearTable_Click;
            // 
            // buttonEditAll
            // 
            buttonEditAll.Location = new Point(3, 70);
            buttonEditAll.Name = "buttonEditAll";
            buttonEditAll.Size = new Size(100, 23);
            buttonEditAll.TabIndex = 3;
            buttonEditAll.Text = "3全部转换";
            buttonEditAll.UseVisualStyleBackColor = true;
            buttonEditAll.Click += buttonEditAll_Click;
            // 
            // buttonOpenB3BForm
            // 
            buttonOpenB3BForm.Location = new Point(3, 106);
            buttonOpenB3BForm.Name = "buttonOpenB3BForm";
            buttonOpenB3BForm.Size = new Size(75, 23);
            buttonOpenB3BForm.TabIndex = 2;
            buttonOpenB3BForm.Text = "4打开B3B";
            buttonOpenB3BForm.UseVisualStyleBackColor = true;
            buttonOpenB3BForm.Click += buttonOpenB3BForm_Click;
            // 
            // buttonEditSelectedItem
            // 
            buttonEditSelectedItem.Location = new Point(3, 41);
            buttonEditSelectedItem.Name = "buttonEditSelectedItem";
            buttonEditSelectedItem.Size = new Size(75, 23);
            buttonEditSelectedItem.TabIndex = 1;
            buttonEditSelectedItem.Text = "转换选中";
            buttonEditSelectedItem.UseVisualStyleBackColor = true;
            buttonEditSelectedItem.Click += buttonEditSelectedItem_Click;
            // 
            // dataGridViewMuOrgExcel
            // 
            dataGridViewMuOrgExcel.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewMuOrgExcel.Dock = DockStyle.Fill;
            dataGridViewMuOrgExcel.Location = new Point(0, 0);
            dataGridViewMuOrgExcel.Name = "dataGridViewMuOrgExcel";
            dataGridViewMuOrgExcel.Size = new Size(880, 641);
            dataGridViewMuOrgExcel.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1090, 666);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "东航产品管理工具";
            Load += MainForm_Load;
            contextMenuStrip1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewMuOrgExcel).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }



        #endregion

        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem 设置ToolStripMenuItem;
        private ToolStripMenuItem 网址ToolStripMenuItem;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem 设置ToolStripMenuItem1;
        private ToolStripMenuItem 日志ToolStripMenuItem;
        private ToolStripMenuItem 计划任务ToolStripMenuItem;
        private Button buttonImportMuExcel;
        private SplitContainer splitContainer1;
        private Button buttonEditSelectedItem;
        private DataGridView dataGridViewMuOrgExcel;
        private Button buttonOpenB3BForm;
        private Button buttonEditAll;
        private Button buttonClearTable;
    }
}

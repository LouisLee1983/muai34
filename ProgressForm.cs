using System;
using System.Drawing;
using System.Windows.Forms;

namespace WF_MUAI_34
{
    public partial class ProgressForm : Form
    {
        private ProgressBar progressBar = null!;
        private Label labelStatus = null!;
        private Label labelProgress = null!;
        private Button buttonCancel = null!;
        private int totalCount;
        private bool isCompleted = false;

        public ProgressForm(int total)
        {
            totalCount = total;
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form 属性
            this.Text = "批量发布进度";
            this.Size = new Size(500, 240);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            // 允许用户关闭窗口
            
            this.ResumeLayout(false);
        }

        private void InitializeCustomComponents()
        {
            // 创建标题标签
            var labelTitle = new Label();
            labelTitle.Text = "正在批量发布价格政策...";
            labelTitle.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            labelTitle.Location = new Point(20, 20);
            labelTitle.Size = new Size(460, 30);
            labelTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(labelTitle);

            // 创建进度条
            progressBar = new ProgressBar();
            progressBar.Location = new Point(20, 60);
            progressBar.Size = new Size(460, 30);
            progressBar.Minimum = 0;
            progressBar.Maximum = totalCount;
            progressBar.Value = 0;
            progressBar.Style = ProgressBarStyle.Continuous;
            this.Controls.Add(progressBar);

            // 创建进度文字标签
            labelProgress = new Label();
            labelProgress.Text = $"0 / {totalCount}";
            labelProgress.Location = new Point(20, 100);
            labelProgress.Size = new Size(460, 25);
            labelProgress.TextAlign = ContentAlignment.MiddleCenter;
            labelProgress.Font = new Font("Microsoft YaHei", 10F);
            this.Controls.Add(labelProgress);

            // 创建状态标签
            labelStatus = new Label();
            labelStatus.Text = "准备开始...";
            labelStatus.Location = new Point(20, 130);
            labelStatus.Size = new Size(460, 25);
            labelStatus.TextAlign = ContentAlignment.MiddleCenter;
            labelStatus.Font = new Font("Microsoft YaHei", 9F);
            labelStatus.ForeColor = Color.Gray;
            this.Controls.Add(labelStatus);

            // 创建取消按钮
            buttonCancel = new Button();
            buttonCancel.Text = "取消";
            buttonCancel.Location = new Point(210, 165);
            buttonCancel.Size = new Size(80, 35);
            buttonCancel.Font = new Font("Microsoft YaHei", 9F);
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += ButtonCancel_Click;
            this.Controls.Add(buttonCancel);
        }

        /// <summary>
        /// 更新进度
        /// </summary>
        /// <param name="currentCount">当前完成数量</param>
        /// <param name="statusText">状态文本</param>
        public void UpdateProgress(int currentCount, string statusText = "")
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int, string>(UpdateProgress), currentCount, statusText);
                return;
            }

            // 更新进度条
            progressBar.Value = Math.Min(currentCount, progressBar.Maximum);

            // 更新进度文字
            double percentage = totalCount > 0 ? (double)currentCount / totalCount * 100 : 0;
            labelProgress.Text = $"{currentCount} / {totalCount} ({percentage:F1}%)";

            // 更新状态文字
            if (!string.IsNullOrEmpty(statusText))
            {
                labelStatus.Text = statusText;
            }

            // 强制刷新界面
            this.Update();
            Application.DoEvents();
        }

        /// <summary>
        /// 设置完成状态
        /// </summary>
        public void SetCompleted()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(SetCompleted));
                return;
            }

            isCompleted = true;
            progressBar.Value = progressBar.Maximum;
            labelProgress.Text = $"{totalCount} / {totalCount} (100.0%)";
            labelStatus.Text = "批量发布完成！";
            labelStatus.ForeColor = Color.Green;
            buttonCancel.Text = "关闭";
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void ButtonCancel_Click(object? sender, EventArgs e)
        {
            if (isCompleted)
            {
                // 如果已完成，直接关闭窗口
                this.Close();
            }
            else
            {
                // 如果未完成，询问用户是否确定要取消
                var result = MessageBox.Show("批量发布正在进行中，确定要取消吗？\n\n注意：已发布的数据不会被撤销。", 
                    "确认取消", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    this.Close();
                }
            }
        }

        /// <summary>
        /// 处理窗口关闭事件
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 如果是程序代码关闭或者已完成，允许关闭
            if (e.CloseReason != CloseReason.UserClosing || isCompleted)
            {
                base.OnFormClosing(e);
                return;
            }

            // 如果是用户尝试关闭且未完成，询问确认
            var result = MessageBox.Show("批量发布正在进行中，确定要取消吗？\n\n注意：已发布的数据不会被撤销。", 
                "确认取消", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            
            base.OnFormClosing(e);
        }
    }
} 
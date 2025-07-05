using System;
using System.Drawing;
using System.Windows.Forms;

namespace WF_MUAI_34
{
    public partial class CaptchaInputForm : Form
    {
        public string CaptchaText { get; private set; } = string.Empty;
        private string imagePath;
        private PictureBox pictureBox;
        private TextBox textBoxCaptcha;
        private Button buttonOK;
        private Button buttonCancel;

        public CaptchaInputForm(string imagePath)
        {
            this.imagePath = imagePath;
            InitializeComponent();
            LoadCaptchaImage();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // 窗体设置
            this.Text = "请输入验证码";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(350, 250);
            
            // 验证码图片显示
            pictureBox = new PictureBox();
            pictureBox.Location = new Point(20, 20);
            pictureBox.Size = new Size(300, 80);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(pictureBox);
            
            // 提示标签
            Label labelPrompt = new Label();
            labelPrompt.Text = "请输入上图中的验证码：";
            labelPrompt.Location = new Point(20, 110);
            labelPrompt.Size = new Size(200, 20);
            this.Controls.Add(labelPrompt);
            
            // 验证码输入框
            textBoxCaptcha = new TextBox();
            textBoxCaptcha.Location = new Point(20, 135);
            textBoxCaptcha.Size = new Size(300, 25);
            textBoxCaptcha.Font = new Font("Arial", 12F, FontStyle.Bold);
            textBoxCaptcha.TextAlign = HorizontalAlignment.Center;
            textBoxCaptcha.KeyPress += TextBoxCaptcha_KeyPress;
            this.Controls.Add(textBoxCaptcha);
            
            // 确定按钮
            buttonOK = new Button();
            buttonOK.Text = "确定";
            buttonOK.Location = new Point(170, 170);
            buttonOK.Size = new Size(70, 30);
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Click += ButtonOK_Click;
            this.Controls.Add(buttonOK);
            
            // 取消按钮
            buttonCancel = new Button();
            buttonCancel.Text = "取消";
            buttonCancel.Location = new Point(250, 170);
            buttonCancel.Size = new Size(70, 30);
            buttonCancel.DialogResult = DialogResult.Cancel;
            this.Controls.Add(buttonCancel);
            
            // 设置默认和取消按钮
            this.AcceptButton = buttonOK;
            this.CancelButton = buttonCancel;
            
            this.ResumeLayout(false);
        }

        private void LoadCaptchaImage()
        {
            try
            {
                if (System.IO.File.Exists(imagePath))
                {
                    pictureBox.Image = Image.FromFile(imagePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载验证码图片失败: {ex.Message}");
            }
        }

        private void TextBoxCaptcha_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 按Enter键相当于点击确定按钮
            if (e.KeyChar == (char)Keys.Enter)
            {
                ButtonOK_Click(sender, e);
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            CaptchaText = textBoxCaptcha.Text.Trim();
            if (string.IsNullOrEmpty(CaptchaText))
            {
                MessageBox.Show("请输入验证码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxCaptcha.Focus();
                return;
            }
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 清理资源
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
            
            // 删除临时文件
            try
            {
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除临时文件失败: {ex.Message}");
            }
            
            base.OnFormClosed(e);
        }
    }
} 
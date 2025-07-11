using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Microsoft.Web.WebView2.WinForms;
using System.Net.Http;
using System.Threading;
using System.Drawing.Printing;
using System.Timers;
using System.IO;
using System.Text.RegularExpressions;

namespace WF_MUAI_34
{
    public partial class B3BForm : Form
    {
        private const string SESSION_PATH = "B3BSession";
        private const string SESSION_FILE = "B3BSession.json";
        private System.Windows.Forms.Timer dailyDeleteTimer = null!; // 每日删除定时器
        private bool isAutoDeleteEnabled = false; // 是否启用自动删除
        private TimeSpan deleteTime = new TimeSpan(23, 58, 0); // 默认删除时间 23:58

        // 定时上传全部相关属性
        private System.Windows.Forms.Timer dailyPostAllTimer = null!; // 每日上传全部定时器
        private bool isAutoPostAllEnabled = false; // 是否启用自动上传全部
        private TimeSpan postAllTime = new TimeSpan(1, 0, 0); // 默认上传全部时间 09:00

        // 自动登录相关属性
        private const string LOGIN_URL = "https://oper.cddyf.net/Login.aspx";
        private bool isAutoLoginEnabled = true; // 是否启用自动登录
        private System.Windows.Forms.Timer loginCheckTimer = null!; // 登录检查定时器

        public B3BForm()
        {
            InitializeComponent();
            InitializeAsync(); // 异步初始化 WebView2 控件
            InitializeDailyDeleteTimer(); // 初始化定时任务
            InitializeDailyPostAllTimer(); // 初始化定时上传全部任务
            InitializeUIControls(); // 初始化UI控件
            InitializeAutoLoginTimer(); // 初始化自动登录检查定时器
        }

        /// <summary>
        /// 初始化UI控件状态
        /// </summary>
        private void InitializeUIControls()
        {
            // 设置默认时间
            dateTimePickerDeleteTime.Value = DateTime.Today.Add(deleteTime);
            checkBoxAutoDelete.Checked = isAutoDeleteEnabled;

            // 设置定时上传全部默认时间
            dateTimePickerPostAllTime.Value = DateTime.Today.Add(postAllTime);
            checkBoxAutoPostAll.Checked = isAutoPostAllEnabled;

            UpdateStatusLabel();
            UpdatePostAllStatusLabel();
        }

        /// <summary>
        /// 更新状态标签
        /// </summary>
        private void UpdateStatusLabel()
        {
            if (isAutoDeleteEnabled)
            {
                string timeStr = deleteTime.ToString(@"hh\:mm");
                labelDeleteStatus.Text = $"状态：已启用 - 每天{timeStr}执行";
                labelDeleteStatus.ForeColor = Color.Green;
            }
            else
            {
                labelDeleteStatus.Text = "状态：未启用";
                labelDeleteStatus.ForeColor = Color.Gray;
            }
        }

        /// <summary>
        /// 更新上传全部状态标签
        /// </summary>
        private void UpdatePostAllStatusLabel()
        {
            if (isAutoPostAllEnabled)
            {
                string timeStr = postAllTime.ToString(@"hh\:mm");
                labelPostAllStatus.Text = $"状态：已启用 - 每天{timeStr}执行";
                labelPostAllStatus.ForeColor = Color.Green;
            }
            else
            {
                labelPostAllStatus.Text = "状态：未启用";
                labelPostAllStatus.ForeColor = Color.Gray;
            }
        }

        /// <summary>
        /// 初始化每日删除定时器
        /// </summary>
        private void InitializeDailyDeleteTimer()
        {
            dailyDeleteTimer = new System.Windows.Forms.Timer();
            dailyDeleteTimer.Interval = 60000; // 每分钟检查一次
            dailyDeleteTimer.Tick += DailyDeleteTimer_Tick;
            // 默认不启动定时器，需要手动启用
        }

        /// <summary>
        /// 初始化每日上传全部定时器
        /// </summary>
        private void InitializeDailyPostAllTimer()
        {
            dailyPostAllTimer = new System.Windows.Forms.Timer();
            dailyPostAllTimer.Interval = 60000; // 每分钟检查一次
            dailyPostAllTimer.Tick += DailyPostAllTimer_Tick;
            // 默认不启动定时器，需要手动启用
        }

        /// <summary>
        /// 定时器事件处理
        /// </summary>
        private async void DailyDeleteTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;

                // 检查是否到了设定的删除时间（允许1分钟的误差）
                if (Math.Abs((currentTime - deleteTime).TotalMinutes) < 1)
                {
                    // 暂停定时器以防止重复触发
                    dailyDeleteTimer.Stop();

                    // 暂时禁用控件
                    checkBoxAutoDelete.Enabled = false;
                    dateTimePickerDeleteTime.Enabled = false;

                    // 更新状态标签
                    if (labelDeleteStatus != null)
                    {
                        labelDeleteStatus.Text = "状态：正在执行删除...";
                        labelDeleteStatus.ForeColor = Color.Orange;
                    }

                    // 异步执行删除操作
                    bool success = await DeleteAllPoliciesAsync();

                    // 恢复控件状态
                    checkBoxAutoDelete.Enabled = true;
                    dateTimePickerDeleteTime.Enabled = true;

                    // 更新状态标签
                    if (labelDeleteStatus != null)
                    {
                        if (success)
                        {
                            labelDeleteStatus.Text = "状态：删除成功 - 等待下次执行";
                            labelDeleteStatus.ForeColor = Color.Blue;
                        }
                        else
                        {
                            labelDeleteStatus.Text = "状态：删除失败 - 等待下次执行";
                            labelDeleteStatus.ForeColor = Color.Red;
                        }
                    }

                    // 等待1分钟后恢复正常状态显示
                    System.Windows.Forms.Timer statusTimer = new System.Windows.Forms.Timer();
                    statusTimer.Interval = 60000; // 1分钟
                    statusTimer.Tick += (s, args) =>
                    {
                        UpdateStatusLabel();
                        statusTimer.Stop();
                        statusTimer.Dispose();
                    };
                    statusTimer.Start();

                    // 重新启动定时器
                    if (isAutoDeleteEnabled)
                    {
                        dailyDeleteTimer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"定时删除任务发生错误: {ex.Message}");

                // 恢复控件状态
                checkBoxAutoDelete.Enabled = true;
                dateTimePickerDeleteTime.Enabled = true;

                // 更新状态标签
                if (labelDeleteStatus != null)
                {
                    labelDeleteStatus.Text = "状态：执行出错 - 等待重试";
                    labelDeleteStatus.ForeColor = Color.Red;
                }

                // 如果发生错误，确保定时器继续运行
                if (isAutoDeleteEnabled && !dailyDeleteTimer.Enabled)
                {
                    dailyDeleteTimer.Start();
                }
            }
        }

        /// <summary>
        /// 定时上传全部事件处理
        /// </summary>
        private async void DailyPostAllTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                TimeSpan currentTime = now.TimeOfDay;

                // 检查是否到了设定的上传全部时间（允许1分钟的误差）
                if (Math.Abs((currentTime - postAllTime).TotalMinutes) < 1)
                {
                    // 暂停定时器以防止重复触发
                    dailyPostAllTimer.Stop();

                    // 暂时禁用控件
                    checkBoxAutoPostAll.Enabled = false;
                    dateTimePickerPostAllTime.Enabled = false;
                    buttonPostAllPolicy.Enabled = false;

                    // 更新状态标签
                    if (labelPostAllStatus != null)
                    {
                        labelPostAllStatus.Text = "状态：正在执行上传...";
                        labelPostAllStatus.ForeColor = Color.Orange;
                    }

                    // 异步执行上传全部操作
                    bool success = await PostAllPoliciesScheduledAsync();

                    // 恢复控件状态
                    checkBoxAutoPostAll.Enabled = true;
                    dateTimePickerPostAllTime.Enabled = true;
                    buttonPostAllPolicy.Enabled = true;

                    // 更新状态标签
                    if (labelPostAllStatus != null)
                    {
                        if (success)
                        {
                            labelPostAllStatus.Text = "状态：上传成功 - 等待下次执行";
                            labelPostAllStatus.ForeColor = Color.Blue;
                        }
                        else
                        {
                            labelPostAllStatus.Text = "状态：上传失败 - 等待下次执行";
                            labelPostAllStatus.ForeColor = Color.Red;
                        }
                    }

                    // 等待1分钟后恢复正常状态显示
                    System.Windows.Forms.Timer statusTimer = new System.Windows.Forms.Timer();
                    statusTimer.Interval = 60000; // 1分钟
                    statusTimer.Tick += (s, args) =>
                    {
                        UpdatePostAllStatusLabel();
                        statusTimer.Stop();
                        statusTimer.Dispose();
                    };
                    statusTimer.Start();

                    // 重新启动定时器
                    if (isAutoPostAllEnabled)
                    {
                        dailyPostAllTimer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"定时上传全部任务发生错误: {ex.Message}");

                // 恢复控件状态
                checkBoxAutoPostAll.Enabled = true;
                dateTimePickerPostAllTime.Enabled = true;
                buttonPostAllPolicy.Enabled = true;

                // 更新状态标签
                if (labelPostAllStatus != null)
                {
                    labelPostAllStatus.Text = "状态：执行出错 - 等待重试";
                    labelPostAllStatus.ForeColor = Color.Red;
                }

                // 如果发生错误，确保定时器继续运行
                if (isAutoPostAllEnabled && !dailyPostAllTimer.Enabled)
                {
                    dailyPostAllTimer.Start();
                }
            }
        }

        /// <summary>
        /// 启用自动删除定时任务
        /// </summary>
        public void EnableAutoDelete()
        {
            isAutoDeleteEnabled = true;
            dailyDeleteTimer.Start();
            UpdateStatusLabel();

            // 同步更新复选框状态（避免重复触发事件）
            checkBoxAutoDelete.CheckedChanged -= checkBoxAutoDelete_CheckedChanged;
            checkBoxAutoDelete.Checked = true;
            checkBoxAutoDelete.CheckedChanged += checkBoxAutoDelete_CheckedChanged;
        }

        /// <summary>
        /// 禁用自动删除定时任务
        /// </summary>
        public void DisableAutoDelete()
        {
            isAutoDeleteEnabled = false;
            dailyDeleteTimer.Stop();
            UpdateStatusLabel();

            // 同步更新复选框状态（避免重复触发事件）
            checkBoxAutoDelete.CheckedChanged -= checkBoxAutoDelete_CheckedChanged;
            checkBoxAutoDelete.Checked = false;
            checkBoxAutoDelete.CheckedChanged += checkBoxAutoDelete_CheckedChanged;
        }

        /// <summary>
        /// 启用自动上传全部定时任务
        /// </summary>
        public void EnableAutoPostAll()
        {
            isAutoPostAllEnabled = true;
            dailyPostAllTimer.Start();
            UpdatePostAllStatusLabel();

            // 同步更新复选框状态（避免重复触发事件）
            checkBoxAutoPostAll.CheckedChanged -= checkBoxAutoPostAll_CheckedChanged;
            checkBoxAutoPostAll.Checked = true;
            checkBoxAutoPostAll.CheckedChanged += checkBoxAutoPostAll_CheckedChanged;
        }

        /// <summary>
        /// 禁用自动上传全部定时任务
        /// </summary>
        public void DisableAutoPostAll()
        {
            isAutoPostAllEnabled = false;
            dailyPostAllTimer.Stop();
            UpdatePostAllStatusLabel();

            // 同步更新复选框状态（避免重复触发事件）
            checkBoxAutoPostAll.CheckedChanged -= checkBoxAutoPostAll_CheckedChanged;
            checkBoxAutoPostAll.Checked = false;
            checkBoxAutoPostAll.CheckedChanged += checkBoxAutoPostAll_CheckedChanged;
        }

        /// <summary>
        /// 设置删除时间
        /// </summary>
        /// <param name="time">删除时间</param>
        public void SetDeleteTime(TimeSpan time)
        {
            deleteTime = time;
        }

        /// <summary>
        /// 获取自动删除状态
        /// </summary>
        public bool IsAutoDeleteEnabled => isAutoDeleteEnabled;

        /// <summary>
        /// 获取删除时间
        /// </summary>
        public TimeSpan DeleteTime => deleteTime;

        /// <summary>
        /// 设置上传全部时间
        /// </summary>
        /// <param name="time">上传全部时间</param>
        public void SetPostAllTime(TimeSpan time)
        {
            postAllTime = time;
        }

        /// <summary>
        /// 获取自动上传全部状态
        /// </summary>
        public bool IsAutoPostAllEnabled => isAutoPostAllEnabled;

        /// <summary>
        /// 获取上传全部时间
        /// </summary>
        public TimeSpan PostAllTime => postAllTime;

        // 窗体关闭时释放定时器资源
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            dailyDeleteTimer?.Stop();
            dailyDeleteTimer?.Dispose();
            dailyPostAllTimer?.Stop();
            dailyPostAllTimer?.Dispose();
            loginCheckTimer?.Stop();
            loginCheckTimer?.Dispose();
            base.OnFormClosed(e);
        }

        /// <summary>
        /// 定时删除勾选框状态改变事件
        /// </summary>
        private void checkBoxAutoDelete_CheckedChanged(object? sender, EventArgs e)
        {
            if (checkBoxAutoDelete.Checked)
            {
                // 从DateTimePicker获取时间设置
                deleteTime = dateTimePickerDeleteTime.Value.TimeOfDay;
                EnableAutoDelete();

                string timeStr = deleteTime.ToString(@"hh\:mm");
                MessageBox.Show($"定时删除已启用！\n每天 {timeStr} 自动删除所有MUAI_34政策。",
                    "定时任务", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                DisableAutoDelete();
                MessageBox.Show("定时删除已禁用。", "定时任务", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 状态标签双击事件 - 显示详细状态
        /// </summary>
        private void labelDeleteStatus_DoubleClick(object sender, EventArgs e)
        {
            ShowAutoDeleteDetailedStatus();
        }

        /// <summary>
        /// 右键菜单 - 显示状态
        /// </summary>
        private void toolStripMenuItemShowStatus_Click(object sender, EventArgs e)
        {
            ShowAutoDeleteDetailedStatus();
        }

        /// <summary>
        /// 右键菜单 - 测试删除功能
        /// </summary>
        private async void toolStripMenuItemTestDelete_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("这将立即执行删除操作！\n确定要测试删除功能吗？",
                "测试删除功能", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // 禁用相关控件
                    buttonDelAllPolicy.Enabled = false;
                    checkBoxAutoDelete.Enabled = false;
                    dateTimePickerDeleteTime.Enabled = false;

                    // 更新状态标签
                    labelDeleteStatus.Text = "状态：正在测试删除...";
                    labelDeleteStatus.ForeColor = Color.Orange;

                    // 执行删除操作
                    bool success = await DeleteAllPoliciesAsync();

                    // 更新状态标签
                    if (success)
                    {
                        labelDeleteStatus.Text = "状态：测试删除成功";
                        labelDeleteStatus.ForeColor = Color.Blue;
                    }
                    else
                    {
                        labelDeleteStatus.Text = "状态：测试删除失败";
                        labelDeleteStatus.ForeColor = Color.Red;
                    }

                    // 3秒后恢复正常状态显示
                    System.Windows.Forms.Timer statusTimer = new System.Windows.Forms.Timer();
                    statusTimer.Interval = 3000;
                    statusTimer.Tick += (s, args) =>
                    {
                        UpdateStatusLabel();
                        statusTimer.Stop();
                        statusTimer.Dispose();
                    };
                    statusTimer.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"测试删除功能时发生错误：{ex.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatusLabel();
                }
                finally
                {
                    // 恢复控件状态
                    buttonDelAllPolicy.Enabled = true;
                    checkBoxAutoDelete.Enabled = true;
                    dateTimePickerDeleteTime.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 右键菜单 - 重置状态
        /// </summary>
        private void toolStripMenuItemResetStatus_Click(object sender, EventArgs e)
        {
            UpdateStatusLabel();
            MessageBox.Show("状态已重置。", "重置状态", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 删除时间设置改变事件
        /// </summary>
        private void dateTimePickerDeleteTime_ValueChanged(object sender, EventArgs e)
        {
            // 更新删除时间
            deleteTime = dateTimePickerDeleteTime.Value.TimeOfDay;

            // 如果定时任务已启用，重新启动定时器并更新状态
            if (isAutoDeleteEnabled)
            {
                dailyDeleteTimer.Stop();
                dailyDeleteTimer.Start();
                UpdateStatusLabel();

                string timeStr = deleteTime.ToString(@"hh\:mm");
                this.Text = $"B3BForm - 定时删除时间已更新: {timeStr}";

                // 3秒后恢复原标题
                System.Windows.Forms.Timer titleTimer = new System.Windows.Forms.Timer();
                titleTimer.Interval = 3000;
                titleTimer.Tick += (s, args) =>
                {
                    this.Text = "B3BForm";
                    titleTimer.Stop();
                    titleTimer.Dispose();
                };
                titleTimer.Start();
            }
        }

        private async void InitializeAsync()
        {
            // 确保 CoreWebView2 已初始化
            await webViewB3B.EnsureCoreWebView2Async(null);
            // 初始化webViewMU

        }

        private async void buttonOpenB3B_Click(object sender, EventArgs e)
        {
            // 在 webViewB3B 中打开 B3B 网站：https://fuwu.cddyf.net/  
            string url = "https://fuwu.cddyf.net/";

            // 检查是否已初始化 CoreWebView2  
            if (webViewB3B.CoreWebView2 == null)
            {
                MessageBox.Show("WebView2 未初始化，请稍候再试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 尝试加载保存的Cookie
            await LoadCookiesAsync();

            // 启动自动登录检查
            EnableAutoLogin();
            System.Diagnostics.Debug.WriteLine("自动登录检查已启动");

            // 导航到目标网站
            webViewB3B.Source = new Uri(url);
            System.Diagnostics.Debug.WriteLine($"导航到: {url}");
        }

        private async void buttonSaveSession_Click(object sender, EventArgs e)
        {
            // 检查WebView2是否已初始化  
            if (webViewB3B.CoreWebView2 == null)
            {
                MessageBox.Show("WebView2 未初始化，无法保存会话。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 创建会话目录  
                if (!System.IO.Directory.Exists(SESSION_PATH))
                {
                    System.IO.Directory.CreateDirectory(SESSION_PATH);
                }

                string sessionFilePath = System.IO.Path.Combine(SESSION_PATH, SESSION_FILE);

                // 获取当前网站的所有Cookie  
                var cookies = await webViewB3B.CoreWebView2.CookieManager.GetCookiesAsync(webViewB3B.Source.ToString());

                if (cookies == null || cookies.Count == 0)
                {
                    MessageBox.Show("当前网站没有找到Cookie，请确保已经登录网站。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 将Cookie转换为可序列化的格式  
                var cookieDataList = new List<CookieData>();
                foreach (var cookie in cookies)
                {
                    var cookieData = new CookieData
                    {
                        Name = cookie.Name,
                        Value = cookie.Value,
                        Domain = cookie.Domain,
                        Path = cookie.Path,
                        Expires = cookie.Expires.ToString("yyyy-MM-dd HH:mm:ss"),
                        IsHttpOnly = cookie.IsHttpOnly,
                        IsSecure = cookie.IsSecure,
                        IsSession = cookie.IsSession
                    };
                    cookieDataList.Add(cookieData);
                }

                // 序列化为JSON并保存到文件  
                string jsonData = JsonConvert.SerializeObject(cookieDataList, Formatting.Indented);
                await System.IO.File.WriteAllTextAsync(sessionFilePath, jsonData);

                MessageBox.Show($"会话已成功保存！共保存了 {cookieDataList.Count} 个Cookie。", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存会话时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadCookiesAsync()
        {
            string sessionFilePath = System.IO.Path.Combine(SESSION_PATH, SESSION_FILE);

            // 检查会话文件是否存在
            if (!System.IO.File.Exists(sessionFilePath))
            {
                return; // 没有保存的会话，直接返回
            }

            try
            {
                // 读取会话数据文件  
                string jsonData = await System.IO.File.ReadAllTextAsync(sessionFilePath);

                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    return;
                }

                // 反序列化 JSON 数据为 Cookie 对象列表  
                var cookieDataList = JsonConvert.DeserializeObject<List<CookieData>>(jsonData);

                if (cookieDataList == null || cookieDataList.Count == 0)
                {
                    return;
                }

                // 将 Cookie 设置到 WebView2 中  
                int successCount = 0;
                foreach (var cookieData in cookieDataList)
                {
                    try
                    {
                        var webCookie = webViewB3B.CoreWebView2.CookieManager.CreateCookie(
                            cookieData.Name,
                            cookieData.Value,
                            cookieData.Domain,
                            cookieData.Path);

                        // 设置Cookie属性
                        if (!cookieData.IsSession && DateTime.TryParse(cookieData.Expires, out DateTime expires))
                        {
                            webCookie.Expires = expires;
                        }
                        webCookie.IsHttpOnly = cookieData.IsHttpOnly;
                        webCookie.IsSecure = cookieData.IsSecure;

                        // 添加Cookie到WebView2
                        webViewB3B.CoreWebView2.CookieManager.AddOrUpdateCookie(webCookie);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        // 单个Cookie加载失败不影响其他Cookie
                        System.Diagnostics.Debug.WriteLine($"加载Cookie失败: {cookieData.Name} - {ex.Message}");
                    }
                }

                if (successCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"成功加载了 {successCount} 个Cookie");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载会话时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCloseB3B_Click(object sender, EventArgs e)
        {
            // 关闭B3B的网站
            if (webViewB3B != null)
            {
                //并不是直接关闭WebView2控件，而是清除其网页的网址和内容，或者说导航到bland空白页
                webViewB3B.Source = new Uri("about:blank");
            }
        }

        private async void buttonShowDBData_Click(object sender, EventArgs e)
        {
            // 从数据库中获取全部的数据MuAi34Entity里面的全部行，并现在显示在DataGridView1控件中
            try
            {
                // 禁用按钮防止重复点击
                buttonShowDBData.Enabled = false;

                // 显示加载提示
                this.Text = "B3BForm - 正在加载数据...";

                // 创建数据库帮助类实例
                var databaseHelper = new DatabaseHelper();
                databaseHelper.SetPassword("Postgre,.1"); // 设置数据库密码

                // 测试数据库连接
                bool isConnected = await databaseHelper.TestConnectionAsync();
                if (!isConnected)
                {
                    MessageBox.Show("数据库连接失败！请检查网络连接和数据库配置。", "连接错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 获取所有数据
                var entities = await databaseHelper.GetAllAsync();

                if (entities == null || entities.Count == 0)
                {
                    MessageBox.Show("数据库中没有找到任何数据。", "无数据",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 清空DataGridView
                    dataGridView1.DataSource = null;
                    return;
                }

                // 将数据绑定到DataGridView
                dataGridView1.DataSource = entities;

                // 设置列标题（可选，美化显示）
                SetDataGridViewColumnHeaders();

                // 自动调整列宽
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }

                // 显示成功消息
                MessageBox.Show($"成功加载 {entities.Count} 条数据记录！", "加载成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 恢复标题
                this.Text = "B3BForm";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据时发生错误：\n{ex.Message}\n\n详细信息：\n{ex.StackTrace}",
                    "加载错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // 恢复标题
                this.Text = "B3BForm";
            }
            finally
            {
                // 恢复按钮状态
                buttonShowDBData.Enabled = true;
            }
        }

        /// <summary>
        /// 设置DataGridView的列标题，使其更易读
        /// </summary>
        private void SetDataGridViewColumnHeaders()
        {
            if (dataGridView1.Columns.Count == 0) return;

            // 设置中文列标题
            var columnMappings = new Dictionary<string, string>
            {
                { "Id", "ID" },
                { "Dep", "出发地" },
                { "Arr", "目的地" },
                { "Carrier", "承运人" },
                { "FlightNo", "航班号" },
                { "Cabins", "舱位" },
                { "ExtraOpenCabin", "额外开放舱位" },
                { "Price", "价格" },
                { "Rebate", "返点" },
                { "Retention", "留存" },
                { "StartFlightDate", "开始飞行日期" },
                { "EndFlightDate", "结束飞行日期" },
                { "AheadDays", "提前天数" },
                { "OrgWeekDays", "原始星期数据" },
                { "OrgExceptDateRanges", "原始例外日期范围" },
                { "OrgFlightNosLimit", "原始航班号限制" },
                { "CreateTime", "创建时间" }
            };

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                if (columnMappings.ContainsKey(column.Name))
                {
                    column.HeaderText = columnMappings[column.Name];
                }
            }

            // 设置价格列的格式（如果存在）
            if (dataGridView1.Columns.Contains("Price"))
            {
                dataGridView1.Columns["Price"].DefaultCellStyle.Format = "N2";
                dataGridView1.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // 设置返点和留存列的格式（如果存在）
            foreach (string columnName in new[] { "Rebate", "Retention" })
            {
                if (dataGridView1.Columns.Contains(columnName))
                {
                    dataGridView1.Columns[columnName].DefaultCellStyle.Format = "N2";
                    dataGridView1.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            // 设置日期列的格式
            foreach (string columnName in new[] { "StartFlightDate", "EndFlightDate" })
            {
                if (dataGridView1.Columns.Contains(columnName))
                {
                    dataGridView1.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }

            // 设置创建时间列的格式
            if (dataGridView1.Columns.Contains("CreateTime"))
            {
                dataGridView1.Columns["CreateTime"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
                dataGridView1.Columns["CreateTime"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private async void buttonPostPolicy_Click(object sender, EventArgs e)
        {
            //fetch("https://fuwu.cddyf.net/Handlers/DomesticProductHandler.ashx/AddDomesticAVCabin?domain=undefined&token=undefined", {
            //    "headers": {
            //        "accept": "application/json, text/javascript, */*; q=0.01",
            //        "accept-language": "en-US,en;q=0.9,zh-CN;q=0.8,zh-TW;q=0.7,zh;q=0.6",
            //        "content-type": "application/json; charset=UTF-8",
            //        "sec-ch-ua": "\"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"138\", \"Microsoft Edge\";v=\"138\"",
            //        "sec-ch-ua-mobile": "?0",
            //        "sec-ch-ua-platform": "\"Windows\"",
            //        "sec-fetch-dest": "empty",
            //        "sec-fetch-mode": "cors",
            //        "sec-fetch-site": "same-origin",
            //        "x-requested-with": "XMLHttpRequest",
            //        "cookie": "bbzwemp=%e3%88%80%e3%a8%80%e3%b0%80%e3%b0%80%e3%b4%80%e3%a8%80%e3%bc%80%e3%b8%80%e3%ac%80%e3%b0%80%e3%ac%80|%e7%ac%80%e6%a4%80%e3%90%80%e3%98%80%e3%a0%80%e3%a8%80%e3%b0%80%e3%b8%80; ASP.NET_SessionId=r0tiicvxxn0csihbbbcjo0vl; _bbzwInsurance=5CB970A0955B19F0D3016391A90421DB13E1966F8B40F5C19EC7AD546072D73EF2AA71ADCE1A01458F7E33322B725FF0D589EF2BE51886E00E824AE79E467AF0F13B21618E06BB9827CAC7C494A6D47E35DB53D5CDEC997DB18D623D5679A55897E6E6965DE92BFA7A9839E4A7F5EB0E6EF102D8",
            //        "Referer": "https://fuwu.cddyf.net/Product/Domestic/AddDomesticAVCabin.aspx?id=505711&returnUrl=%2FProduct%2FDomestic%2FDomesticAVCabinList.aspx%3Fpb%3Dtrue"
            //                    },
            //      "body": "{\"data\":{\"ProductType\":\"4\",\"Airline\":\"MU\",\"LimitedWeekdays\":[\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"0\"],\"FlightNoLimitType\":\"1\",\"FlightTimeLimitType\":\"0\",\"Fare\":\"499\",\"Details\":[{\"StartFlightDays\":null,\"EndFlightDays\":null,\"StartFlightDate\":\"2025-07-06\",\"EndFlightDate\":\"2025-07-07\",\"AheadDays\":\"1\",\"ExtraOpenCabin\":\"G\",\"Rebate\":\"2\",\"Retention\":\"0\",\"LowerPrice\":null,\"UpperPrice\":null,\"Cabins\":[\"K\",\"L\"]}],\"OfficeNo\":\"SHA009\",\"ProductCode\":\"MUAI_34\",\"AvCabinFlightType\":\"1\",\"AvCabinPriceType\":\"0\",\"EnableAdjustPrice\":true,\"RequireReserveSeat\":false,\"Departures\":[\"NKG\"],\"Arrivals\":[\"FOC\"],\"LimitedFlightNos\":[\"9781\"],\"LimitedFlightTimeRanges\":[]}}",
            //      "method": "POST"
            //    });

            try
            {
                // 检查WebView2是否已初始化
                if (webViewB3B.CoreWebView2 == null)
                {
                    MessageBox.Show("WebView2 未初始化，无法获取Cookie。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 检查是否有选中的数据行
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("请先选择要发布的价格政策数据行。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 禁用按钮防止重复点击
                buttonPostPolicy.Enabled = false;
                buttonPostPolicy.Text = "发布中...";

                // 1. 从webViewB3B控件中获取当前页面的Cookie
                string cookieString = await GetCookieStringAsync();
                if (string.IsNullOrEmpty(cookieString))
                {
                    MessageBox.Show("无法获取网站Cookie，请确保已登录B3B网站。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2. 读取当前选中的行数据，生成B3BPolicy对象
                var selectedEntity = GetSelectedRowData();
                if (selectedEntity == null)
                {
                    MessageBox.Show("无法读取选中的数据行。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 转换为B3BPolicy对象
                var policy = ConvertEntityToB3BPolicy(selectedEntity);

                // 3. 使用HttpClient发送POST请求到指定的URL
                string postUrl = "https://fuwu.cddyf.net/Handlers/DomesticProductHandler.ashx/AddDomesticAVCabin?domain=undefined&token=undefined";

                if (checkBoxUsePrice.Checked)
                {
                    int lowerPriceAdd = int.Parse(textBoxAddLowerPrice.Text);
                    int upperPriceAdd = int.Parse(textBoxUpperPriceAdd.Text);
                    policy.Details[0].LowerPrice = (int.Parse(policy.Fare) + lowerPriceAdd).ToString();
                    policy.Details[0].UpperPrice = (int.Parse(policy.Fare) + upperPriceAdd).ToString();
                    policy.Details[0].Cabins = new List<string>();
                    policy.AvCabinPriceType = "1"; // 设置为价格类型
                }

                string requestBody = policy.ToCompactJson();
                MessageBox.Show($"请求体内容:\n{requestBody}", "请求体内容", MessageBoxButtons.OK, MessageBoxIcon.Information);

                bool success = await SendPostRequestAsync(postUrl, requestBody, cookieString);

                if (success)
                {
                    MessageBox.Show("价格政策发布成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("价格政策发布失败，请检查网络连接和登录状态。", "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发布价格政策时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复按钮状态
                buttonPostPolicy.Enabled = true;
                buttonPostPolicy.Text = "发布政策";
            }
        }

        /// <summary>
        /// 从WebView2获取Cookie字符串
        /// </summary>
        private async Task<string> GetCookieStringAsync()
        {
            try
            {
                var cookies = await webViewB3B.CoreWebView2.CookieManager.GetCookiesAsync("https://fuwu.cddyf.net");
                if (cookies == null || cookies.Count == 0)
                {
                    return string.Empty;
                }

                var cookieStrings = new List<string>();
                foreach (var cookie in cookies)
                {
                    cookieStrings.Add($"{cookie.Name}={cookie.Value}");
                }

                return string.Join("; ", cookieStrings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取Cookie失败: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取选中行的数据
        /// </summary>
        private MuAi34Entity? GetSelectedRowData()
        {
            try
            {
                if (dataGridView1.SelectedRows.Count == 0)
                    return null;

                var selectedRow = dataGridView1.SelectedRows[0];
                var entity = selectedRow.DataBoundItem as MuAi34Entity;
                return entity;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取选中行数据失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 将MuAi34Entity转换为B3BPolicy对象
        /// </summary>
        private B3BPolicy ConvertEntityToB3BPolicy(MuAi34Entity entity)
        {
            var policy = new B3BPolicy();

            // 设置基本信息
            policy.Airline = entity.Carrier ?? "MU";
            policy.Fare = entity.Price?.ToString() ?? "0";

            // 设置出发地和目的地
            if (!string.IsNullOrEmpty(entity.Dep))
            {
                policy.Departures = new List<string> { entity.Dep };
            }
            if (!string.IsNullOrEmpty(entity.Arr))
            {
                policy.Arrivals = new List<string> { entity.Arr };
            }

            // 设置航班号
            if (!string.IsNullOrEmpty(entity.FlightNo))
            {
                policy.LimitedFlightNos = new List<string> { entity.FlightNo };
            }

            // 设置星期限制
            if (!string.IsNullOrEmpty(entity.OrgWeekDays))
            {
                policy.LimitedWeekdays = Util.ConvertWeekDaysToList(entity.OrgWeekDays);
            }

            // 设置Details
            var detail = new B3BPolicyDetail();
            detail.StartFlightDate = entity.StartFlightDate ?? DateTime.Now.ToString("yyyy-MM-dd");
            detail.EndFlightDate = entity.EndFlightDate ?? DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
            detail.AheadDays = entity.AheadDays?.ToString() ?? "1";
            detail.ExtraOpenCabin = entity.ExtraOpenCabin ?? "G";
            detail.Rebate = entity.Rebate?.ToString() ?? "2";
            detail.Retention = entity.Retention?.ToString() ?? "0";

            // 设置舱位
            if (!string.IsNullOrEmpty(entity.Cabins))
            {
                // 假设舱位用逗号或分号分隔
                var cabins = entity.Cabins.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(c => c.Trim())
                                         .Where(c => !string.IsNullOrEmpty(c))
                                         .ToList();
                if (cabins.Count > 0)
                {
                    detail.Cabins = cabins;
                }
            }

            policy.Details = new List<B3BPolicyDetail> { detail };

            return policy;
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        private async Task<bool> SendPostRequestAsync(string url, string requestBody, string cookieString)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // 设置请求头
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,zh-CN;q=0.8,zh-TW;q=0.7,zh;q=0.6");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"138\", \"Microsoft Edge\";v=\"138\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                    httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    httpClient.DefaultRequestHeaders.Add("Cookie", cookieString);
                    httpClient.DefaultRequestHeaders.Add("Referer", "https://fuwu.cddyf.net/Product/Domestic/AddDomesticAVCabin.aspx?id=505711&returnUrl=%2FProduct%2FDomestic%2FDomesticAVCabinList.aspx%3Fpb%3Dtrue");

                    // 创建请求内容
                    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    // 发送POST请求
                    var response = await httpClient.PostAsync(url, content);

                    // 检查响应状态
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"响应内容: {responseContent}");

                        // 你可以在这里解析响应内容来判断是否真正成功
                        // 例如检查是否包含成功标识等
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"请求失败: {response.StatusCode} - {response.ReasonPhrase}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"发送POST请求失败: {ex.Message}");
                return false;
            }
        }

        private async void buttonPostAllPolicy_Click(object sender, EventArgs e)
        {
            //把dataGridView1控件中的全部行数据都发布到B3B网站上
            //使用多线程去发，缩短时间

            try
            {
                // 检查WebView2是否已初始化
                if (webViewB3B.CoreWebView2 == null)
                {
                    MessageBox.Show("WebView2 未初始化，无法获取Cookie。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 检查是否有数据
                if (dataGridView1.Rows.Count == 0 || dataGridView1.DataSource == null)
                {
                    MessageBox.Show("没有数据可以发布。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 确认操作
                var result = MessageBox.Show(
                    $"确定要发布所有 {dataGridView1.Rows.Count} 条价格政策吗？\n\n" +
                    "此操作将使用多线程并行发布，请确保网络连接稳定。",
                    "批量发布确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                // 禁用按钮防止重复点击
                buttonPostAllPolicy.Enabled = false;
                buttonPostPolicy.Enabled = false;
                buttonPostAllPolicy.Text = "批量发布中...";

                // 获取Cookie
                string cookieString = await GetCookieStringAsync();
                if (string.IsNullOrEmpty(cookieString))
                {
                    MessageBox.Show("无法获取网站Cookie，请确保已登录B3B网站。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 获取所有数据行
                var allEntities = GetAllRowData();
                if (allEntities.Count == 0)
                {
                    MessageBox.Show("无法读取数据行。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 显示进度窗口
                var progressForm = new ProgressForm(allEntities.Count);
                // 进度窗口要有删除图标
                progressForm.Show();

                try
                {
                    // 使用多线程批量发布
                    var batchResult = await BatchPostPoliciesAsync(allEntities, cookieString, progressForm);

                    // 设置进度窗口为完成状态
                    progressForm.SetCompleted();

                    // 显示结果
                    string resultMessage = $"批量发布完成！\n\n" +
                                         $"总数据条数：{allEntities.Count}\n" +
                                         $"成功发布：{batchResult.SuccessCount}\n" +
                                         $"发布失败：{batchResult.FailureCount}\n" +
                                         $"处理时间：{batchResult.ElapsedTime.TotalSeconds:F1} 秒";

                    if (batchResult.FailureCount > 0)
                    {
                        resultMessage += $"\n\n失败的数据行：\n{string.Join(", ", batchResult.FailedIndices)}";
                    }

                    MessageBox.Show(resultMessage, "批量发布结果", MessageBoxButtons.OK,
                        batchResult.FailureCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                }
                finally
                {
                    // 确保进度窗口一定会关闭
                    if (progressForm != null && !progressForm.IsDisposed)
                    {
                        progressForm.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"批量发布时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复按钮状态
                buttonPostAllPolicy.Enabled = true;
                buttonPostPolicy.Enabled = true;
                buttonPostAllPolicy.Text = "批量发布";
            }
        }

        /// <summary>
        /// 获取所有行的数据
        /// </summary>
        private List<MuAi34Entity> GetAllRowData()
        {
            var entities = new List<MuAi34Entity>();
            try
            {
                if (dataGridView1.DataSource is List<MuAi34Entity> dataList)
                {
                    entities.AddRange(dataList);
                }
                else
                {
                    // 如果数据源不是List<MuAi34Entity>，则遍历行
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.DataBoundItem is MuAi34Entity entity)
                        {
                            entities.Add(entity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取所有行数据失败: {ex.Message}");
            }
            return entities;
        }

        /// <summary>
        /// 批量发布结果
        /// </summary>
        public class BatchPostResult
        {
            public int SuccessCount { get; set; }
            public int FailureCount { get; set; }
            public List<int> FailedIndices { get; set; } = new List<int>();
            public TimeSpan ElapsedTime { get; set; }
        }

        /// <summary>
        /// 批量发布价格政策（多线程）
        /// </summary>
        private async Task<BatchPostResult> BatchPostPoliciesAsync(List<MuAi34Entity> entities, string cookieString, ProgressForm progressForm)
        {
            var result = new BatchPostResult();
            var startTime = DateTime.Now;

            // 使用信号量控制并发数量，避免过多并发请求导致服务器拒绝
            int maxConcurrency = 20; // 最大并发数
            using (var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency))
            {
                var tasks = new List<Task>();
                var lockObject = new object();

                for (int i = 0; i < entities.Count; i++)
                {
                    int index = i; // 闭包捕获
                    var entity = entities[i];

                    var task = Task.Run(async () =>
                    {
                        await semaphore.WaitAsync(); // 等待信号量
                        try
                        {
                            // 转换为B3BPolicy对象
                            var policy = ConvertEntityToB3BPolicy(entity);
                            if (checkBoxUsePrice.Checked)
                            {
                                int lowerPriceAdd = int.Parse(textBoxAddLowerPrice.Text);
                                int upperPriceAdd = int.Parse(textBoxUpperPriceAdd.Text);
                                policy.Details[0].LowerPrice = (int.Parse(policy.Fare) + lowerPriceAdd).ToString();
                                policy.Details[0].UpperPrice = (int.Parse(policy.Fare) + upperPriceAdd).ToString();
                                policy.Details[0].Cabins = new List<string>();
                                policy.AvCabinPriceType = "1"; // 设置为价格类型
                            }

                            // 发送POST请求
                            string postUrl = "https://fuwu.cddyf.net/Handlers/DomesticProductHandler.ashx/AddDomesticAVCabin?domain=undefined&token=undefined";
                            string requestBody = policy.ToCompactJson();

                            bool success = await SendPostRequestAsync(postUrl, requestBody, cookieString);

                            // 线程安全地更新结果
                            lock (lockObject)
                            {
                                if (success)
                                {
                                    result.SuccessCount++;
                                }
                                else
                                {
                                    result.FailureCount++;
                                    result.FailedIndices.Add(index + 1); // 显示时从1开始计数
                                }

                                // 更新进度（在UI线程中）
                                progressForm.Invoke(new Action(() =>
                                {
                                    progressForm.UpdateProgress(result.SuccessCount + result.FailureCount,
                                        $"已处理：{result.SuccessCount + result.FailureCount}/{entities.Count}，" +
                                        $"成功：{result.SuccessCount}，失败：{result.FailureCount}");
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"处理第{index + 1}行数据失败: {ex.Message}");
                            lock (lockObject)
                            {
                                result.FailureCount++;
                                result.FailedIndices.Add(index + 1);

                                // 更新进度
                                progressForm.Invoke(new Action(() =>
                                {
                                    progressForm.UpdateProgress(result.SuccessCount + result.FailureCount,
                                        $"已处理：{result.SuccessCount + result.FailureCount}/{entities.Count}，" +
                                        $"成功：{result.SuccessCount}，失败：{result.FailureCount}");
                                }));
                            }
                        }
                        finally
                        {
                            semaphore.Release(); // 释放信号量
                        }
                    });

                    tasks.Add(task);
                }

                // 等待所有任务完成
                await Task.WhenAll(tasks);
            }

            result.ElapsedTime = DateTime.Now - startTime;
            return result;
        }

        private async void buttonDelAllPolicy_Click(object sender, EventArgs e)
        {
            // 在UI线程中执行删除操作
            try
            {
                // 禁用按钮防止重复点击
                buttonDelAllPolicy.Enabled = false;
                buttonDelAllPolicy.Text = "删除中...";
                checkBoxAutoDelete.Enabled = false;
                dateTimePickerDeleteTime.Enabled = false;

                // 执行删除操作
                bool success = await DeleteAllPoliciesAsync();

                if (success)
                {
                    MessageBox.Show("删除操作已完成！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除操作发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复按钮状态
                buttonDelAllPolicy.Enabled = true;
                buttonDelAllPolicy.Text = "删除B3B政策";
                checkBoxAutoDelete.Enabled = true;
                dateTimePickerDeleteTime.Enabled = true;
            }
        }

        /// <summary>
        /// 显示当前自动删除详细状态
        /// </summary>
        private void ShowAutoDeleteDetailedStatus()
        {
            string status = isAutoDeleteEnabled ? "已启用" : "已禁用";
            string timeStr = deleteTime.ToString(@"hh\:mm");
            string nextDeleteTime = isAutoDeleteEnabled ? $"下次删除时间：每天{timeStr}" : "无";

            DateTime now = DateTime.Now;
            string timeToNext = "";
            if (isAutoDeleteEnabled)
            {
                DateTime nextDelete = DateTime.Today.Add(deleteTime);
                if (nextDelete <= now)
                {
                    nextDelete = nextDelete.AddDays(1);
                }
                TimeSpan timeSpan = nextDelete - now;
                timeToNext = $"\n距离下次删除还有：{timeSpan.Days}天{timeSpan.Hours}小时{timeSpan.Minutes}分钟";
            }

            MessageBox.Show($"自动删除状态：{status}\n{nextDeleteTime}{timeToNext}",
                "定时任务状态", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 手动触发删除操作的方法（用于测试）
        /// </summary>
        private async void buttonManualDelete_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("确定要立即删除所有MUAI_34政策吗？", "手动删除确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                await DeleteAllPoliciesAsync();
            }
        }

        /// <summary>
        /// 定时上传全部政策的主方法
        /// </summary>
        public async Task<bool> PostAllPoliciesScheduledAsync()
        {
            try
            {
                // 检查WebView2是否已初始化
                if (webViewB3B.CoreWebView2 == null)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("WebView2 未初始化，无法获取Cookie。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                    return false;
                }

                // 检查是否有数据
                if (dataGridView1.Rows.Count == 0 || dataGridView1.DataSource == null)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("没有数据可以上传。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }));
                    return false;
                }

                // 获取Cookie
                string cookieString = await GetCookieStringAsync();
                if (string.IsNullOrEmpty(cookieString))
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("无法获取网站Cookie，请确保已登录B3B网站。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                    return false;
                }

                // 获取所有数据行
                var allEntities = GetAllRowData();
                if (allEntities.Count == 0)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("无法读取数据行。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                    return false;
                }

                // 创建进度窗口
                ProgressForm progressForm = null;
                this.Invoke(new Action(() =>
                {
                    progressForm = new ProgressForm(allEntities.Count);
                    progressForm.Show();
                }));

                bool success = false;
                try
                {
                    // 使用多线程批量上传
                    var batchResult = await BatchPostPoliciesAsync(allEntities, cookieString, progressForm);

                    // 设置进度窗口为完成状态
                    progressForm.Invoke(new Action(() =>
                    {
                        progressForm.SetCompleted();
                    }));

                    success = batchResult.FailureCount == 0;

                    // 在UI线程中显示结果
                    this.Invoke(new Action(() =>
                    {
                        string resultMessage = $"定时上传全部完成！\n\n" +
                                             $"总数据条数：{allEntities.Count}\n" +
                                             $"成功上传：{batchResult.SuccessCount}\n" +
                                             $"上传失败：{batchResult.FailureCount}\n" +
                                             $"处理时间：{batchResult.ElapsedTime.TotalSeconds:F1} 秒";

                        if (batchResult.FailureCount > 0)
                        {
                            resultMessage += $"\n\n失败的数据行：\n{string.Join(", ", batchResult.FailedIndices)}";
                        }

                        System.Diagnostics.Debug.WriteLine($"定时上传结果: {resultMessage}");

                        // 可选：显示详细结果消息框
                        // MessageBox.Show(resultMessage, "定时上传结果", MessageBoxButtons.OK,
                        //     batchResult.FailureCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    }));
                }
                finally
                {
                    // 确保进度窗口一定会关闭
                    if (progressForm != null)
                    {
                        progressForm.Invoke(new Action(() =>
                        {
                            if (!progressForm.IsDisposed)
                            {
                                progressForm.Close();
                            }
                        }));
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show($"定时上传全部时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return false;
            }
        }

        /// <summary>
        /// 删除所有政策的主方法
        /// </summary>
        public async Task<bool> DeleteAllPoliciesAsync()
        {
            try
            {
                // 检查WebView2是否已初始化
                if (webViewB3B.CoreWebView2 == null)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("WebView2 未初始化，无法获取Cookie。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                    return false;
                }

                // 获取Cookie
                string cookieString = await GetCookieStringAsync();
                if (string.IsNullOrEmpty(cookieString))
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("无法获取网站Cookie，请确保已登录B3B网站。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                    return false;
                }

                // 确认删除操作
                bool confirmed = false;
                this.Invoke(new Action(() =>
                {
                    var result = MessageBox.Show(
                        "确定要删除所有MUAI_34产品代码的政策吗？\n\n此操作不可撤销！",
                        "批量删除确认",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    confirmed = (result == DialogResult.Yes);
                }));

                if (!confirmed)
                {
                    return false;
                }

                // 1. 查询所有政策ID
                var policyInfo = await QueryAllPolicyIdsAsync(cookieString);
                if (policyInfo == null || policyInfo.TotalCount == 0)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("没有找到需要删除的政策。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                    return true;
                }

                // 2. 批量删除政策
                bool success = await BatchDeletePoliciesAsync(policyInfo.AllIds, cookieString);

                this.Invoke(new Action(() =>
                {
                    if (success)
                    {
                        MessageBox.Show($"成功删除了 {policyInfo.TotalCount} 个政策！", "删除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("删除政策时发生错误，请检查网络连接和登录状态。", "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));

                return success;
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show($"删除政策时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return false;
            }
        }

        /// <summary>
        /// 政策查询结果信息
        /// </summary>
        public class PolicyQueryInfo
        {
            public int TotalCount { get; set; }
            public int MaxId { get; set; }
            public int MinId { get; set; }
            public List<string> AllIds { get; set; } = new List<string>();
        }

        /// <summary>
        /// 查询所有政策ID
        /// </summary>
        private async Task<PolicyQueryInfo?> QueryAllPolicyIdsAsync(string cookieString)
        {
            try
            {
                string queryUrl = "https://fuwu.cddyf.net/Handlers/DomesticProductHandler.ashx/QueryDomesticAVCabins?domain=undefined&token=undefined";
                string requestBody = "{\"condition\":{\"Airline\":\"\",\"Departure\":\"\",\"Arrival\":\"\",\"ProductType\":null,\"ProductCode\":\"MUAI_34\",\"Disabled\":\"False\"},\"pagination\":{\"PageIndex\":1,\"PageSize\":100}}";

                using (var httpClient = new HttpClient())
                {
                    // 设置请求头
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,zh-CN;q=0.8,zh-TW;q=0.7,zh;q=0.6");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"138\", \"Microsoft Edge\";v=\"138\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                    httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    httpClient.DefaultRequestHeaders.Add("Cookie", cookieString);
                    httpClient.DefaultRequestHeaders.Add("Referer", "https://fuwu.cddyf.net/Product/Domestic/DomesticAVCabinList.aspx");

                    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(queryUrl, content);
                    richTextBoxLog.AppendText($"查询政策ID请求: {queryUrl}\n请求体: {requestBody}\n");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"查询响应: {responseContent}");
                        //把返回的内容显示在log控件
                        richTextBoxLog.AppendText($"查询政策ID响应状态: {responseContent}\n");

                        // responseContent={"Record":[{"Operator":{"Team":null,"Role":{"Code":"Merchant","Value":"2","Des":"商户"},"Username":"18988486220"},"Id":508710,"ProductType":{"Code":"Customer","Value":"4","Des":"切位"},"Airline":"FM","Departures":["XMN"],"Arrivals":["SHA"],"FlightNoLimitType":{"Code":"Include","Value":"1","Des":"包含"},"LimitedFlightNos":"9260","FlightTimeLimitType":{"Code":"None","Value":"0","Des":"无"},"LimitedFlightTimeRanges":"","LimitedWeekdays":"1/2/3/4/6","Fare":"649","EnableAdjustPrice":true,"RequireReserveSeat":false,"AvCabinFlightType":{"Code":"FlightDate","Value":"1","Des":"FlightDate"},"AvCabinPriceType":{"Code":"Price","Value":"1","Des":"Price"},"Disabled":false,"Details":[{"FlightDays":{"Start":null,"End":null},"FlightDate":{"Start":"2025-07-08","End":"2025-07-09"},"AheadDays":1,"Cabins":[],"LowerPrice":"700","UpperPrice":"1200","ExtraOpenCabin":"G","Rebate":"3","Retention":"-100"}],"OfficeNo":"SHA009","ProductTypeCodes":"","ProductCode":"MUAI_34","UpdateTime":"2025-07-05 12:15:19"}],"Pagination": {"PageSize": 10,"PageIndex": 1,"RowCount": 286,"PageCount": 29,"GetRowCount": true}}
                        // 解析响应数据
                        var queryResult = JsonConvert.DeserializeObject<PolicyQueryResponse>(responseContent);
                        if (queryResult?.Pagination != null && queryResult.Pagination.RowCount > 0)
                        {
                            var policyInfo = new PolicyQueryInfo
                            {
                                TotalCount = queryResult.Pagination.RowCount
                            };

                            // 如果有记录，从第一个记录获取最大ID
                            if (queryResult.Record != null && queryResult.Record.Count > 0)
                            {
                                policyInfo.MaxId = queryResult.Record[0].Id;
                                policyInfo.MinId = policyInfo.MaxId - policyInfo.TotalCount + 1;

                                // 生成所有ID列表
                                for (int id = policyInfo.MinId; id <= policyInfo.MaxId; id++)
                                {
                                    policyInfo.AllIds.Add(id.ToString());
                                }
                            }

                            return policyInfo;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"查询失败: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"查询政策ID失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 批量删除政策
        /// </summary>
        private async Task<bool> BatchDeletePoliciesAsync(List<string> ids, string cookieString)
        {
            try
            {
                if (ids == null || ids.Count == 0)
                {
                    return true;
                }

                string deleteUrl = "https://fuwu.cddyf.net/Handlers/DomesticProductHandler.ashx/BatchDeleteDomesticAVCabins?domain=undefined&token=undefined";

                // 创建删除请求体
                var deleteRequest = new { ids = ids };
                string requestBody = JsonConvert.SerializeObject(deleteRequest);

                using (var httpClient = new HttpClient())
                {
                    // 设置请求头
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,zh-CN;q=0.8,zh-TW;q=0.7,zh;q=0.6");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"138\", \"Microsoft Edge\";v=\"138\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                    httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    httpClient.DefaultRequestHeaders.Add("Cookie", cookieString);
                    httpClient.DefaultRequestHeaders.Add("Referer", "https://fuwu.cddyf.net/Product/Domestic/DomesticAVCabinList.aspx");

                    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    System.Diagnostics.Debug.WriteLine($"批量删除请求: {deleteUrl}\n请求体: {requestBody}");
                    // 先测试到这一步，不真的去请求删除，测试获取ids是否成功
                    //return true;

                    //发送POST请求
                    var response = await httpClient.PostAsync(deleteUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"删除响应: {responseContent}");
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"删除失败: {response.StatusCode} - {response.ReasonPhrase}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"批量删除政策失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 政策查询响应数据结构
        /// {"Record":[{"Operator":{"Team":null,"Role":{"Code":"Merchant","Value":"2","Des":"商户"},"Username":"18988486220"},"Id":508710,"ProductType":{"Code":"Customer","Value":"4","Des":"切位"},"Airline":"FM","Departures":["XMN"],"Arrivals":["SHA"],"FlightNoLimitType":{"Code":"Include","Value":"1","Des":"包含"},"LimitedFlightNos":"9260","FlightTimeLimitType":{"Code":"None","Value":"0","Des":"无"},"LimitedFlightTimeRanges":"","LimitedWeekdays":"1/2/3/4/6","Fare":"649","EnableAdjustPrice":true,"RequireReserveSeat":false,"AvCabinFlightType":{"Code":"FlightDate","Value":"1","Des":"FlightDate"},"AvCabinPriceType":{"Code":"Price","Value":"1","Des":"Price"},"Disabled":false,"Details":[{"FlightDays":{"Start":null,"End":null},"FlightDate":{"Start":"2025-07-08","End":"2025-07-09"},"AheadDays":1,"Cabins":[],"LowerPrice":"700","UpperPrice":"1200","ExtraOpenCabin":"G","Rebate":"3","Retention":"-100"}],"OfficeNo":"SHA009","ProductTypeCodes":"","ProductCode":"MUAI_34","UpdateTime":"2025-07-05 12:15:19"}],"Pagination": {"PageSize": 10,"PageIndex": 1,"RowCount": 286,"PageCount": 29,"GetRowCount": true}}
        /// </summary>
        public class PolicyQueryResponse
        {
            public List<PolicyRecord> Record { get; set; } = null!;
            public PolicyPagination Pagination { get; set; } = null!;
        }

        public class PolicyRecord
        {
            public int Id { get; set; }
        }

        public class PolicyPagination
        {
            public int PageSize { get; set; }
            public int PageIndex { get; set; }
            public int RowCount { get; set; }
            public int PageCount { get; set; }
            public bool GetRowCount { get; set; }
        }

        /// <summary>
        /// 表单检查结果数据结构
        /// </summary>
        public class FormCheckResult
        {
            [JsonProperty("userNameInput")]
            public bool UserNameInput { get; set; }

            [JsonProperty("passwordInput")]
            public bool PasswordInput { get; set; }

            [JsonProperty("codeInput")]
            public bool CodeInput { get; set; }

            [JsonProperty("checkboxInput")]
            public bool CheckboxInput { get; set; }

            [JsonProperty("loginButton")]
            public bool LoginButton { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; } = string.Empty;

            [JsonProperty("title")]
            public string Title { get; set; } = string.Empty;

            [JsonProperty("readyState")]
            public string ReadyState { get; set; } = string.Empty;
        }

        /// <summary>
        /// 页面测试结果数据结构
        /// </summary>
        public class PageTestResult
        {
            [JsonProperty("url")]
            public string Url { get; set; } = string.Empty;

            [JsonProperty("title")]
            public string Title { get; set; } = string.Empty;

            [JsonProperty("userNameExists")]
            public bool UserNameExists { get; set; }

            [JsonProperty("passwordExists")]
            public bool PasswordExists { get; set; }

            [JsonProperty("codeExists")]
            public bool CodeExists { get; set; }

            [JsonProperty("checkboxExists")]
            public bool CheckboxExists { get; set; }

            [JsonProperty("loginBtnExists")]
            public bool LoginBtnExists { get; set; }

            [JsonProperty("readyState")]
            public string ReadyState { get; set; } = string.Empty;

            [JsonProperty("bodyExists")]
            public bool BodyExists { get; set; }

            [JsonProperty("formExists")]
            public bool FormExists { get; set; }

            [JsonProperty("allInputs")]
            public List<InputInfo>? AllInputs { get; set; }
        }

        /// <summary>
        /// 输入框信息数据结构
        /// </summary>
        public class InputInfo
        {
            [JsonProperty("id")]
            public string Id { get; set; } = string.Empty;

            [JsonProperty("name")]
            public string Name { get; set; } = string.Empty;

            [JsonProperty("type")]
            public string Type { get; set; } = string.Empty;
        }

        /// <summary>
        /// 填写结果数据结构
        /// </summary>
        public class FillResult
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("actualValue")]
            public string ActualValue { get; set; } = string.Empty;

            [JsonProperty("expectedValue")]
            public string ExpectedValue { get; set; } = string.Empty;

            [JsonProperty("elementType")]
            public string ElementType { get; set; } = string.Empty;

            [JsonProperty("elementName")]
            public string ElementName { get; set; } = string.Empty;

            [JsonProperty("error")]
            public string Error { get; set; } = string.Empty;

            [JsonProperty("message")]
            public string Message { get; set; } = string.Empty;
        }

        /// <summary>
        /// 复选框操作结果数据结构
        /// </summary>
        public class CheckboxResult
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("wasChecked")]
            public bool WasChecked { get; set; }

            [JsonProperty("isChecked")]
            public bool IsChecked { get; set; }

            [JsonProperty("action")]
            public string Action { get; set; } = string.Empty;

            [JsonProperty("error")]
            public string Error { get; set; } = string.Empty;

            [JsonProperty("message")]
            public string Message { get; set; } = string.Empty;
        }

        /// <summary>
        /// 自动填写验证结果
        /// </summary>
        public class AutoFillVerifyResult
        {
            public bool UsernameOk { get; set; }
            public bool PasswordOk { get; set; }
            public bool CheckboxOk { get; set; }
            public string UsernameValue { get; set; } = string.Empty;
            public string PasswordValue { get; set; } = string.Empty;
            public bool CheckboxChecked { get; set; }
        }

        /// <summary>
        /// 初始化自动登录检查定时器
        /// </summary>
        private void InitializeAutoLoginTimer()
        {
            loginCheckTimer = new System.Windows.Forms.Timer();
            loginCheckTimer.Interval = 2000; // 每2秒检查一次
            loginCheckTimer.Tick += LoginCheckTimer_Tick;
        }

        /// <summary>
        /// 登录检查定时器事件
        /// </summary>
        private async void LoginCheckTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (webViewB3B.CoreWebView2 == null)
                {
                    System.Diagnostics.Debug.WriteLine("WebView2 未初始化");
                    return;
                }

                if (!isAutoLoginEnabled)
                {
                    System.Diagnostics.Debug.WriteLine("自动登录已禁用");
                    return;
                }

                if (!Config.Instance.EnableAutoLogin)
                {
                    System.Diagnostics.Debug.WriteLine("配置中自动登录已禁用");
                    return;
                }

                string currentUrl = webViewB3B.CoreWebView2.Source;
                System.Diagnostics.Debug.WriteLine($"🔍 当前URL: {currentUrl}");

                // 检查是否在登录页面 - 更精确的匹配
                if (currentUrl.Contains("Login.aspx") || currentUrl.Contains("oper.cddyf.net/Login"))
                {
                    System.Diagnostics.Debug.WriteLine("✅ 检测到登录页面，开始自动登录...");

                    // 暂停定时器，避免重复触发
                    loginCheckTimer.Stop();

                    // 等待页面完全加载
                    await Task.Delay(3000);

                    // 执行自动登录
                    await PerformAutoLoginAsync();
                }
                else if (currentUrl.Contains("fuwu.cddyf.net"))
                {
                    System.Diagnostics.Debug.WriteLine("📍 在主站页面，等待跳转到登录页面...");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"📍 在其他页面: {currentUrl}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"登录检查定时器发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行自动登录
        /// </summary>
        private async Task PerformAutoLoginAsync()
        {
            try
            {
                // 更新状态
                this.Invoke(new Action(() =>
                {
                    this.Text = "B3BForm - 正在自动登录...";
                }));

                System.Diagnostics.Debug.WriteLine("开始执行自动登录...");

                // 等待页面完全加载，最多重试5次
                bool formExists = false;
                for (int i = 0; i < 5; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"第{i + 1}次检查登录表单...");
                    formExists = await CheckLoginFormExistsAsync();
                    if (formExists)
                    {
                        break;
                    }
                    await Task.Delay(2000); // 等待2秒后重试
                }

                if (!formExists)
                {
                    System.Diagnostics.Debug.WriteLine("登录表单检查失败，所有重试都失败");
                    this.Invoke(new Action(() =>
                    {
                        this.Text = "B3BForm - 登录表单未找到";
                        MessageBox.Show("无法找到登录表单元素，请检查页面是否已完全加载。", "登录表单未找到",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }));
                    return;
                }

                System.Diagnostics.Debug.WriteLine("登录表单检查成功，开始填写...");

                // 填写用户名
                await FillInputFieldAsync("txtUserName", Config.Instance.LoginUsername);
                await Task.Delay(800);

                // 填写密码
                await FillInputFieldAsync("txtPassword", Config.Instance.LoginPassword);
                await Task.Delay(800);

                // 勾选记住密码
                await CheckRememberPasswordAsync();
                await Task.Delay(500);

                // 等待一下确保所有操作完成
                await Task.Delay(1000);

                // 验证填写结果
                var verifyResult = await VerifyAutoFillResultAsync();

                // 暂时跳过验证码识别，只完成基本填写
                this.Invoke(new Action(() =>
                {
                    this.Text = "B3BForm - 已填写用户名和密码，请手动输入验证码";

                    string statusMessage = "✅ 自动填写完成！\n\n已处理的项目:\n";
                    statusMessage += $"• 用户名: {(verifyResult.UsernameOk ? "✅" : "❌")} 18988486220\n";
                    statusMessage += $"• 密码: {(verifyResult.PasswordOk ? "✅" : "❌")} ********\n";
                    statusMessage += $"• 记住密码: {(verifyResult.CheckboxOk ? "✅" : "❌")} 已勾选\n\n";
                    statusMessage += "📝 下一步: 请手动输入验证码后点击登录按钮。";

                    MessageBox.Show(statusMessage, "自动填写状态", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));

                // 停止定时器，避免重复触发
                loginCheckTimer.Stop();

                System.Diagnostics.Debug.WriteLine("✅ 自动填写完成");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 自动登录异常: {ex.Message}");
                this.Invoke(new Action(() =>
                {
                    this.Text = "B3BForm - 自动登录失败";
                    MessageBox.Show($"自动登录过程中发生错误:\n{ex.Message}\n\n请尝试手动填写登录信息。", "自动登录错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        /// <summary>
        /// 检查登录表单是否存在
        /// </summary>
        private async Task<bool> CheckLoginFormExistsAsync()
        {
            try
            {
                string script = @"
                    (function() {
                        var result = {
                            userNameInput: !!document.getElementById('txtUserName'),
                            passwordInput: !!document.getElementById('txtPassword'),
                            codeInput: !!document.getElementById('txtCode'),
                            checkboxInput: !!document.getElementById('chkJizhu'),
                            loginButton: !!document.getElementById('btnLogon'),
                            url: window.location.href,
                            title: document.title,
                            readyState: document.readyState
                        };
                        return JSON.stringify(result);
                    })();
                ";

                var result = await webViewB3B.CoreWebView2.ExecuteScriptAsync(script);
                string jsonResult = result.Trim('"').Replace("\\\"", "\"");
                System.Diagnostics.Debug.WriteLine($"登录表单检查详情: {jsonResult}");

                // 解析JSON结果 - 修复类型转换问题
                var formCheck = JsonConvert.DeserializeObject<FormCheckResult>(jsonResult);
                bool allElementsExist = formCheck.UserNameInput && formCheck.PasswordInput && formCheck.CodeInput;

                System.Diagnostics.Debug.WriteLine($"表单元素检查结果: 用户名={formCheck.UserNameInput}, 密码={formCheck.PasswordInput}, 验证码={formCheck.CodeInput}, 复选框={formCheck.CheckboxInput}, 登录按钮={formCheck.LoginButton}");

                return allElementsExist;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查登录表单异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 填写输入框 - 针对WebView2优化
        /// </summary>
        private async Task FillInputFieldAsync(string fieldId, string value)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🖊️ 开始填写 {fieldId} = {value}");

                // 直接用webview的方法进行页面元素的查找和填写
                //webViewB3B.

                // 使用更兼容的JavaScript方法
                string script = $@"
                    (function() {{
                        try {{
                            var input = document.getElementById('{fieldId}');
                            if (!input) {{
                                return JSON.stringify({{
                                    success: false,
                                    error: 'element_not_found',
                                    message: '元素未找到: {fieldId}'
                                }});
                            }}
                            
                            // 清空现有值
                            input.value = '';
                            
                            // 聚焦到输入框
                            input.focus();
                            
                            // 设置新值
                            input.value = '{value}';
                            
                            // 触发多种事件以确保兼容性
                            var events = ['input', 'change', 'keyup', 'blur'];
                            events.forEach(function(eventType) {{
                                var event = new Event(eventType, {{ bubbles: true, cancelable: true }});
                                input.dispatchEvent(event);
                            }});
                            
                            // 验证值是否设置成功
                            var actualValue = input.value;
                            var success = actualValue === '{value}';
                            
                            return JSON.stringify({{
                                success: success,
                                actualValue: actualValue,
                                expectedValue: '{value}',
                                elementType: input.type,
                                elementName: input.name
                            }});
                        }} catch (error) {{
                            return JSON.stringify({{
                                success: false,
                                error: 'script_error',
                                message: error.message
                            }});
                        }}
                    }})();
                ";

                var result = await webViewB3B.CoreWebView2.ExecuteScriptAsync(script);
                string jsonResult = result.Trim('"').Replace("\\\"", "\"");
                System.Diagnostics.Debug.WriteLine($"📝 填写 {fieldId} 详细结果: {jsonResult}");

                var fillResult = JsonConvert.DeserializeObject<FillResult>(jsonResult);

                if (fillResult.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ 成功填写 {fieldId}: {fillResult.ActualValue}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ 填写 {fieldId} 失败: {fillResult.Error} - {fillResult.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 填写输入框 {fieldId} 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 勾选记住密码 - 针对WebView2优化
        /// </summary>
        private async Task CheckRememberPasswordAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("☑️ 开始勾选记住密码");

                string script = @"
                    (function() {
                        try {
                            var checkbox = document.getElementById('chkJizhu');
                            if (!checkbox) {
                                return JSON.stringify({
                                    success: false,
                                    error: 'element_not_found',
                                    message: '记住密码复选框未找到'
                                });
                            }
                            
                            var wasChecked = checkbox.checked;
                            
                            // 确保复选框被勾选
                            checkbox.checked = true;
                            
                            // 触发change事件
                            var changeEvent = new Event('change', { bubbles: true, cancelable: true });
                            checkbox.dispatchEvent(changeEvent);
                            
                            // 也触发click事件以确保兼容性
                            var clickEvent = new Event('click', { bubbles: true, cancelable: true });
                            checkbox.dispatchEvent(clickEvent);
                            
                            return JSON.stringify({
                                success: true,
                                wasChecked: wasChecked,
                                isChecked: checkbox.checked,
                                action: wasChecked ? 'already_checked' : 'checked'
                            });
                        } catch (error) {
                            return JSON.stringify({
                                success: false,
                                error: 'script_error',
                                message: error.message
                            });
                        }
                    })();
                ";

                var result = await webViewB3B.CoreWebView2.ExecuteScriptAsync(script);
                string jsonResult = result.Trim('"').Replace("\\\"", "\"");
                System.Diagnostics.Debug.WriteLine($"📋 勾选记住密码详细结果: {jsonResult}");

                var checkResult = JsonConvert.DeserializeObject<CheckboxResult>(jsonResult);

                if (checkResult.Success)
                {
                    if (checkResult.Action == "already_checked")
                    {
                        System.Diagnostics.Debug.WriteLine("✅ 记住密码已经勾选");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("✅ 成功勾选记住密码");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ 勾选记住密码失败: {checkResult.Error} - {checkResult.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 勾选记住密码异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取验证码图片并识别文字
        /// </summary>
        private async Task<string> GetCaptchaTextAsync()
        {
            try
            {
                // 获取验证码图片的Base64数据
                string captchaBase64 = await GetCaptchaImageBase64Async();
                if (string.IsNullOrEmpty(captchaBase64))
                {
                    return string.Empty;
                }

                // 使用简单的OCR识别（您可以替换为阿里云百炼API）
                string captchaText = await RecognizeCaptchaAsync(captchaBase64);

                System.Diagnostics.Debug.WriteLine($"验证码识别结果: {captchaText}");
                return captchaText;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取验证码失败: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取验证码图片的Base64数据
        /// </summary>
        private async Task<string> GetCaptchaImageBase64Async()
        {
            try
            {
                string script = @"
                    (function() {
                        var img = document.getElementById('imgValidateCode');
                        if (img && img.complete) {
                            var canvas = document.createElement('canvas');
                            var ctx = canvas.getContext('2d');
                            canvas.width = img.naturalWidth;
                            canvas.height = img.naturalHeight;
                            ctx.drawImage(img, 0, 0);
                            return canvas.toDataURL('image/png');
                        }
                        return '';
                    })();
                ";

                var result = await webViewB3B.CoreWebView2.ExecuteScriptAsync(script);
                string base64Data = result.Trim('"');

                if (base64Data.StartsWith("data:image/png;base64,"))
                {
                    return base64Data.Substring("data:image/png;base64,".Length);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取验证码图片失败: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 识别验证码（简单实现，您可以替换为阿里云百炼API）
        /// </summary>
        private async Task<string> RecognizeCaptchaAsync(string base64Image)
        {
            try
            {
                // 这里是简单的本地OCR实现
                // 您可以替换为调用阿里云百炼API

                // 将Base64转换为图像
                byte[] imageBytes = Convert.FromBase64String(base64Image);

                // 保存临时图片文件用于调试
                string tempPath = Path.Combine(Path.GetTempPath(), "captcha_temp.png");
                await File.WriteAllBytesAsync(tempPath, imageBytes);

                // 这里返回一个示例结果，您需要替换为实际的OCR调用
                // 可以调用阿里云百炼API或其他OCR服务
                return await CallAliCloudOCRAsync(base64Image);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"识别验证码失败: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 调用阿里云百炼OCR API（示例实现）
        /// </summary>
        private async Task<string> CallAliCloudOCRAsync(string base64Image)
        {
            try
            {
                // 检查是否启用阿里云OCR
                if (!Config.Instance.UseAliCloudOCR || Config.Instance.AliCloudApiKey == "YOUR_API_KEY")
                {
                    // 如果未配置API Key或禁用了阿里云OCR，使用备选方案
                    return await SimpleNumberRecognitionAsync(base64Image);
                }

                using (var httpClient = new HttpClient())
                {
                    // 设置请求头
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Config.Instance.AliCloudApiKey}");
                    httpClient.DefaultRequestHeaders.Add("X-DashScope-Async", "enable");

                    // 构建请求体
                    var requestBody = new
                    {
                        model = Config.Instance.AliCloudModel,
                        input = new
                        {
                            messages = new[]
                            {
                                new
                                {
                                    role = "user",
                                    content = new object[]
                                    {
                                        new
                                        {
                                            image = $"data:image/png;base64,{base64Image}"
                                        },
                                        new
                                        {
                                            text = "请识别这个验证码图片中的数字或字母，只返回识别出的字符，不要包含任何其他内容。"
                                        }
                                    }
                                }
                            }
                        },
                        parameters = new
                        {
                            result_format = "message"
                        }
                    };

                    string jsonBody = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(Config.Instance.AliCloudApiUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                        // 解析返回结果
                        if (result?.output?.choices != null && result.output.choices.Count > 0)
                        {
                            string recognizedText = result.output.choices[0].message.content.ToString();
                            // 清理识别结果，只保留数字和字母
                            string cleanText = Regex.Replace(recognizedText, @"[^\w]", "");
                            return cleanText;
                        }
                    }
                }

                // 如果API调用失败，使用备选方案
                return await SimpleNumberRecognitionAsync(base64Image);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"调用阿里云OCR失败: {ex.Message}");
                // 如果API调用失败，使用备选方案
                return await SimpleNumberRecognitionAsync(base64Image);
            }
        }

        /// <summary>
        /// 简单的数字识别（备选方案）
        /// </summary>
        private async Task<string> SimpleNumberRecognitionAsync(string base64Image)
        {
            await Task.Delay(100); // 模拟处理时间

            try
            {
                // 保存图片到临时文件供用户查看
                byte[] imageBytes = Convert.FromBase64String(base64Image);
                string tempPath = Path.Combine(Path.GetTempPath(), "captcha_for_manual_input.png");
                await File.WriteAllBytesAsync(tempPath, imageBytes);

                // 在UI线程中显示对话框让用户手动输入
                string userInput = string.Empty;
                this.Invoke(new Action(() =>
                {
                    var inputForm = new CaptchaInputForm(tempPath);
                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        userInput = inputForm.CaptchaText;
                    }
                }));

                return userInput;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"简单识别失败: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 点击登录按钮
        /// </summary>
        private async Task ClickLoginButtonAsync()
        {
            try
            {
                string script = @"
                    (function() {
                        var loginBtn = document.getElementById('btnLogon');
                        if (loginBtn) {
                            loginBtn.click();
                            return true;
                        }
                        return false;
                    })();
                ";

                var result = await webViewB3B.CoreWebView2.ExecuteScriptAsync(script);
                bool success = result.Trim('"').ToLower() == "true";
                System.Diagnostics.Debug.WriteLine($"点击登录按钮: {(success ? "成功" : "失败")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"点击登录按钮失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 启用自动登录
        /// </summary>
        public void EnableAutoLogin()
        {
            isAutoLoginEnabled = true;
            if (loginCheckTimer != null)
            {
                loginCheckTimer.Start();
                System.Diagnostics.Debug.WriteLine($"自动登录定时器已启动，间隔: {loginCheckTimer.Interval}ms");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("警告：自动登录定时器为空");
            }
        }

        /// <summary>
        /// 禁用自动登录
        /// </summary>
        public void DisableAutoLogin()
        {
            isAutoLoginEnabled = false;
            loginCheckTimer?.Stop();
        }

        /// <summary>
        /// 手动触发自动登录（用于测试）
        /// </summary>
        public async Task TriggerAutoLoginAsync()
        {
            System.Diagnostics.Debug.WriteLine("手动触发自动登录");
            if (webViewB3B.CoreWebView2 != null)
            {
                string currentUrl = webViewB3B.CoreWebView2.Source;
                System.Diagnostics.Debug.WriteLine($"当前URL: {currentUrl}");
                await PerformAutoLoginAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("WebView2 未初始化，无法触发自动登录");
            }
        }

        /// <summary>
        /// 添加一个测试方法来检查页面元素
        /// </summary>
        public async Task TestPageElementsAsync()
        {
            try
            {
                if (webViewB3B.CoreWebView2 == null)
                {
                    System.Diagnostics.Debug.WriteLine("WebView2 未初始化");
                    return;
                }

                string script = @"
                    (function() {
                        var result = {
                            url: window.location.href,
                            title: document.title,
                            userNameExists: !!document.getElementById('txtUserName'),
                            passwordExists: !!document.getElementById('txtPassword'),
                            codeExists: !!document.getElementById('txtCode'),
                            checkboxExists: !!document.getElementById('chkJizhu'),
                            loginBtnExists: !!document.getElementById('btnLogon'),
                            readyState: document.readyState,
                            bodyExists: !!document.body,
                            formExists: !!document.querySelector('form'),
                            allInputs: Array.from(document.querySelectorAll('input')).map(input => ({
                                id: input.id,
                                name: input.name,
                                type: input.type
                            }))
                        };
                        return JSON.stringify(result);
                    })();
                ";

                var result = await webViewB3B.CoreWebView2.ExecuteScriptAsync(script);
                string jsonResult = result.Trim('"').Replace("\\\"", "\"");
                System.Diagnostics.Debug.WriteLine($"页面元素检查结果: {jsonResult}");

                // 解析结果并格式化显示 - 修复类型转换问题
                var pageInfo = JsonConvert.DeserializeObject<PageTestResult>(jsonResult);
                string formattedResult = $"页面信息:\n" +
                    $"URL: {pageInfo.Url}\n" +
                    $"标题: {pageInfo.Title}\n" +
                    $"页面状态: {pageInfo.ReadyState}\n\n" +
                    $"登录元素检查:\n" +
                    $"✓ 用户名输入框: {(pageInfo.UserNameExists ? "存在" : "不存在")}\n" +
                    $"✓ 密码输入框: {(pageInfo.PasswordExists ? "存在" : "不存在")}\n" +
                    $"✓ 验证码输入框: {(pageInfo.CodeExists ? "存在" : "不存在")}\n" +
                    $"✓ 记住密码复选框: {(pageInfo.CheckboxExists ? "存在" : "不存在")}\n" +
                    $"✓ 登录按钮: {(pageInfo.LoginBtnExists ? "存在" : "不存在")}\n\n" +
                    $"页面中所有输入框数量: {pageInfo.AllInputs?.Count ?? 0}";

                // 显示结果给用户
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show(formattedResult, "页面元素检查结果",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查页面元素失败: {ex.Message}");
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show($"检查页面元素时发生错误:\n{ex.Message}", "检查失败",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        /// <summary>
        /// 验证自动填写结果
        /// </summary>
        private async Task<AutoFillVerifyResult> VerifyAutoFillResultAsync()
        {
            var result = new AutoFillVerifyResult();

            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 开始验证自动填写结果...");

                string script = @"
                    (function() {
                        try {
                            var username = document.getElementById('txtUserName');
                            var password = document.getElementById('txtPassword');
                            var checkbox = document.getElementById('chkJizhu');
                            
                            return JSON.stringify({
                                usernameValue: username ? username.value : '',
                                passwordValue: password ? password.value : '',
                                checkboxChecked: checkbox ? checkbox.checked : false,
                                usernameExists: !!username,
                                passwordExists: !!password,
                                checkboxExists: !!checkbox
                            });
                        } catch (error) {
                            return JSON.stringify({
                                error: error.message
                            });
                        }
                    })();
                ";

                var jsResult = await webViewB3B.CoreWebView2.ExecuteScriptAsync(script);
                string jsonResult = jsResult.Trim('"').Replace("\\\"", "\"");
                System.Diagnostics.Debug.WriteLine($"🔍 验证结果: {jsonResult}");

                var verifyData = JsonConvert.DeserializeObject<dynamic>(jsonResult);

                result.UsernameValue = verifyData.usernameValue?.ToString() ?? "";
                result.PasswordValue = verifyData.passwordValue?.ToString() ?? "";
                result.CheckboxChecked = verifyData.checkboxChecked ?? false;

                // 检查是否符合预期
                result.UsernameOk = result.UsernameValue == Config.Instance.LoginUsername;
                result.PasswordOk = result.PasswordValue == Config.Instance.LoginPassword;
                result.CheckboxOk = result.CheckboxChecked;

                System.Diagnostics.Debug.WriteLine($"✅ 验证完成: 用户名={result.UsernameOk}, 密码={result.PasswordOk}, 复选框={result.CheckboxOk}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 验证自动填写结果异常: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 手动测试自动登录功能（调试用）
        /// </summary>
        public async Task ManualTestAutoLoginAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🧪 开始手动测试自动登录功能...");

                if (webViewB3B.CoreWebView2 == null)
                {
                    MessageBox.Show("WebView2 未初始化，请先打开B3B网站。", "测试失败",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string currentUrl = webViewB3B.CoreWebView2.Source;
                System.Diagnostics.Debug.WriteLine($"🔍 当前测试URL: {currentUrl}");

                if (!currentUrl.Contains("Login.aspx") && !currentUrl.Contains("oper.cddyf.net/Login"))
                {
                    var result = MessageBox.Show($"当前页面不是登录页面:\n{currentUrl}\n\n是否继续测试？",
                        "页面确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                }

                // 执行测试
                await PerformAutoLoginAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 手动测试失败: {ex.Message}");
                MessageBox.Show($"手动测试自动登录时发生错误:\n{ex.Message}", "测试错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonAutoLogin_Click(object sender, EventArgs e)
        {
            // 手动测试自动登录，自动填写webview2中的页面的元素
            try
            {
                //直接操作webviewB3B中的页面的Dom，实现自动填写
                System.Diagnostics.Debug.WriteLine("手动触发自动登录按钮点击事件");
                if (webViewB3B.CoreWebView2 != null)
                {
                    // 停止定时器，避免重复触发
                    loginCheckTimer?.Stop();
                    // 执行自动登录
                    PerformAutoLoginAsync().ConfigureAwait(false);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WebView2 未初始化，无法触发自动登录");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"手动触发自动登录失败: {ex.Message}");
            }
        }

        private void checkBoxAutoPostAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoPostAll.Checked)
            {
                // 从DateTimePicker获取时间设置
                postAllTime = dateTimePickerPostAllTime.Value.TimeOfDay;
                EnableAutoPostAll();

                string timeStr = postAllTime.ToString(@"hh\:mm");
                MessageBox.Show($"定时上传全部已启用！\n每天 {timeStr} 自动上传所有MUAI_34政策。",
                    "定时任务", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                DisableAutoPostAll();
                MessageBox.Show("定时上传全部已禁用。", "定时任务", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 上传全部时间设置改变事件
        /// </summary>
        private void dateTimePickerPostAllTime_ValueChanged(object sender, EventArgs e)
        {
            // 更新上传全部时间
            postAllTime = dateTimePickerPostAllTime.Value.TimeOfDay;

            // 如果定时任务已启用，重新启动定时器并更新状态
            if (isAutoPostAllEnabled)
            {
                dailyPostAllTimer.Stop();
                dailyPostAllTimer.Start();
                UpdatePostAllStatusLabel();

                string timeStr = postAllTime.ToString(@"hh\:mm");
                this.Text = $"B3BForm - 定时上传全部时间已更新: {timeStr}";

                // 3秒后恢复原标题
                System.Windows.Forms.Timer titleTimer = new System.Windows.Forms.Timer();
                titleTimer.Interval = 3000;
                titleTimer.Tick += (s, args) =>
                {
                    this.Text = "B3BForm";
                    titleTimer.Stop();
                    titleTimer.Dispose();
                };
                titleTimer.Start();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //帮我在webviewMU打开网址：https://www.ceair.com/
            try
            {
                if (webViewMU.CoreWebView2 != null)
                {
                    string url = "https://www.ceair.com/";
                    webViewMU.CoreWebView2.Navigate(url);
                    System.Diagnostics.Debug.WriteLine($"正在打开网址: {url}");
                }
                else
                { 
                    System.Diagnostics.Debug.WriteLine("WebViewMU 未初始化，无法打开网址");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"打开网址失败: {ex.Message}");
            }

        }
    }
}

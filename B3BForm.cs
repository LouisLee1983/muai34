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

namespace WF_MUAI_34
{
    public partial class B3BForm : Form
    {
        private const string SESSION_PATH = "B3BSession";
        private const string SESSION_FILE = "B3BSession.json";
        private System.Windows.Forms.Timer dailyDeleteTimer = null!; // 每日删除定时器
        private bool isAutoDeleteEnabled = false; // 是否启用自动删除
        private TimeSpan deleteTime = new TimeSpan(23, 58, 0); // 默认删除时间 23:58

        public B3BForm()
        {
            InitializeComponent();
            InitializeAsync(); // 异步初始化 WebView2 控件
            InitializeDailyDeleteTimer(); // 初始化定时任务
            InitializeUIControls(); // 初始化UI控件
        }

        /// <summary>
        /// 初始化UI控件状态
        /// </summary>
        private void InitializeUIControls()
        {
            // 设置默认时间
            dateTimePickerDeleteTime.Value = DateTime.Today.Add(deleteTime);
            checkBoxAutoDelete.Checked = isAutoDeleteEnabled;
            UpdateStatusLabel();
            UpdateTimerButtonText();
        }

        /// <summary>
        /// 更新定时器按钮文本
        /// </summary>
        private void UpdateTimerButtonText()
        {
            if (isAutoDeleteEnabled)
            {
                buttonToggleTimer.Text = "关闭定时";
                buttonToggleTimer.BackColor = Color.LightCoral;
                buttonToggleTimer.ForeColor = Color.Black;
                string timeStr = deleteTime.ToString(@"hh\:mm");
                toolTip1.SetToolTip(buttonToggleTimer, $"点击关闭定时删除任务\n当前设置：每天{timeStr}执行");
            }
            else
            {
                buttonToggleTimer.Text = "启动定时";
                buttonToggleTimer.BackColor = Color.LightGreen;
                buttonToggleTimer.ForeColor = Color.Black;
                toolTip1.SetToolTip(buttonToggleTimer, "点击启动定时删除任务\n将按设置的时间自动删除所有MUAI_34政策");
            }
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
                    buttonToggleTimer.Enabled = false;
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
                    buttonToggleTimer.Enabled = true;
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
                buttonToggleTimer.Enabled = true;
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
        /// 启用自动删除定时任务
        /// </summary>
        public void EnableAutoDelete()
        {
            isAutoDeleteEnabled = true;
            dailyDeleteTimer.Start();
            UpdateStatusLabel();
            UpdateTimerButtonText();
            
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
            UpdateTimerButtonText();
            
            // 同步更新复选框状态（避免重复触发事件）
            checkBoxAutoDelete.CheckedChanged -= checkBoxAutoDelete_CheckedChanged;
            checkBoxAutoDelete.Checked = false;
            checkBoxAutoDelete.CheckedChanged += checkBoxAutoDelete_CheckedChanged;
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

        // 窗体关闭时释放定时器资源
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            dailyDeleteTimer?.Stop();
            dailyDeleteTimer?.Dispose();
            base.OnFormClosed(e);
        }

        /// <summary>
        /// 定时器控制按钮点击事件
        /// </summary>
        private void buttonToggleTimer_Click(object sender, EventArgs e)
        {
            if (isAutoDeleteEnabled)
            {
                // 当前已启用，点击后禁用
                DisableAutoDelete();
                MessageBox.Show("定时删除任务已关闭。", "定时任务", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // 当前未启用，点击后启用
                // 从DateTimePicker获取时间设置
                deleteTime = dateTimePickerDeleteTime.Value.TimeOfDay;
                EnableAutoDelete();
                
                string timeStr = deleteTime.ToString(@"hh\:mm");
                MessageBox.Show($"定时删除任务已启动！\n每天 {timeStr} 自动删除所有MUAI_34政策。", 
                    "定时任务", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
                    buttonToggleTimer.Enabled = false;
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
                    buttonToggleTimer.Enabled = true;
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

            // 导航到目标网站
            webViewB3B.Source = new Uri(url);
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
                buttonToggleTimer.Enabled = false;
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
                buttonToggleTimer.Enabled = true;
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

                        // 解析响应数据
                        var queryResult = JsonConvert.DeserializeObject<PolicyQueryResponse>(responseContent);
                        if (queryResult?.Data?.Pagination != null && queryResult.Data.Pagination.RowCount > 0)
                        {
                            var policyInfo = new PolicyQueryInfo
                            {
                                TotalCount = queryResult.Data.Pagination.RowCount
                            };

                            // 如果有记录，从第一个记录获取最大ID
                            if (queryResult.Data.Record != null && queryResult.Data.Record.Count > 0)
                            {
                                policyInfo.MaxId = queryResult.Data.Record[0].Id;
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
        /// </summary>
        public class PolicyQueryResponse
        {
            public PolicyQueryData Data { get; set; } = null!;
        }

        public class PolicyQueryData
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
    }
}

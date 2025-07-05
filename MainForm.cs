using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WF_MUAI_34
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 保存价格政策到数据库
        /// </summary>
        private async Task SavePolicyToDatabase(B3BPolicy policy)
        {
            try
            {
                var _databaseHelper = new DatabaseHelper();
                // 设置数据库密码（请替换为实际密码）
                _databaseHelper.SetPassword("Postgre,.1"); // 请根据实际情况修改密码

                // 检查数据库连接
                if (_databaseHelper == null)
                {
                    MessageBox.Show("数据库连接未初始化！", "数据库错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 测试数据库连接
                bool isConnected = await _databaseHelper.TestConnectionAsync();
                if (!isConnected)
                {
                    MessageBox.Show("数据库连接失败！请检查网络连接和数据库配置。", "连接错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 从B3BPolicy转换为数据库实体
                var entity = DatabaseHelper.FromB3BPolicy(policy);

                // 插入到数据库
                int insertedId = await _databaseHelper.InsertAsync(entity);

                //if (insertedId > 0)
                //{
                //    MessageBox.Show($"价格政策保存成功！\n数据库ID: {insertedId}\n出发地: {entity.Dep}\n目的地: {entity.Arr}\n价格: {entity.Price}",
                //        "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
                //else
                //{
                //    MessageBox.Show("价格政策保存失败！插入记录返回ID为0。", "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存价格政策时发生错误：\n{ex.Message}\n\n详细信息：\n{ex.StackTrace}",
                    "保存错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //initDataGridViewMuOrgExcel();
        }

        private void initDataGridViewMuOrgExcel()
        {
            // 定义一下表头，固定在gridview的列头
            //序号 发布状态    状态 始发地 目的地 航程类型    舱位 价格  客收 B2T/ 黑屏价格    APP / B2C价格   OTA / NDC价格   折扣率 最小航距    最大航距 低价限制    TourCode 适用规则    补充代理费 航线性质
            //例外航线 去程周中周末  回程周中周末 去程例外日期  回程例外日期 EI选择    去程航班号限制 回程航班号限制 最早提前购票时间 最晚提前购票时间    库存共享标识（默认不共享）	强K标识（默认不强K）	客座率区间（最小 最大）
            //库存（B2C / OTA B2T）  航程图 适用航班时刻  适用销售时刻 限制机型    销售开始时间 销售结束时间  承运开始时间 承运结束时间  定向群组
            //dataGridViewMuOrgExcel.Columns.Add("序号", "序号");
            //dataGridViewMuOrgExcel.Columns.Add("发布状态", "发布状态");
            //dataGridViewMuOrgExcel.Columns.Add("状态", "状态");
            //dataGridViewMuOrgExcel.Columns.Add("始发地", "始发地");
            //dataGridViewMuOrgExcel.Columns.Add("目的地", "目的地");
            //dataGridViewMuOrgExcel.Columns.Add("航程类型", "航程类型");
            //dataGridViewMuOrgExcel.Columns.Add("舱位", "舱位");
            //dataGridViewMuOrgExcel.Columns.Add("价格", "价格");
            //dataGridViewMuOrgExcel.Columns.Add("客收", "客收");
            //dataGridViewMuOrgExcel.Columns.Add("B2T/黑屏价格", "B2T/黑屏价格");
            //dataGridViewMuOrgExcel.Columns.Add("APP/B2C价格", "APP/B2C价格");
            //dataGridViewMuOrgExcel.Columns.Add("OTA/NDC价格", "OTA/NDC价格");
            //dataGridViewMuOrgExcel.Columns.Add("折扣率", "折扣率");
            //dataGridViewMuOrgExcel.Columns.Add("最小航距", "最小航距");
            //dataGridViewMuOrgExcel.Columns.Add("最大航距", "最大航距");
            //dataGridViewMuOrgExcel.Columns.Add("低价限制", "低价限制");
            //dataGridViewMuOrgExcel.Columns.Add("TourCode", "TourCode");
            //dataGridViewMuOrgExcel.Columns.Add("适用规则", "适用规则");
            //dataGridViewMuOrgExcel.Columns.Add("补充代理费", "补充代理费");
            //dataGridViewMuOrgExcel.Columns.Add("航线性质", "航线性质");
            //dataGridViewMuOrgExcel.Columns.Add("例外航线", "例外航线");
            //dataGridViewMuOrgExcel.Columns.Add("去程周中周末", "去程周中周末");
            //dataGridViewMuOrgExcel.Columns.Add("回程周中周末", "回程周中周末");
            //dataGridViewMuOrgExcel.Columns.Add("去程例外日期", "去程例外日期");
            //dataGridViewMuOrgExcel.Columns.Add("回程例外日期", "回程例外日期");
            //dataGridViewMuOrgExcel.Columns.Add("EI选择", "EI选择");
            //dataGridViewMuOrgExcel.Columns.Add("去程航班号限制", "去程航班号限制");
            //dataGridViewMuOrgExcel.Columns.Add("回程航班号限制", "回程航班号限制");
            //dataGridViewMuOrgExcel.Columns.Add("最早提前购票时间", "最早提前购票时间");
            //dataGridViewMuOrgExcel.Columns.Add("最晚提前购票时间", "最晚提前购票时间");
            //dataGridViewMuOrgExcel.Columns.Add("库存共享标识", "库存共享标识");
            //dataGridViewMuOrgExcel.Columns.Add("强K标识", "强K标识");
            //dataGridViewMuOrgExcel.Columns.Add("客座率区间（最小 最大）", "客座率区间（最小 最大）");
            //dataGridViewMuOrgExcel.Columns.Add("库存（B2C/OTA B2T）", "库存（B2C/OTA B2T）");
            //dataGridViewMuOrgExcel.Columns.Add("航程图", "航程图");
            //dataGridViewMuOrgExcel.Columns.Add("适用航班时刻", "适用航班时刻");
            //dataGridViewMuOrgExcel.Columns.Add("适用销售时刻", "适用销售时刻");
            //dataGridViewMuOrgExcel.Columns.Add("限制机型", "限制机型");
            //dataGridViewMuOrgExcel.Columns.Add("销售开始时间", "销售开始时间");
            //dataGridViewMuOrgExcel.Columns.Add("销售结束时间", "销售结束时间");
            //dataGridViewMuOrgExcel.Columns.Add("承运开始时间", "承运开始时间");
            //dataGridViewMuOrgExcel.Columns.Add("承运结束时间", "承运结束时间");
            //dataGridViewMuOrgExcel.Columns.Add("定向群组", "定向群组");
            // 设置列宽
            //foreach (DataGridViewColumn column in dataGridViewMuOrgExcel.Columns)
            //{
            //    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // 自动填充
            //    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // 居中对齐
            //}
        }

        private void buttonImportMuExcel_Click(object sender, EventArgs e)
        {
            // 点击按钮-弹出文件选择窗-选择对应的excel文件之后，读取对应的数据行，然后在dataGridViewMuOrgExcel中显示出来
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx;*.xlsm",
                Title = "Select an Excel File"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    // 读取Excel文件并显示数据
                    var dataTable = ExcelHelper.ReadMUSHExcel(filePath);
                    dataGridViewMuOrgExcel.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading Excel file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // 一共42列，设置列宽和对齐方式
            for (int i = 0; i < 42; i++)
            {
                dataGridViewMuOrgExcel.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; // 自动填充
                dataGridViewMuOrgExcel.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // 居中对齐
            }
        }

        private void buttonOpenB3BForm_Click(object sender, EventArgs e)
        {
            // 点击按钮-打开B3BForm窗体
            B3BForm b3bForm = new B3BForm();
            b3bForm.Show(); // 显示B3BForm窗体
        }

        private async void buttonEditSelectedItem_Click(object sender, EventArgs e)
        {
            // 读取当前行的数据，组成一个目标对象，弹出一个提示，展示当前选中行的数据
            dataGridViewMuOrgExcel.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // 设置为全行选择
            if (dataGridViewMuOrgExcel.SelectedRows.Count > 0)
            {
                var selectedRowIndex = dataGridViewMuOrgExcel.SelectedRows[0].Index; // 获取选中行的索引
                B3BPolicy[]? b3BPolicys = convertFromDataGridRow(selectedRowIndex); // 将选中行转换为B3BPolicy对象

                if (b3BPolicys == null)
                {
                    MessageBox.Show("转换B3BPolicy对象失败，请检查选中行数据是否完整。", "转换错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // 将B3BPolicy对象转换为JSON字符串
                foreach (var b3BPolicy in b3BPolicys)
                {
                    // 这里可以根据需要处理多个B3BPolicy对象
                    // 例如，弹出一个对话框显示每个对象的JSON
                    MessageBox.Show(b3BPolicy.ToCompactJson(), "B3BPolicy JSON", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await SavePolicyToDatabase(b3BPolicy);
                }
            }
            else
            {
                MessageBox.Show("Please select a row to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private B3BPolicy[]? convertFromDataGridRow(int rowIndex)
        {
            List<B3BPolicy> b3BPolicys = new List<B3BPolicy>();
            if (rowIndex < 0 || rowIndex >= dataGridViewMuOrgExcel.Rows.Count)
            {
                return null;
            }
            DataGridViewRow selectedRow = dataGridViewMuOrgExcel.Rows[rowIndex];
            // 只需要提取航空公司，出发机场，到达机场，去程星期限制，去程除外日期，价格
            string excelFlightNosLimit = selectedRow.Cells[26].Value?.ToString() ?? string.Empty;
            string[] flightNosLimit = Util.ConvertFlightNumbers(excelFlightNosLimit);
            string excelDep = selectedRow.Cells[3].Value?.ToString() ?? string.Empty; // 始发地
            string dep = excelDep.Replace("机场-", "").Trim(); // 去除"机场-"前缀
            string excelArr = selectedRow.Cells[4].Value?.ToString() ?? string.Empty; // 目的地
            string arr = excelArr.Replace("机场-", "").Trim(); // 去除"机场-"前缀
            string excelForwardWeekDays = selectedRow.Cells[21].Value?.ToString() ?? string.Empty; // 去程周中周末
            string excelForwardExceptionDates = selectedRow.Cells[23].Value?.ToString() ?? string.Empty; // 去程例外日期
            string excelPrice = selectedRow.Cells[9].Value?.ToString() ?? string.Empty; // 价格
            string price = excelPrice.Replace("元", "").Trim(); // 去除"元"后缀
            //MessageBox.Show($"Selected Row Data:\n" +
            //    $"Dep: {dep}\n" +
            //    $"Arr: {arr}\n" +
            //    $"Flight Nos Limit: {string.Join(", ", flightNosLimit)}\n" +
            //    $"Forward Week Days: {excelForwardWeekDays}\n" +
            //    $"Forward Exception Dates: {excelForwardExceptionDates}\n" +
            //    $"Price: {price}\n", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
            // 计算从今日起第三天和第四天是否符合日期和星期限制，如果符合就赋值
            DateTime today = DateTime.Today;
            DateTime thirdDay = today.AddDays(3);
            DateTime fourthDay = today.AddDays(4);
            bool isThirdDayValid = Util.CheckFlightDateMatch(thirdDay, excelForwardWeekDays, excelForwardExceptionDates);
            bool isFourthDayValid = Util.CheckFlightDateMatch(fourthDay, excelForwardWeekDays, excelForwardExceptionDates);
            if (!isThirdDayValid && !isFourthDayValid)
            {
                return null;
            }

            // 根据flightNosLimit的数组元素，一个生成一个B3BPolicy对象，添加到b3BPolicys列表中
            for (int i = 0; i < flightNosLimit.Length; i++)
            {
                // 只录一个航班号就行了MU1234/FM1234其实是同一个航班
                B3BPolicy b3BPolicy = new B3BPolicy();
                b3BPolicy.orgFlightNosLimit = excelFlightNosLimit; // 原始航班号限制
                b3BPolicy.orgExceptDateRanges = excelForwardExceptionDates; // 原始例外日期范围
                b3BPolicy.orgWeekdaysLimit = excelForwardWeekDays; // 原始去程星期限制

                b3BPolicy.Airline = flightNosLimit[i].Substring(0, 2); // 航空公司
                string flightNo = flightNosLimit[i].Substring(2); // 第一个航班号去掉前两个航空公司代码，如MU1234-》1234
                b3BPolicy.Departures.Add(dep); // 始发地
                b3BPolicy.Arrivals.Add(arr); // 目的地
                                             // 设置去程星期限制,把excel的7转成0，组合成数组存放到
                b3BPolicy.LimitedWeekdays = Util.ConvertWeekDaysToList(excelForwardWeekDays);
                b3BPolicy.Fare = price;
                b3BPolicy.LimitedFlightNos.Add(flightNo);
                B3BPolicyDetail detail = new B3BPolicyDetail();
                if (isThirdDayValid)
                {
                    detail.StartFlightDate = thirdDay.ToString("yyyy-MM-dd");
                    if (isFourthDayValid)
                    {
                        detail.EndFlightDate = fourthDay.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        detail.EndFlightDate = thirdDay.ToString("yyyy-MM-dd");
                    }
                }
                else
                {
                    detail.StartFlightDate = fourthDay.ToString("yyyy-MM-dd");
                    detail.EndFlightDate = fourthDay.ToString("yyyy-MM-dd");
                }
                b3BPolicy.Details.Add(detail); // 添加到政策详情中

                b3BPolicys.Add(b3BPolicy); // 添加到列表中
            }
            return b3BPolicys.ToArray(); // 返回转换后的B3BPolicy数组
        }

        private async void buttonEditAll_Click(object sender, EventArgs e)
        {
            // 先清空数据表
            var _databaseHelper = new DatabaseHelper();
            _databaseHelper.SetPassword("Postgre,.1");
            
            // 快速清空数据表
            try
            {
                string clearResult = await _databaseHelper.SmartClearTableAsync("muai34");
                System.Diagnostics.Debug.WriteLine($"清理结果: {clearResult}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清空数据表失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 全部转换，从第一行到最后一行批量转换，数量太多了，启动多线程处理
            if (dataGridViewMuOrgExcel.Rows.Count == 0)
            {
                MessageBox.Show("没有数据可以处理！请先导入Excel文件。", "无数据", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 确认是否要批量处理
            var result = MessageBox.Show($"确定要批量处理 {dataGridViewMuOrgExcel.Rows.Count} 行数据吗？\n这可能需要一些时间。",
                "确认批量处理", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            // 禁用按钮防止重复点击
            buttonEditAll.Enabled = false;
            buttonEditSelectedItem.Enabled = false;
            buttonImportMuExcel.Enabled = false;

            try
            {
                int totalRows = dataGridViewMuOrgExcel.Rows.Count;
                int successCount = 0;
                int failCount = 0;
                List<string> errors = new List<string>();

                // 创建进度对话框或使用状态栏显示进度
                var progress = new Progress<int>(value =>
                {
                    // 更新状态栏或标题栏显示进度
                    this.Text = $"处理进度: {value}/{totalRows} ({(value * 100 / totalRows):F1}%)";
                });

                // 使用 SemaphoreSlim 控制并发数量，避免过多并发导致数据库压力过大
                using (var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2))
                {
                    var tasks = new List<Task>();

                    for (int i = 0; i < totalRows; i++)
                    {
                        int rowIndex = i; // 避免闭包问题

                        var task = ProcessRowAsync(rowIndex, semaphore, progress, successCount, failCount, errors);
                        tasks.Add(task);
                    }

                    // 等待所有任务完成
                    await Task.WhenAll(tasks);

                    // 统计结果
                    successCount = tasks.Count(t => t.IsCompletedSuccessfully && ((Task<bool>)t).Result);
                    failCount = totalRows - successCount;
                }

                // 显示处理结果
                string message = $"批量处理完成！\n" +
                    $"总计: {totalRows} 行\n" +
                    $"成功: {successCount} 行\n" +
                    $"失败: {failCount} 行";

                if (errors.Count > 0 && errors.Count <= 10)
                {
                    message += $"\n\n前 {errors.Count} 个错误:\n" + string.Join("\n", errors.Take(10));
                }
                else if (errors.Count > 10)
                {
                    message += $"\n\n前10个错误:\n" + string.Join("\n", errors.Take(10)) + "\n...还有更多错误";
                }

                MessageBox.Show(message, "批量处理结果", MessageBoxButtons.OK,
                    failCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"批量处理时发生严重错误：\n{ex.Message}", "批量处理错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复按钮状态
                buttonEditAll.Enabled = true;
                buttonEditSelectedItem.Enabled = true;
                buttonImportMuExcel.Enabled = true;

                // 恢复标题
                this.Text = "WF_MUAI_34";
            }
        }

        /// <summary>
        /// 异步处理单行数据
        /// </summary>
        private async Task<bool> ProcessRowAsync(int rowIndex, SemaphoreSlim semaphore, IProgress<int> progress,
            int successCount, int failCount, List<string> errors)
        {
            await semaphore.WaitAsync();

            try
            {
                // 转换行数据为B3BPolicy对象
                B3BPolicy[]? b3BPolicys = convertFromDataGridRow(rowIndex);

                if (b3BPolicys == null)
                {
                    lock (errors)
                    {
                        errors.Add($"第 {rowIndex + 1} 行: 数据转换失败，可能不符合日期或星期限制");
                    }
                    return false;
                }

                // 保存到数据库
                foreach (var b3BPolicy in b3BPolicys)
                {
                    await SavePolicyToDatabase(b3BPolicy);
                }                

                // 报告进度
                progress?.Report(rowIndex + 1);

                return true;
            }
            catch (Exception ex)
            {
                lock (errors)
                {
                    errors.Add($"第 {rowIndex + 1} 行: {ex.Message}");
                }
                return false;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async void buttonClearTable_Click(object sender, EventArgs e)
        {
            try
            {
                // 确认操作
                var result = MessageBox.Show(
                    "确定要清空所有旧数据吗？\n\n" +
                    "此操作将使用最快的清理方法，不可恢复！",
                    "清空数据确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                // 禁用按钮防止重复点击
                buttonClearTable.Enabled = false;
                buttonClearTable.Text = "清理中...";

                // 创建数据库帮助类实例
                var _databaseHelper = new DatabaseHelper();
                _databaseHelper.SetPassword("Postgre,.1");

                // 测试连接
                bool isConnected = await _databaseHelper.TestConnectionAsync();
                if (!isConnected)
                {
                    MessageBox.Show("数据库连接失败！请检查网络连接。", "连接错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 使用智能清理方法
                string clearResult = await _databaseHelper.SmartClearTableAsync("muai34");
                
                MessageBox.Show($"数据清理完成！\n\n{clearResult}", "清理成功", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清理数据时发生错误：{ex.Message}", "清理失败", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 恢复按钮状态
                buttonClearTable.Enabled = true;
                buttonClearTable.Text = "清楚旧数据表";
            }
        }
    }
}

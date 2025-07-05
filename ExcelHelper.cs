using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using OfficeOpenXml; // Ensure you have installed the EPPlus NuGet package  

namespace WF_MUAI_34
{
    public static class ExcelHelper
    {
        /// <summary>  
        /// 读取东航MUSH Excel文件并将其转换为DataTable。  
        /// 其中有一些行不是具体的价格规则数据，需要跳过这些行。标志就是第一列是序号（正整数）才是有效数据行。  
        /// </summary>  
        public static DataTable ReadMUSHExcel(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            var dataTable = new DataTable();
            #region 折叠这段代码  
            // Define the columns in the DataTable  
            dataTable.Columns.Add("序号", typeof(string));
            dataTable.Columns.Add("发布状态", typeof(string));
            dataTable.Columns.Add("状态", typeof(string));
            dataTable.Columns.Add("始发地", typeof(string));
            dataTable.Columns.Add("目的地", typeof(string));
            dataTable.Columns.Add("航程类型", typeof(string));
            dataTable.Columns.Add("舱位", typeof(string));
            dataTable.Columns.Add("价格", typeof(string));
            dataTable.Columns.Add("客收", typeof(string));
            dataTable.Columns.Add("B2T/黑屏价格", typeof(string));
            dataTable.Columns.Add("APP/B2C价格", typeof(string));
            dataTable.Columns.Add("OTA/NDC价格", typeof(string));
            dataTable.Columns.Add("折扣率", typeof(string));
            dataTable.Columns.Add("最小航距", typeof(string));
            dataTable.Columns.Add("最大航距", typeof(string));
            dataTable.Columns.Add("低价限制", typeof(string));
            dataTable.Columns.Add("TourCode", typeof(string));
            dataTable.Columns.Add("适用规则", typeof(string));
            dataTable.Columns.Add("补充代理费", typeof(string));
            dataTable.Columns.Add("航线性质", typeof(string));
            dataTable.Columns.Add("例外航线", typeof(string));
            dataTable.Columns.Add("去程周中周末", typeof(string));
            dataTable.Columns.Add("回程周中周末", typeof(string));
            dataTable.Columns.Add("去程例外日期", typeof(string));
            dataTable.Columns.Add("回程例外日期", typeof(string));
            dataTable.Columns.Add("EI选择", typeof(string));
            dataTable.Columns.Add("去程航班号限制", typeof(string));
            dataTable.Columns.Add("回程航班号限制", typeof(string));
            dataTable.Columns.Add("最早提前购票时间", typeof(string));
            dataTable.Columns.Add("最晚提前购票时间", typeof(string));
            dataTable.Columns.Add("库存共享标识", typeof(string));
            dataTable.Columns.Add("强K标识", typeof(string));
            dataTable.Columns.Add("客座率区间（最小 最大）", typeof(string));
            dataTable.Columns.Add("库存（B2C/OTA B2T）", typeof(string));
            dataTable.Columns.Add("航程图", typeof(string));
            dataTable.Columns.Add("适用航班时刻", typeof(string));
            dataTable.Columns.Add("适用销售时刻", typeof(string));
            dataTable.Columns.Add("限制机型", typeof(string));
            dataTable.Columns.Add("销售开始时间", typeof(string));
            dataTable.Columns.Add("销售结束时间", typeof(string));
            dataTable.Columns.Add("承运开始时间", typeof(string));
            dataTable.Columns.Add("承运结束时间", typeof(string));
            dataTable.Columns.Add("定向群组", typeof(string));
            #endregion

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet == null)
                    throw new Exception("No worksheet found in the Excel file.");

                // Add rows to DataTable  
                for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
                {
                    var dataRow = dataTable.NewRow();
                    // Check if the first cell in the row is a valid integer (indicating a data row)  
                    if (!int.TryParse(worksheet.Cells[row, 1].Text, out _))
                    {
                        // Skip this row if the first cell is not a valid integer  
                        continue;
                    }

                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        dataRow[col - 1] = worksheet.Cells[row, col].Text;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }
    }

    // Cookie数据类，用于序列化和反序列化
    public class CookieData
    {
        public required string Name { get; set; }
        public required string Value { get; set; }
        public required string Domain { get; set; }
        public required string Path { get; set; }
        public required string Expires { get; set; } // 使用字符串格式化日期
        public bool IsHttpOnly { get; set; }
        public bool IsSecure { get; set; }
        public bool IsSession { get; set; }
    }

    public class B3BPolicy
    {
        /// <summary>
        /// {"data":{"ProductType":"4","Airline":"MU","LimitedWeekdays":["1","2","3","4","5","6","0"],"FlightNoLimitType":"1","FlightTimeLimitType":"0",
        /// "Fare":"499",
        /// "Details":[{"StartFlightDays":null,"EndFlightDays":null,"StartFlightDate":"2025-07-06","EndFlightDate":"2025-07-07","AheadDays":"1","ExtraOpenCabin":"G","Rebate":"2","Retention":"0","LowerPrice":null,"UpperPrice":null,"Cabins":["K","L"]}],
        /// "OfficeNo":"SHA009","ProductCode":"MUAI_34","AvCabinFlightType":"1","AvCabinPriceType":"0","EnableAdjustPrice":true,"RequireReserveSeat":false,"Departures":["NKG"],
        /// "Arrivals":["FOC"],"LimitedFlightNos":["9781"],"LimitedFlightTimeRanges":[]}}
        /// 
        public string ProductType { get; set; } // 产品类型，默认是"4"
        public string Airline { get; set; } // 航空公司
        public List<string> LimitedWeekdays { get; set; } // 限制的星期几可用，默认是["1","2","3","4","5","6","0"]
        public string FlightNoLimitType { get; set; } // 航班号限制类型"1"表示"仅适用"，默认是"1"
        public string FlightTimeLimitType { get; set; } // 航班时间限制类型"0"表示"不限"，默认是"0"
        public string Fare { get; set; } // 价格
        public List<B3BPolicyDetail> Details { get; set; } // 价格政策详情
        public string OfficeNo { get; set; } // Eterm编号，默认是"SHA009"
        public string ProductCode { get; set; } // 产品代码，默认是"MUAI_34"
        public string AvCabinFlightType { get; set; } // 可用舱位航班类型，默认是"1"
        public string AvCabinPriceType { get; set; } // 可用舱位价格类型，默认是"0"
        public bool EnableAdjustPrice { get; set; } // 是否允许调整价格，默认是true
        public bool RequireReserveSeat { get; set; } // 是否需要预留座位，默认是false
        public List<string> Departures { get; set; } // 始发地列表，默认是["NKG"]
        public List<string> Arrivals { get; set; } // 目的地列表，默认是["FOC"]
        public List<string> LimitedFlightNos { get; set; } // 限制的航班号列表，默认是["9781"]
        public List<string> LimitedFlightTimeRanges { get; set; } // 限制的航班时间范围列表，默认是空数组

        public string orgWeekdaysLimit { get; set; } // 原星期限制，默认是"1234567"
        public string orgExceptDateRanges { get; set; } // 原除外日期段，默认是空字符串
        public string orgFlightNosLimit { get; set; } // 原航班号限制，默认是空字符串

        // 构造函数
        public B3BPolicy()
        {
            ProductType = "4";
            Airline = string.Empty; // 需要在使用时设置
            LimitedWeekdays = new List<string> { "1", "2", "3", "4", "5", "6", "0" };
            FlightNoLimitType = "1";
            FlightTimeLimitType = "0";
            Fare = string.Empty; // 需要在使用时设置
            Details = new List<B3BPolicyDetail>();
            OfficeNo = "SHA009";
            ProductCode = "MUAI_34";
            AvCabinFlightType = "1";
            AvCabinPriceType = "0";
            EnableAdjustPrice = true;
            RequireReserveSeat = false;
            Departures = new List<string>();
            Arrivals = new List<string>();
            LimitedFlightNos = new List<string>();
            LimitedFlightTimeRanges = new List<string>();

            orgWeekdaysLimit = "1234567"; // 默认星期限制
            orgExceptDateRanges = string.Empty; // 默认除外日期段为空
            orgFlightNosLimit = string.Empty; // 默认航班号限制为空

        }

        // 把 B3BPolicy 对象转换为 JSON 字符串
        public string ToJson()
        {
            try
            {
                // 配置JSON序列化设置
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include, // 包含null值
                    Formatting = Formatting.Indented, // 格式化输出，便于查看
                    DateFormatString = "yyyy-MM-dd" // 日期格式
                };

                // 创建要序列化的对象
                var dataObject = new
                {
                    data = new
                    {
                        ProductType = this.ProductType ?? "",
                        Airline = this.Airline ?? "",
                        LimitedWeekdays = this.LimitedWeekdays ?? new List<string>(),
                        FlightNoLimitType = this.FlightNoLimitType ?? "1",
                        FlightTimeLimitType = this.FlightTimeLimitType ?? "0",
                        Fare = this.Fare ?? "",
                        Details = this.Details ?? new List<B3BPolicyDetail>(),
                        OfficeNo = this.OfficeNo ?? "SHA009",
                        ProductCode = this.ProductCode ?? "MUAI_34",
                        AvCabinFlightType = this.AvCabinFlightType ?? "1",
                        AvCabinPriceType = this.AvCabinPriceType ?? "0",
                        EnableAdjustPrice = this.EnableAdjustPrice,
                        RequireReserveSeat = this.RequireReserveSeat,
                        Departures = this.Departures ?? new List<string>(),
                        Arrivals = this.Arrivals ?? new List<string>(),
                        LimitedFlightNos = this.LimitedFlightNos ?? new List<string>(),
                        LimitedFlightTimeRanges = this.LimitedFlightTimeRanges ?? new List<string>()
                    }
                };

                // 序列化为JSON字符串
                string jsonStr = JsonConvert.SerializeObject(dataObject, settings);
                return jsonStr;
            }
            catch (Exception ex)
            {
                // 如果序列化失败，返回错误信息
                return $"{{\"error\": \"JSON序列化失败: {ex.Message}\"}}";
            }
        }

        // 生成紧凑格式的JSON（无格式化）
        public string ToCompactJson()
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    Formatting = Formatting.None // 紧凑格式
                };

                var dataObject = new
                {
                    data = new
                    {
                        ProductType = this.ProductType ?? "",
                        Airline = this.Airline ?? "",
                        LimitedWeekdays = this.LimitedWeekdays ?? new List<string>(),
                        FlightNoLimitType = this.FlightNoLimitType ?? "1",
                        FlightTimeLimitType = this.FlightTimeLimitType ?? "0",
                        Fare = this.Fare ?? "",
                        Details = this.Details ?? new List<B3BPolicyDetail>(),
                        OfficeNo = this.OfficeNo ?? "SHA009",
                        ProductCode = this.ProductCode ?? "MUAI_34",
                        AvCabinFlightType = this.AvCabinFlightType ?? "1",
                        AvCabinPriceType = this.AvCabinPriceType ?? "0",
                        EnableAdjustPrice = this.EnableAdjustPrice,
                        RequireReserveSeat = this.RequireReserveSeat,
                        Departures = this.Departures ?? new List<string>(),
                        Arrivals = this.Arrivals ?? new List<string>(),
                        LimitedFlightNos = this.LimitedFlightNos ?? new List<string>(),
                        LimitedFlightTimeRanges = this.LimitedFlightTimeRanges ?? new List<string>()
                    }
                };

                return JsonConvert.SerializeObject(dataObject, settings);
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"JSON序列化失败: {ex.Message}\"}}";
            }
        }

        // 做一个转成cvs格式的字符串转换函数
        public string ToCsv()
        {
            // 使用StringBuilder来构建CSV字符串
            var csvBuilder = new System.Text.StringBuilder();
            // 添加表头,只要几个字段，起飞，到达，航空公司代码，航班号，舱位，额外开放舱位，价格，返点，留钱，开始日期，结束日期，提前天数，原星期限制，原除外日期段
            csvBuilder.AppendLine("");
            // 添加数据行
            csvBuilder.AppendLine($"{ProductType},{Airline},{string.Join(";", LimitedWeekdays)},{FlightNoLimitType},{FlightTimeLimitType},{Fare},{OfficeNo},{ProductCode},{AvCabinFlightType},{AvCabinPriceType},{EnableAdjustPrice},{RequireReserveSeat}");
            return csvBuilder.ToString();
        }

    }
    public class B3BPolicyDetail
    {
        /// "Details":[
        /// {"StartFlightDays":null,"EndFlightDays":null,"StartFlightDate":"2025-07-06","EndFlightDate":"2025-07-07","AheadDays":"1","ExtraOpenCabin":"G","Rebate":"2","Retention":"0",
        /// "LowerPrice":null,"UpperPrice":null,"Cabins":["K","L"]}
        /// ],
        /// 
        public string? StartFlightDays { get; set; } // 提前天数的最小数，可以为null
        public string? EndFlightDays { get; set; } // 提前天数的最大数，可以为null
        public string StartFlightDate { get; set; } // 航班开始日期
        public string EndFlightDate { get; set; } // 航班结束日期
        public string AheadDays { get; set; } // 最早提前购票时间，默认1
        public string ExtraOpenCabin { get; set; } // 额外开放的舱位，默认是"G"
        public string Rebate { get; set; } // 返点，默认是"2"
        public string Retention { get; set; } // 返现金金额，默认是"0"
        public string? LowerPrice { get; set; } // 最低价格，可以为null
        public string? UpperPrice { get; set; } // 最高价格，可以为null
        public List<string> Cabins { get; set; } // 舱位列表，默认是["K","L"]

        // 构造函数
        public B3BPolicyDetail()
        {
            StartFlightDays = null; // 可以为null
            EndFlightDays = null; // 可以为null
            StartFlightDate = string.Empty; // 需要在使用时设置
            EndFlightDate = string.Empty; // 需要在使用时设置
            AheadDays = "1"; // 默认1天
            ExtraOpenCabin = "G"; // 默认是"G"
            Rebate = "2"; // 默认是"2"
            Retention = "0"; // 默认是"0"
            LowerPrice = null; // 可以为null
            UpperPrice = null; // 可以为null
            Cabins = new List<string> { "V", "S", "R", "N", "L", "K" }; // 需要在使用时设置
        }
    }

    public static class Util
    {
        /// <summary>
        /// 这个类主要是负责转换原始的价格政策变成我们目标的格式，主要是转换日期段（排除日期和星期的匹配之后的结果），转换价格，转换航班号为单独的航班号
        /// 机场- NKG	机场- CAN	OW	G	649 CNY B2T/黑屏	0.517	649	649	649	41  %			1234567	2025-07-01~2025-08-17		MU9767/FM9767 适用 
        /// </summary>        
        public static string[] ConvertFlightNumbers(string flightNumbers)
        {
            // 传入的字符串："MU9767/FM9767 适用",识别处理之后返回["MU9767","FM9767"]
            if (string.IsNullOrEmpty(flightNumbers))
                return Array.Empty<string>();
            // 先把"适用"去掉，再用"/"分割出每个航班号
            flightNumbers = flightNumbers.Replace("适用", "").Trim();
            var flightNumberList = flightNumbers.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            // 返回结果
            return flightNumberList;
        }
        public static bool CheckFlightDateMatch(DateTime flightDate, string weekDays, string exceptDateRanges)
        {
            // 例如flightDate=2025-07-05, weekDays=1234567(表示周一到周日), exceptDateRanges=2025-06-20~2025-06-30;2025-08-18~2025-08-31（表示2025-06-20到2025-06-30和2025-08-18到2025-08-31都是无效的日期）            
            bool isMatch = false;
            // 对flightDate进行星期匹配，进行除外日期范围匹配，如果匹配就返回true，否则返回false

            // 1. 检查星期匹配
            if (!string.IsNullOrEmpty(weekDays))
            {
                // 获取航班日期的星期几 (1=周一, 2=周二, ..., 7=周日)
                int dayOfWeek = flightDate.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)flightDate.DayOfWeek;

                // 检查这个星期几是否在weekDays字符串中
                if (weekDays.Contains(dayOfWeek.ToString()))
                {
                    isMatch = true;
                }
            }

            // 2. 如果星期匹配，检查是否在例外日期范围内
            if (isMatch && !string.IsNullOrEmpty(exceptDateRanges))
            {
                // 按分号分割多个日期范围
                string[] dateRanges = exceptDateRanges.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string range in dateRanges)
                {
                    // 按~分割开始和结束日期
                    string[] dates = range.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

                    if (dates.Length == 2)
                    {
                        if (DateTime.TryParse(dates[0].Trim(), out DateTime startDate) &&
                            DateTime.TryParse(dates[1].Trim(), out DateTime endDate))
                        {
                            // 如果航班日期在例外范围内，则不匹配
                            if (flightDate.Date >= startDate.Date && flightDate.Date <= endDate.Date)
                            {
                                isMatch = false;
                                break;
                            }
                        }
                    }
                }
            }

            return isMatch; // 返回匹配结果
        }

        // 把excel的星期1234567变成字符串列表["1", "2", "3", "4", "5", "6", "0"]
        public static List<string> ConvertWeekDaysToList(string weekDays)
        {
            if (string.IsNullOrEmpty(weekDays))
                return new List<string>();
            // 将字符串转换为字符数组，然后转换为字符串列表，需要把7转换为0
            char[] days = weekDays.ToCharArray();
            List<string> weekDaysList = new List<string>();
            for (int i = 0; i < days.Length; i++)
            {
                // 如果是7，就转换为0
                weekDaysList.Add(days[i] == '7' ? "0" : days[i].ToString());
            }
            return weekDaysList;
        }
    }
}

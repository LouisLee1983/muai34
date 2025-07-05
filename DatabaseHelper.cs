using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WF_MUAI_34
{
    // 数据库实体类，对应 muai34 表
    public class MuAi34Entity
    {
        public int Id { get; set; }
        public string? Dep { get; set; } // 出发地
        public string? Arr { get; set; } // 目的地
        public string? Carrier { get; set; } // 承运人
        public string? FlightNo { get; set; } // 航班号
        public string? Cabins { get; set; } // 舱位
        public string? ExtraOpenCabin { get; set; } // 额外开放舱位
        public decimal? Price { get; set; } // 价格
        public decimal? Rebate { get; set; } // 返点
        public decimal? Retention { get; set; } // 留存
        public string? StartFlightDate { get; set; } // 开始飞行日期
        public string? EndFlightDate { get; set; } // 结束飞行日期
        public int? AheadDays { get; set; } // 提前天数
        public string? OrgWeekDays { get; set; } // 原始星期数据
        public string? OrgExceptDateRanges { get; set; } // 原始例外日期范围
        public string? OrgFlightNosLimit { get; set; } // 原始航班号限制
        public DateTime? CreateTime { get; set; } // 创建时间

        public MuAi34Entity()
        {
            CreateTime = DateTime.Now;
        }
    }

    // 数据库操作类
    public class DatabaseHelper
    {
        private string _connectionString;

        public DatabaseHelper(string host = "47.111.119.238", int port = 5432, 
            string database = "etermaidb", string username = "postgres", string password = "Postgre,.1")
        {
            _connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};";
        }

        /// <summary>
        /// 设置数据库密码（出于安全考虑，密码单独设置）
        /// </summary>
        public void SetPassword(string password)
        {
            var builder = new NpgsqlConnectionStringBuilder(_connectionString)
            {
                Password = password
            };
            _connectionString = builder.ConnectionString;
        }

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"数据库连接失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 插入单条记录
        /// </summary>
        public async Task<int> InsertAsync(MuAi34Entity entity)
        {
            const string sql = @"
                INSERT INTO muai34 (dep, arr, carrier, flightno, cabins, ""extraOpenCabin"", price, rebate, retention, 
                    ""StartFlightDate"", ""EndFlightDate"", ""AheadDays"", ""orgWeekDays"", ""orgExceptDateRanges"", ""orgFlightNosLimit"", ""createTime"")
                VALUES (@dep, @arr, @carrier, @flightno, @cabins, @extraOpenCabin, @price, @rebate, @retention,
                    @StartFlightDate, @EndFlightDate, @AheadDays, @orgWeekDays, @orgExceptDateRanges, @orgFlightNosLimit, @createTime)
                RETURNING id";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new NpgsqlCommand(sql, connection);
                AddParameters(command, entity);
                
                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"插入数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 批量插入记录
        /// </summary>
        public async Task<List<int>> InsertBatchAsync(List<MuAi34Entity> entities)
        {
            var insertedIds = new List<int>();
            
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                const string sql = @"
                    INSERT INTO muai34 (dep, arr, carrier, flightno, cabins, ""extraOpenCabin"", price, rebate, retention, 
                        ""StartFlightDate"", ""EndFlightDate"", ""AheadDays"", ""orgWeekDays"", ""orgExceptDateRanges"", ""orgFlightNosLimit"", ""createTime"")
                    VALUES (@dep, @arr, @carrier, @flightno, @cabins, @extraOpenCabin, @price, @rebate, @retention,
                        @StartFlightDate, @EndFlightDate, @AheadDays, @orgWeekDays, @orgExceptDateRanges, @orgFlightNosLimit, @createTime)
                    RETURNING id";

                foreach (var entity in entities)
                {
                    using var command = new NpgsqlCommand(sql, connection, transaction);
                    AddParameters(command, entity);
                    
                    var result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        insertedIds.Add(Convert.ToInt32(result));
                    }
                }

                await transaction.CommitAsync();
                return insertedIds;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"批量插入数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据ID查询记录
        /// </summary>
        public async Task<MuAi34Entity?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT id, dep, arr, carrier, flightno, cabins, ""extraOpenCabin"", price, rebate, retention,
                    ""StartFlightDate"", ""EndFlightDate"", ""AheadDays"", ""orgWeekDays"", ""orgExceptDateRanges"", ""orgFlightNosLimit"", ""createTime""
                FROM muai34 WHERE id = @id";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);
                
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return ReadEntity(reader);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"查询数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 查询所有记录
        /// </summary>
        public async Task<List<MuAi34Entity>> GetAllAsync()
        {
            const string sql = @"
                SELECT id, dep, arr, carrier, flightno, cabins, ""extraOpenCabin"", price, rebate, retention,
                    ""StartFlightDate"", ""EndFlightDate"", ""AheadDays"", ""orgWeekDays"", ""orgExceptDateRanges"", ""orgFlightNosLimit"", ""createTime""
                FROM muai34 ORDER BY id DESC";

            try
            {
                var entities = new List<MuAi34Entity>();
                
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new NpgsqlCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    entities.Add(ReadEntity(reader));
                }
                
                return entities;
            }
            catch (Exception ex)
            {
                throw new Exception($"查询所有数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 按条件查询记录
        /// </summary>
        public async Task<List<MuAi34Entity>> GetByConditionAsync(string dep = "", string arr = "", 
            string carrier = "", string flightno = "")
        {
            var conditions = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrEmpty(dep))
            {
                conditions.Add("dep = @dep");
                parameters.Add(new NpgsqlParameter("@dep", dep));
            }
            if (!string.IsNullOrEmpty(arr))
            {
                conditions.Add("arr = @arr");
                parameters.Add(new NpgsqlParameter("@arr", arr));
            }
            if (!string.IsNullOrEmpty(carrier))
            {
                conditions.Add("carrier = @carrier");
                parameters.Add(new NpgsqlParameter("@carrier", carrier));
            }
            if (!string.IsNullOrEmpty(flightno))
            {
                conditions.Add("flightno = @flightno");
                parameters.Add(new NpgsqlParameter("@flightno", flightno));
            }

            string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
            
            string sql = $@"
                SELECT id, dep, arr, carrier, flightno, cabins, ""extraOpenCabin"", price, rebate, retention,
                    ""StartFlightDate"", ""EndFlightDate"", ""AheadDays"", ""orgWeekDays"", ""orgExceptDateRanges"", ""orgFlightNosLimit"", ""createTime""
                FROM muai34 {whereClause} ORDER BY id DESC";

            try
            {
                var entities = new List<MuAi34Entity>();
                
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddRange(parameters.ToArray());
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    entities.Add(ReadEntity(reader));
                }
                
                return entities;
            }
            catch (Exception ex)
            {
                throw new Exception($"按条件查询数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM muai34 WHERE id = @id";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);
                
                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"删除数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从B3BPolicy转换为MuAi34Entity
        /// </summary>
        public static MuAi34Entity FromB3BPolicy(B3BPolicy policy)
        {
            var entity = new MuAi34Entity
            {
                Dep = policy.Departures?.Count > 0 ? policy.Departures[0] : null,
                Arr = policy.Arrivals?.Count > 0 ? policy.Arrivals[0] : null,
                Carrier = policy.Airline,
                FlightNo = policy.LimitedFlightNos?.Count > 0 ? policy.LimitedFlightNos[0] : null,
                Price = !string.IsNullOrEmpty(policy.Fare) && decimal.TryParse(policy.Fare, out decimal price) ? price : null,
                CreateTime = DateTime.Now
            };

            // 处理Details信息
            if (policy.Details?.Count > 0)
            {
                var detail = policy.Details[0];
                entity.StartFlightDate = detail.StartFlightDate;
                entity.EndFlightDate = detail.EndFlightDate;
                entity.AheadDays = !string.IsNullOrEmpty(detail.AheadDays) && int.TryParse(detail.AheadDays, out int ahead) ? ahead : null;
                entity.ExtraOpenCabin = detail.ExtraOpenCabin;
                entity.Rebate = !string.IsNullOrEmpty(detail.Rebate) && decimal.TryParse(detail.Rebate, out decimal rebate) ? rebate : null;
                entity.Retention = !string.IsNullOrEmpty(detail.Retention) && decimal.TryParse(detail.Retention, out decimal retention) ? retention : null;
                entity.Cabins = detail.Cabins?.Count > 0 ? string.Join(",", detail.Cabins) : null;
            }

            // 处理原始数据（这些需要在调用时设置）
            entity.OrgWeekDays = policy.orgWeekdaysLimit; // 需要在调用时设置
            entity.OrgExceptDateRanges = policy.orgExceptDateRanges; // 需要在调用时设置  
            entity.OrgFlightNosLimit = policy.orgFlightNosLimit; // 需要在调用时设置

            return entity;
        }

        // 私有辅助方法
        private void AddParameters(NpgsqlCommand command, MuAi34Entity entity)
        {
            command.Parameters.AddWithValue("@dep", (object?)entity.Dep ?? DBNull.Value);
            command.Parameters.AddWithValue("@arr", (object?)entity.Arr ?? DBNull.Value);
            command.Parameters.AddWithValue("@carrier", (object?)entity.Carrier ?? DBNull.Value);
            command.Parameters.AddWithValue("@flightno", (object?)entity.FlightNo ?? DBNull.Value);
            command.Parameters.AddWithValue("@cabins", (object?)entity.Cabins ?? DBNull.Value);
            command.Parameters.AddWithValue("@extraOpenCabin", (object?)entity.ExtraOpenCabin ?? DBNull.Value);
            command.Parameters.AddWithValue("@price", (object?)entity.Price ?? DBNull.Value);
            command.Parameters.AddWithValue("@rebate", (object?)entity.Rebate ?? DBNull.Value);
            command.Parameters.AddWithValue("@retention", (object?)entity.Retention ?? DBNull.Value);
            command.Parameters.AddWithValue("@StartFlightDate", (object?)entity.StartFlightDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@EndFlightDate", (object?)entity.EndFlightDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@AheadDays", (object?)entity.AheadDays ?? DBNull.Value);
            command.Parameters.AddWithValue("@orgWeekDays", (object?)entity.OrgWeekDays ?? DBNull.Value);
            command.Parameters.AddWithValue("@orgExceptDateRanges", (object?)entity.OrgExceptDateRanges ?? DBNull.Value);
            command.Parameters.AddWithValue("@orgFlightNosLimit", (object?)entity.OrgFlightNosLimit ?? DBNull.Value);
            command.Parameters.AddWithValue("@createTime", entity.CreateTime ?? DateTime.Now);
        }

        private MuAi34Entity ReadEntity(NpgsqlDataReader reader)
        {
            return new MuAi34Entity
            {
                Id = reader.GetInt32("id"),
                Dep = reader.IsDBNull("dep") ? null : reader.GetString("dep"),
                Arr = reader.IsDBNull("arr") ? null : reader.GetString("arr"),
                Carrier = reader.IsDBNull("carrier") ? null : reader.GetString("carrier"),
                FlightNo = reader.IsDBNull("flightno") ? null : reader.GetString("flightno"),
                Cabins = reader.IsDBNull("cabins") ? null : reader.GetString("cabins"),
                ExtraOpenCabin = reader.IsDBNull("extraOpenCabin") ? null : reader.GetString("extraOpenCabin"),
                Price = reader.IsDBNull("price") ? null : reader.GetDecimal("price"),
                Rebate = reader.IsDBNull("rebate") ? null : reader.GetDecimal("rebate"),
                Retention = reader.IsDBNull("retention") ? null : reader.GetDecimal("retention"),
                StartFlightDate = reader.IsDBNull("StartFlightDate") ? null : reader.GetString("StartFlightDate"),
                EndFlightDate = reader.IsDBNull("EndFlightDate") ? null : reader.GetString("EndFlightDate"),
                AheadDays = reader.IsDBNull("AheadDays") ? null : reader.GetInt32("AheadDays"),
                OrgWeekDays = reader.IsDBNull("orgWeekDays") ? null : reader.GetString("orgWeekDays"),
                OrgExceptDateRanges = reader.IsDBNull("orgExceptDateRanges") ? null : reader.GetString("orgExceptDateRanges"),
                OrgFlightNosLimit = reader.IsDBNull("orgFlightNosLimit") ? null : reader.GetString("orgFlightNosLimit"),
                CreateTime = reader.IsDBNull("createTime") ? null : reader.GetDateTime("createTime")
            };
        }

        /// <summary>
        /// 快速清空指定数据表的所有记录（使用TRUNCATE，最快方法）
        /// </summary>
        /// <param name="tableName">要清空的表名</param>
        /// <returns>是否成功清空</returns>
        public async Task<bool> FastClearTableAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("表名不能为空", nameof(tableName));
            }

            // 验证表名，防止SQL注入（只允许字母、数字和下划线）
            if (!System.Text.RegularExpressions.Regex.IsMatch(tableName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                throw new ArgumentException("表名格式不正确", nameof(tableName));
            }

            try
            {
                // 优化连接字符串，增加超时时间
                var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_connectionString)
                {
                    CommandTimeout = 300, // 5分钟超时
                    Timeout = 30 // 连接超时30秒
                };

                using var connection = new NpgsqlConnection(connectionStringBuilder.ToString());
                await connection.OpenAsync();

                // 使用TRUNCATE TABLE，比DELETE快得多
                string sql = $"TRUNCATE TABLE {tableName} RESTART IDENTITY CASCADE";
                
                using var command = new NpgsqlCommand(sql, connection);
                command.CommandTimeout = 300; // 5分钟超时
                await command.ExecuteNonQueryAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TRUNCATE清空表失败: {ex.Message}");
                throw new Exception($"快速清空表 '{tableName}' 失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 超快速清空表（DROP + CREATE，最快但会重置表结构）
        /// </summary>
        /// <param name="tableName">要清空的表名</param>
        /// <returns>是否成功</returns>
        public async Task<bool> SuperFastClearTableAsync(string tableName = "muai34")
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("表名不能为空", nameof(tableName));
            }

            try
            {
                // 优化连接字符串
                var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_connectionString)
                {
                    CommandTimeout = 300,
                    Timeout = 30
                };

                using var connection = new NpgsqlConnection(connectionStringBuilder.ToString());
                await connection.OpenAsync();

                // 先删除表
                string dropSql = $"DROP TABLE IF EXISTS {tableName}";
                using (var dropCommand = new NpgsqlCommand(dropSql, connection))
                {
                    dropCommand.CommandTimeout = 300;
                    await dropCommand.ExecuteNonQueryAsync();
                }

                // 重新创建表结构（专门针对muai34表）
                string createSql = @"
                    CREATE TABLE muai34 (
                        id SERIAL PRIMARY KEY,
                        dep varchar(255),
                        arr varchar(255),
                        carrier varchar(255),
                        flightno varchar(255),
                        cabins varchar(255),
                        ""extraOpenCabin"" varchar(255),
                        price numeric(10,0),
                        rebate numeric(10,2),
                        retention numeric(10,0),
                        ""StartFlightDate"" varchar(255),
                        ""EndFlightDate"" varchar(255),
                        ""AheadDays"" int4,
                        ""orgWeekDays"" varchar(255),
                        ""orgExceptDateRanges"" varchar(255),
                        ""orgFlightNosLimit"" varchar(255),
                        ""createTime"" timestamp(6)
                    )";

                using (var createCommand = new NpgsqlCommand(createSql, connection))
                {
                    createCommand.CommandTimeout = 300;
                    await createCommand.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"超快速清空表失败: {ex.Message}");
                throw new Exception($"超快速清空表 '{tableName}' 失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 智能清空表（自动选择最佳清理方法）
        /// </summary>
        /// <param name="tableName">要清空的表名</param>
        /// <returns>返回清理结果信息</returns>
        public async Task<string> SmartClearTableAsync(string tableName = "muai34")
        {
            try
            {
                // 方法1：尝试TRUNCATE（最快且保留表结构）
                try
                {
                    await FastClearTableAsync(tableName);
                    return "使用TRUNCATE成功清空表";
                }
                catch (Exception ex1)
                {
                    System.Diagnostics.Debug.WriteLine($"TRUNCATE失败: {ex1.Message}");
                }

                // 方法2：尝试DROP+CREATE（超快但重建表）
                try
                {
                    await SuperFastClearTableAsync(tableName);
                    return "使用DROP+CREATE成功清空表";
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"DROP+CREATE失败: {ex2.Message}");
                }

                // 方法3：最后使用原始DELETE方法（慢但兼容性好）
                int deletedRows = await ClearTableSlowAsync(tableName);
                return $"使用DELETE成功清空表，删除了{deletedRows}行数据";
            }
            catch (Exception ex)
            {
                throw new Exception($"所有清空方法都失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 清空指定数据表的所有记录（原DELETE方法，重命名为慢速版本）
        /// </summary>
        /// <param name="tableName">要清空的表名</param>
        /// <returns>返回被删除的记录数量</returns>
        public async Task<int> ClearTableSlowAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("表名不能为空", nameof(tableName));
            }

            // 验证表名，防止SQL注入（只允许字母、数字和下划线）
            if (!System.Text.RegularExpressions.Regex.IsMatch(tableName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                throw new ArgumentException("表名格式不正确", nameof(tableName));
            }

            try
            {
                // 优化连接字符串，增加超时时间
                var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_connectionString)
                {
                    CommandTimeout = 600, // 10分钟超时
                    Timeout = 60 // 连接超时60秒
                };

                using var connection = new NpgsqlConnection(connectionStringBuilder.ToString());
                await connection.OpenAsync();

                // 先获取总行数
                string countSql = $"SELECT COUNT(*) FROM {tableName}";
                using var countCommand = new NpgsqlCommand(countSql, connection);
                var totalRows = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

                if (totalRows == 0)
                {
                    return 0; // 表已经是空的
                }

                // 使用DELETE删除数据
                string sql = $"DELETE FROM {tableName}";
                
                using var command = new NpgsqlCommand(sql, connection);
                command.CommandTimeout = 600; // 10分钟超时
                int deletedRows = await command.ExecuteNonQueryAsync();
                
                return deletedRows;
            }
            catch (Exception ex)
            {
                throw new Exception($"清空表 '{tableName}' 失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 保持向后兼容的清空表方法（现在使用智能清理）
        /// </summary>
        /// <param name="tableName">要清空的表名</param>
        /// <returns>返回被删除的记录数量或成功标识</returns>
        public async Task<int> ClearTableAsync(string tableName)
        {
            try
            {
                string result = await SmartClearTableAsync(tableName);
                System.Diagnostics.Debug.WriteLine(result);
                return 1; // 成功标识
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清空表失败: {ex.Message}");
                return 0; // 失败标识
            }
        }

        /// <summary>
        /// 清空指定数据表的所有记录（同步版本，为兼容性保留）
        /// </summary>
        /// <param name="tableName">要清空的表名</param>
        /// <returns>返回被删除的记录数量</returns>
        public int ClearTable(string tableName)
        {
            return ClearTableAsync(tableName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 重置表的自增ID序列（适用于PostgreSQL）
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="sequenceName">序列名，如果为空则自动推断为 tablename_id_seq</param>
        /// <returns>是否成功重置</returns>
        public async Task<bool> ResetAutoIncrementAsync(string tableName, string sequenceName = "")
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("表名不能为空", nameof(tableName));
            }

            // 验证表名，防止SQL注入
            if (!System.Text.RegularExpressions.Regex.IsMatch(tableName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                throw new ArgumentException("表名格式不正确", nameof(tableName));
            }

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // 如果没有指定序列名，使用默认规则
                if (string.IsNullOrWhiteSpace(sequenceName))
                {
                    sequenceName = $"{tableName}_id_seq";
                }

                // 验证序列名
                if (!System.Text.RegularExpressions.Regex.IsMatch(sequenceName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                {
                    throw new ArgumentException("序列名格式不正确", nameof(sequenceName));
                }

                // 重置序列到1
                string sql = $"ALTER SEQUENCE {sequenceName} RESTART WITH 1";
                
                using var command = new NpgsqlCommand(sql, connection);
                await command.ExecuteNonQueryAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"重置自增序列失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清空表并重置自增ID（组合操作）
        /// </summary>
        /// <param name="tableName">要清空的表名</param>
        /// <param name="resetAutoIncrement">是否重置自增ID</param>
        /// <returns>返回被删除的记录数量</returns>
        public async Task<int> ClearTableAndResetAsync(string tableName, bool resetAutoIncrement = true)
        {
            int deletedRows = await ClearTableAsync(tableName);
            
            if (resetAutoIncrement && deletedRows > 0)
            {
                await ResetAutoIncrementAsync(tableName);
            }
            
            return deletedRows;
        }
    }
} 
using System;
using System.IO;
using Newtonsoft.Json;

namespace WF_MUAI_34
{
    public class Config
    {
        private const string CONFIG_FILE = "config.json";
        
        public string AliCloudApiKey { get; set; } = "sk-7be51a35e4ea4ee9bc3e0d2584e0a33a";
        public string AliCloudApiUrl { get; set; } = "https://dashscope.aliyuncs.com/api/v1/services/aigc/multimodal-generation/generation";
        public string AliCloudModel { get; set; } = "qwen-vl-plus";
        public bool EnableAutoLogin { get; set; } = true;
        public string LoginUsername { get; set; } = "18988486220";
        public string LoginPassword { get; set; } = "zg123456";
        public bool UseAliCloudOCR { get; set; } = true;
        
        private static Config? _instance;
        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadConfig();
                }
                return _instance;
            }
        }
        
        private static Config LoadConfig()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    string json = File.ReadAllText(CONFIG_FILE);
                    var config = JsonConvert.DeserializeObject<Config>(json);
                    return config ?? new Config();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载配置文件失败: {ex.Message}");
            }
            
            // 如果加载失败或文件不存在，创建默认配置
            var defaultConfig = new Config();
            defaultConfig.SaveConfig();
            return defaultConfig;
        }
        
        public void SaveConfig()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存配置文件失败: {ex.Message}");
            }
        }
    }
} 
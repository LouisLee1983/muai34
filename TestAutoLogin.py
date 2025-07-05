#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
自动登录功能测试脚本
用于验证B3BForm的自动登录功能是否正常工作
"""

import json
import os
import sys

def create_test_config():
    """创建测试配置文件"""
    config = {
        "AliCloudApiKey": "sk-7be51a35e4ea4ee9bc3e0d2584e0a33a",
        "AliCloudApiUrl": "https://dashscope.aliyuncs.com/api/v1/services/aigc/multimodal-generation/generation",
        "AliCloudModel": "qwen-vl-plus",
        "EnableAutoLogin": True,
        "LoginUsername": "18988486220",
        "LoginPassword": "zg123456",
        "UseAliCloudOCR": True
    }
    
    with open("config.json", "w", encoding="utf-8") as f:
        json.dump(config, f, indent=2, ensure_ascii=False)
    
    print("✓ 测试配置文件已创建: config.json")
    return config

def validate_config():
    """验证配置文件"""
    if not os.path.exists("config.json"):
        print("❌ 配置文件不存在，正在创建...")
        return create_test_config()
    
    try:
        with open("config.json", "r", encoding="utf-8") as f:
            config = json.load(f)
        
        required_fields = [
            "AliCloudApiKey", "AliCloudApiUrl", "AliCloudModel",
            "EnableAutoLogin", "LoginUsername", "LoginPassword", "UseAliCloudOCR"
        ]
        
        missing_fields = [field for field in required_fields if field not in config]
        if missing_fields:
            print(f"❌ 配置文件缺少字段: {', '.join(missing_fields)}")
            return None
        
        print("✓ 配置文件验证通过")
        return config
    except Exception as e:
        print(f"❌ 配置文件读取失败: {e}")
        return None

def print_test_instructions():
    """打印测试说明"""
    print("\n" + "="*60)
    print("🔍 自动登录功能测试说明")
    print("="*60)
    print("1. 确保已编译并运行 WF_MUAI_34.exe")
    print("2. 点击 '打开B3B' 按钮")
    print("3. 观察是否自动跳转到登录页面")
    print("4. 检查是否自动填写用户名和密码")
    print("5. 验证是否自动勾选'记住密码'")
    print("6. 观察验证码识别和自动登录过程")
    print("\n📋 测试检查点:")
    print("- [ ] 自动检测登录页面")
    print("- [ ] 自动填写用户名: 18988486220")
    print("- [ ] 自动填写密码: zg123456")
    print("- [ ] 自动勾选记住密码")
    print("- [ ] 验证码识别或手动输入")
    print("- [ ] 自动点击登录按钮")
    print("- [ ] 登录成功后停止检查")
    print("="*60)

def main():
    """主函数"""
    print("🚀 B3B自动登录功能测试工具")
    print("-" * 40)
    
    # 验证配置文件
    config = validate_config()
    if not config:
        print("❌ 配置验证失败，请检查配置文件")
        return 1
    
    # 显示当前配置
    print(f"📊 当前配置:")
    print(f"  - 自动登录: {'启用' if config['EnableAutoLogin'] else '禁用'}")
    print(f"  - 用户名: {config['LoginUsername']}")
    print(f"  - 密码: {'*' * len(config['LoginPassword'])}")
    print(f"  - 阿里云OCR: {'启用' if config['UseAliCloudOCR'] else '禁用'}")
    
    if config['AliCloudApiKey'] == "YOUR_API_KEY":
        print("⚠️  阿里云API Key未配置，将使用手动验证码输入")
    else:
        print("✓ 阿里云API Key已配置")
    
    # 打印测试说明
    print_test_instructions()
    
    print("\n🎯 测试准备完成！请按照上述说明进行测试。")
    return 0

if __name__ == "__main__":
    sys.exit(main()) 
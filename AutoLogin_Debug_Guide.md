# B3B自动登录调试指南

## 🔍 问题排查步骤

### 1. 启用调试输出
在Visual Studio中：
1. 打开 `视图` -> `输出`
2. 在输出窗口中选择 `调试`
3. 运行程序时可以看到详细的调试信息

### 2. 测试步骤

#### 第一步：基本功能测试
1. 运行程序
2. 点击"打开B3B"按钮
3. 观察调试输出中是否显示：
   ```
   自动登录检查已启动
   导航到: https://fuwu.cddyf.net/
   自动登录定时器已启动，间隔: 2000ms
   ```

#### 第二步：URL检查
等待页面跳转到登录页面，观察调试输出：
```
当前URL: https://oper.cddyf.net/Login.aspx#
检测到登录页面，开始自动登录...
```

#### 第三步：页面元素检查
如果自动登录没有触发，可以手动调用测试方法：
```csharp
// 在代码中添加临时按钮或在窗体Load事件中调用
await TestPageElementsAsync();
```

### 3. 常见问题及解决方案

#### 问题1：定时器没有启动
**症状**：没有看到"自动登录定时器已启动"的消息
**解决**：
1. 检查`InitializeAutoLoginTimer()`是否在构造函数中被调用
2. 确认`EnableAutoLogin()`在`buttonOpenB3B_Click`中被调用

#### 问题2：没有检测到登录页面
**症状**：看到URL检查但没有"检测到登录页面"的消息
**可能原因**：
- URL不包含"Login.aspx"
- 页面还在加载中

**解决**：
1. 检查实际的登录页面URL
2. 增加等待时间

#### 问题3：页面元素不存在
**症状**：看到"登录表单不存在"或元素填写失败
**解决**：
1. 使用`TestPageElementsAsync()`检查页面元素
2. 确认元素ID是否正确：
   - 用户名：`txtUserName`
   - 密码：`txtPassword`
   - 验证码：`txtCode`
   - 记住密码：`chkJizhu`
   - 登录按钮：`btnLogon`

### 4. 手动测试方法

#### 方法1：添加测试按钮
在窗体设计器中添加一个测试按钮：
```csharp
private async void buttonTest_Click(object sender, EventArgs e)
{
    await TestPageElementsAsync();
}
```

#### 方法2：使用即时窗口
在Visual Studio调试时，在即时窗口中输入：
```csharp
await ((B3BForm)this).TriggerAutoLoginAsync()
```

### 5. 调试输出说明

#### 正常流程的调试输出应该是：
```
自动登录检查已启动
自动登录定时器已启动，间隔: 2000ms
导航到: https://fuwu.cddyf.net/
当前URL: https://fuwu.cddyf.net/
当前URL: https://oper.cddyf.net/Login.aspx#
检测到登录页面，开始自动登录...
开始执行自动登录...
登录表单存在，开始填写...
开始填写 txtUserName = 18988486220
填写 txtUserName 结果: success
✓ 成功填写 txtUserName
开始填写 txtPassword = zg123456
填写 txtPassword 结果: success
✓ 成功填写 txtPassword
开始勾选记住密码
勾选记住密码结果: checked
✓ 记住密码已勾选
自动填写完成
```

### 6. 如果仍然不工作

#### 检查配置文件
确认`config.json`中的设置：
```json
{
  "EnableAutoLogin": true,
  "LoginUsername": "18988486220",
  "LoginPassword": "zg123456"
}
```

#### 检查页面加载时机
有时页面需要更长时间加载，可以：
1. 增加等待时间（在`LoginCheckTimer_Tick`中的`Task.Delay`）
2. 添加页面加载完成的事件监听

#### 手动触发测试
在登录页面加载完成后，手动调用：
```csharp
await TriggerAutoLoginAsync();
```

### 7. 紧急解决方案

如果自动检测不工作，可以添加一个手动触发按钮：

```csharp
private async void buttonManualLogin_Click(object sender, EventArgs e)
{
    try
    {
        // 直接执行填写操作
        await FillInputFieldAsync("txtUserName", "18988486220");
        await Task.Delay(500);
        await FillInputFieldAsync("txtPassword", "zg123456");
        await Task.Delay(500);
        await CheckRememberPasswordAsync();
        
        MessageBox.Show("用户名和密码已填写完成！", "手动填写", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"手动填写失败: {ex.Message}", "错误", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

### 8. 联系支持

如果以上步骤都无法解决问题，请提供：
1. 完整的调试输出
2. 实际的登录页面URL
3. 页面元素检查的结果
4. 错误信息（如果有） 
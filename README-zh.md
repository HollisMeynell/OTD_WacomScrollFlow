# WacomScrollFlow

[English](README.md) | 简体中文

Windows 上的 OpenTabletDriver 滚动插件。使用 .NET 8，针对 OTD 0.6.6.2 插件 API 编译；参考了 0.6.7 的输入管线源码。已在 OpenTabletDriver 0.6.7 中实机测试通过。

## 操作方式

1. 按住指定按键，立即固定光标；悬空移动笔不滚动。
2. 笔尖达到压力阈值后，开始识别滑动方向。默认向上划 → 向下滚，向下划 → 向上滚；向左划 → 向右滚，向右划 → 向左滚。
3. 保留纵向滚动逻辑；从本次落笔开始，转换后的横向输出位移超过 100px 且大于纵向位移的 3 倍时，切换为横向滚动，抬笔后重新判断。绝对模式使用输出坐标差，相对模式累计输出位移。
4. 松开按键时，如果笔尖仍接触板面，停止滚动并继续固定光标、屏蔽笔尖点击，直到抬笔才恢复正常。未抬笔时再次按键可继续滚动，保留方向且不补发暂停期间的位移。绝对模式会回到当前笔位置；相对模式在滚动期间持续更新内部位置，避免积累位移。

滚动期间笔尖压力对下游置零，避免触发左键拖动。光标锁定针对这支笔的输出，其他鼠标设备仍能移动光标。

## 构建与测试

安装 .NET 8 SDK（8.0.403 或同一主版本的更新功能版本），在仓库根目录运行：

```powershell
dotnet build src/WacomScrollFlow/WacomScrollFlow.csproj -c Release
dotnet test tests/WacomScrollFlow.Tests/WacomScrollFlow.Tests.csproj -c Release
```

源码位于 `src/WacomScrollFlow/`，xUnit 测试位于 `tests/WacomScrollFlow.Tests/`。测试覆盖手势方向、阈值、抬笔重置、细小位移累积，以及过滤器的按键、锁定、压力屏蔽和释放行为。

## 安装和配置

在 OTD 中打开插件目录，在其下新建 `WacomScrollFlow` 文件夹，将构建生成的 `src/WacomScrollFlow/bin/Release/net8.0/WacomScrollFlow.dll` 放进去，然后完整重启 OTD。不要复制 OTD 自身的 DLL。

在同一数位板配置中启用两个过滤器：

- **Scroll Flow: Input**：放在其他过滤器之前，读取原始坐标和压力。
- **Scroll Flow: Cursor Lock**：放在其他过滤器之后，锁定转换后的输出。使用 OTD Relative Mode 时，把它的 **Relative output mode** 设为 `true`；Absolute Mode 保持 `false`。

目前面向标准 Absolute Mode / Relative Mode；Windows Ink、其他自定义输出模式和异步插值过滤器未验证。先关闭其他过滤器进行验证。

Input 的配置项：

| 配置 | 默认值 | 含义 |
| --- | --- | --- |
| Button source | Pen | `Pen` 笔侧键、`Aux` 板上快捷键、`Keyboard` Windows 键盘 |
| Button number / virtual key | 1 | Pen/Aux 从 1 开始；Keyboard 使用十进制虚拟键码，例如空格 32、Shift 16、F8 119 |
| Tip pressure (%) | 1 | 开始识别滑动所需的压力百分比 |
| Vertical threshold (mm) | 0.5 | 进入纵向滚动的位移 |
| 横向切换阈值（固定） | 100px | 横向输出位移必须严格超过此值 |
| 横向优势比例（固定） | 3 | 横向位移必须严格大于纵向位移的 3 倍，可从纵向切换 |
| Wheel units per mm | 24 | 每毫米滚动量，Windows 的 120 单位通常相当于一格滚轮 |
| Reverse vertical | false | 反转纵向滚动方向 |
| Reverse horizontal | false | 反转横向滚动方向 |

按键直接在插件中选择，不需要给它绑定一个 OTD 动作。插件会屏蔽选中的笔侧键或快捷键原有的 OTD 动作，同时在滚动期间屏蔽笔尖点击。Keyboard 使用轮询，不拦截键盘自身输入；按键状态在下一份设备报告到来时更新。

## 实机验收

在支持横向滚动的窗口中检查：按住键悬空移动时光标固定；落笔上下滑动时页面反向滚动；轻微横向偏移不改变轴；横向位移超过 100px 且大于纵向的 3 倍时切换横向；落笔松键后仍固定且不点击，重按继续滚动，松键抬笔后恢复移动。分别验证绝对和相对模式，并检查原有笔尖点击正常。

## 实现参考

OTD 的 [OutputMode 输入管线](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/v0.6.7/OpenTabletDriver.Plugin/Output/OutputMode.cs) 分为坐标转换前后两个阶段。本插件在前阶段计算毫米位移并屏蔽压力和滚动按键，在后阶段保留最后一次 OTD 输出的绝对位置或清零相对位移，不混用 Windows 屏幕坐标；滚轮通过 Windows `SendInput` 输出。若启用后第一份报告就是按住状态，且尚无可用的绝对位置，则暂不输出移动，直到松键恢复。



## 自动发布

推送 `v0.1.2` 格式的 tag 会触发 GitHub Actions：校验 tag 与项目版本一致，运行测试、构建插件，并自动创建包含 ZIP 和 SHA256 校验文件的 GitHub Release。无需额外配置密钥。

后续发布先修改 `src/WacomScrollFlow/WacomScrollFlow.csproj` 中的 `<Version>`，提交并推送，再创建对应标签，例如：

```powershell
git tag -a v0.1.3 -m "Release v0.1.3"
git push origin v0.1.3
```

本地构建后也可运行 `./scripts/Package.ps1 -Version 0.1.2` 打包。发布 ZIP 的根目录直接包含 DLL 和中英文说明。GitHub Release 不会自动进入 Plugins Manager，仍需单独向官方插件索引提交收录申请。

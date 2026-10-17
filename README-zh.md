# WacomScrollFlow

[English](README.md) | 简体中文

用笔滚动页面，同时保持光标不动。按住笔侧键或数位板按钮，落笔后移动即可纵向或横向滚动。笔尖未抬起时松开按钮会暂停滚动，避免误触点击。

适用于 Windows、Linux 和 macOS 上的 OpenTabletDriver 0.6.7 及以上版本，使用标准 Absolute Mode 或 Relative Mode。Linux 和 macOS 的实际桌面滚动效果仍需实机验证。

## 安装

1. 从 [Releases](https://github.com/HollisMeynell/OTD_WacomScrollFlow/releases/latest) 下载 `WacomScrollFlow-<版本>.zip`。
2. 在 OpenTabletDriver 中打开插件目录，新建 `WacomScrollFlow` 文件夹，将 ZIP 解压进去，确保 `WacomScrollFlow.dll` 直接位于该文件夹内。
3. 完整重启 OpenTabletDriver。
4. 在同一数位板配置中启用两个过滤器：
   - **Scroll Flow: Input**：放在其他过滤器之前。
   - **Scroll Flow: Cursor Lock**：放在其他过滤器之后。
5. 在 **Scroll Flow: Cursor Lock** 中设置 **Relative output mode**：OTD Relative Mode 设为 `true`，Absolute Mode 设为 `false`。
6. 在 **Scroll Flow: Input** 中选择 **Button source** 和 **Button number**。

## 使用

- 按住配置的按钮，笔光标固定；悬空移动不会滚动。
- 落笔后上下移动即可滚动。默认向上划向下滚，向下划向上滚。
- 横向位移超过 **100 输出像素**，且大于纵向位移的 **3 倍**时，切换为横向滚动。默认向左划向右滚，向右划向左滚。抬笔后重新判断方向。
- 松开按钮暂停滚动。如果笔尖仍接触板面，光标继续固定，并屏蔽笔尖点击直到抬笔；未抬笔时再次按住按钮可继续滚动。

插件启用后会屏蔽选中 Pen/Aux 按钮原有的 OTD 动作，无需额外绑定动作。其他鼠标设备仍能移动光标。

## 设置

以下选项位于 **Scroll Flow: Input**：

| 设置 | 默认值 | 说明 |
| --- | --- | --- |
| Button source | Pen | `Pen` 为笔侧键，`Aux` 为数位板按钮 |
| Button number | 1 | 按钮编号，从 1 开始 |
| Tip pressure (%) | 1 | 开始识别滚动所需的最低压力百分比 |
| Vertical threshold (mm) | 0.5 | 开始纵向滚动前需要移动的距离 |
| Wheel units per mm | 24 | 滚动速度，数值越大滚动越快 |
| Reverse vertical | false | 反转纵向滚动方向 |
| Reverse horizontal | false | 反转横向滚动方向 |

横向切换阈值和比例固定。**Relative output mode** 位于 **Scroll Flow: Cursor Lock** 中，必须与 OTD 输出模式一致。

Windows 和 Linux 的 120 滚轮单位对应一格，macOS 使用像素滚动单位。可根据平台和习惯调整 **Wheel units per mm**。

## 常见问题

- **不能滚动：** 确认两个过滤器均已启用，按钮来源、编号和 **Relative output mode** 设置正确，并且落笔压力达到阈值。
- **松开按钮后光标仍固定：** 抬起笔尖即可恢复，这是为了避免滚动结束时误触点击。
- **滚动方向不习惯：** 开启对应的 **Reverse** 选项。
- **与其他过滤器冲突：** 先关闭其他过滤器，再逐一启用排查。Windows Ink、自定义输出模式和异步插值过滤器尚未验证。
- **输出模式不支持：** 如果当前指针不支持滚轮，插件会放行正常笔输入，并在 OTD 日志中记录警告。

## 许可证

[MIT No Attribution（MIT-0）](LICENSE)。允许使用、修改、分发和商用，无需保留署名。

# LottieNet

[![NuGet](https://img.shields.io/nuget/v/LottieNet.svg)](https://www.nuget.org/packages/LottieNet/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)

一个高性能的 WPF Lottie 动画库，基于 SkiaSharp.Skottie 实现。支持两种渲染模式以适应不同的应用场景。

## ✨ 特性

- 🎨 **两种渲染模式**
  - **Dynamic（动态渲染）**: 实时渲染每一帧，内存占用小
  - **Prerendered（预渲染）**: 预渲染所有帧为图片序列，完全避免渲染干扰

- 🚀 **高性能**
  - 基于 SkiaSharp 硬件加速
  - 智能可见性优化（不可见时自动停止渲染）
  - 可配置帧率控制

- 🎯 **易于使用**
  - 统一的 XAML 控件接口
  - 丰富的依赖属性支持数据绑定
  - 完全集成到 WPF 生态

## 📦 安装

通过 NuGet 安装：

```bash
dotnet add package LottieNet
```

或者在 Visual Studio 中使用包管理器控制台：

```powershell
Install-Package LottieNet
```

## 🚀 快速开始

### 基本用法

在 XAML 中引入命名空间：

```xml
xmlns:lottie="clr-namespace:LottieNet.Controls;assembly=LottieNet"
```

使用 LottieView 控件：

```xml
<lottie:LottieView
    FileName="Assets/animation.json"
    RenderMode="Prerendered"
    IsPlaying="True"
    Fps="30"
    RepeatCount="-1"
    Width="200"
    Height="200" />
```

### 渲染模式选择

#### Dynamic（动态渲染）

```xml
<lottie:LottieView
    RenderMode="Dynamic"
    FileName="animation.json"
    IsPlaying="True" />
```

**优点**: 内存占用小，支持任意帧率
**缺点**: 可能影响其他 WPF 控件的渲染

#### Prerendered（预渲染）

```xml
<lottie:LottieView
    RenderMode="Prerendered"
    FileName="animation.json"
    IsPlaying="True"
    Fps="30" />
```

**优点**: 完全避免 SKElement 的 InvalidateVisual 问题，不影响其他控件
**缺点**: 内存占用较大，初始化时间较长

## 📖 属性说明

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `FileName` | `string` | `null` | Lottie 动画文件路径（.json） |
| `RenderMode` | `RenderMode` | `Prerendered` | 渲染模式（Dynamic/Prerendered） |
| `IsPlaying` | `bool` | `false` | 是否正在播放 |
| `Fps` | `int` | `30` | 帧率 |
| `RepeatCount` | `int` | `-1` | 重复次数（-1 表示无限循环） |
| `Repeat` | `RepeatMode` | `Restart` | 重复模式（Restart/Reverse）* |

> *注意：`Reverse` 模式目前尚未实现，计划在 v1.1.0 版本中提供。

## 🎯 使用场景

- ✅ 启动画面和加载动画
- ✅ UI 交互反馈
- ✅ 图标和按钮动画
- ✅ 数据可视化动画
- ✅ 品牌标识动画

## 🔧 系统要求

- .NET 8.0 或更高版本
- Windows 10 或更高版本
- WPF 应用程序

## 📚 示例项目

查看 [Samples](Samples) 目录获取完整的示例代码。

## 🤝 贡献

欢迎贡献代码！请查看 [贡献指南](docs/待办清单.md) 了解详情。

## 📝 许可证

本项目采用 [MIT License](LICENSE) 开源许可证。

## 🙏 致谢

- [SkiaSharp](https://github.com/mono/SkiaSharp) - 强大的跨平台 2D 图形库
- [SkiaSharp.Skottie](https://github.com/mono/SkiaSharp) - Lottie 动画引擎

## 📮 联系方式

- 项目主页: https://github.com/Hopesy/LottieNet
- 问题反馈: https://github.com/Hopesy/LottieNet/issues

---

⭐ 如果这个项目对你有帮助，请给个 Star！

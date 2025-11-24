# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

LottieNet 是一个用于 WPF 的 Lottie 动画库，基于 SkiaSharp.Skottie 实现。该库提供了两种渲染模式来适应不同的应用场景：

- **Dynamic（动态渲染）**：使用 SKElement 实时渲染每一帧，内存占用小但可能影响其他 WPF 控件
- **Prerendered（预渲染）**：启动时将所有帧预渲染为图片序列，内存占用大但完全避免渲染干扰

## 解决方案结构

解决方案包含两个项目：

1. **LottieNet** - 核心库项目 (.NET 8, WPF)
   - 提供可重用的 Lottie 控件
   - 生成 NuGet 包供其他项目引用

2. **Samples** - 示例应用程序 (.NET 8, WPF)
   - 演示如何使用 LottieNet 控件
   - 用于测试和验证功能

## 核心架构

### 控件层次结构

```
LottieView (统一控件)
├── DynamicLottieView (动态渲染实现)
│   └── 继承自 SKElement
│   └── 实时渲染每一帧
└── PrerenderedLottieView (预渲染实现)
    └── 继承自 Control
    └── 预渲染所有帧到图片序列
```

### 关键设计模式

**LottieView 作为适配器模式**：
- LottieView 根据 `RenderMode` 属性动态创建并切换 DynamicLottieView 或 PrerenderedLottieView
- 使用反射将属性同步到内部控件（见 LottieView.cs:172 的 `SetPropertyValue` 方法）
- 通过 PART_ContentHost（ContentControl）承载实际的渲染控件

**依赖属性一致性**：
- 所有三个控件（LottieView, DynamicLottieView, PrerenderedLottieView）都暴露相同的依赖属性：
  - `FileName`: 动画文件路径
  - `IsPlaying`: 播放状态
  - `Fps`: 帧率
  - `RepeatCount`: 重复次数（-1 表示无限循环）
  - `Repeat`: 重复模式（Restart/Reverse）

### 样式与模板

所有控件样式定义在 `LottieNet/Themes/Styles/LottieStyles.xaml` 中：
- PrerenderedLottieView 使用 `Image` 控件作为 PART_Image
- LottieView 使用 `ContentControl` 作为 PART_ContentHost
- 样式通过 `Generic.xaml` 自动合并

## 常用命令

### 构建与测试
```bash
# 构建整个解决方案
dotnet build

# 清理构建输出
dotnet clean

# 还原 NuGet 包
dotnet restore

# 运行示例应用
dotnet run --project Samples/Samples.csproj
```

### 包管理
```bash
# 查看已安装的包
dotnet list package

# 构建时生成 NuGet 包（需要在 .csproj 中设置 GeneratePackageOnBuild=True）
dotnet pack LottieNet/LottieNet.csproj
```

## 开发注意事项

### SkiaSharp 渲染特性

**DynamicLottieView 的可见性优化**：
- 控件在不可见时会自动停止渲染（见 DynamicLottieView.cs:149-163）
- 使用 `IsVisibleAndEnabled()` 方法检查 Visibility、IsEnabled 和 IsVisible 属性
- 避免在后台浪费 CPU 资源

**PrerenderedLottieView 的尺寸依赖**：
- 预渲染发生在 `OnApplyTemplate` 之后（见 PrerenderedLottieView.cs:147-161）
- 使用控件的 Width/Height 属性作为渲染尺寸（默认 32x32）
- 所有帧被冻结（Freeze）以支持跨线程使用

### 属性同步机制

LottieView 使用反射同步属性到内部控件：
- 优点：避免重复代码，自动适配两种渲染模式
- 缺点：运行时开销，类型安全性降低
- 如果添加新属性，确保在三个控件中都定义相同的依赖属性

### 调试输出

PrerenderedLottieView 包含详细的 Console.WriteLine 调试输出：
- 加载过程、帧数、渲染进度
- 如果遇到渲染问题，检查控制台输出以诊断问题

## 依赖项

- **SkiaSharp.Skottie** (3.119.1) - Lottie 动画解析和渲染引擎
- **SkiaSharp.Views.WPF** (3.119.1) - WPF 的 SkiaSharp 集成（提供 SKElement）

## 常见问题

### 编译错误
如果遇到找不到属性或方法的错误，可能是因为：
1. XAML 设计器缓存未刷新 - 执行 `dotnet clean` 然后 `dotnet build`
2. Generic.xaml 资源字典未正确加载 - 检查 AssemblyInfo.cs 中的 ThemeInfo 特性

### 运行时问题
1. **动画不显示**：检查 FileName 路径是否正确，查看控制台输出的错误信息
2. **帧率不稳定**：DynamicLottieView 使用 DispatcherTimer，受 UI 线程繁忙程度影响
3. **内存占用高**：PrerenderedLottieView 会预渲染所有帧，考虑降低 Fps 或使用 Dynamic 模式

### RepeatMode.Reverse 支持
当前 Reverse 模式仅在枚举中定义，但两个渲染控件中都未实现。如需实现，需要：
- DynamicLottieView: 反向调整 Stopwatch 时间
- PrerenderedLottieView: 反向遍历 _frames 列表

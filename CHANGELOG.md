# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- RepeatMode.Reverse 反向播放功能
- 异步加载和预渲染
- 性能优化（内存占用优化）
- 更丰富的动画控制 API（暂停、恢复、跳转等）
- 单元测试覆盖

## [1.0.0] - 2025-01-24

### Added
- 初始版本发布
- LottieView 统一控件，支持通过 RenderMode 切换渲染模式
- DynamicLottieView 动态渲染模式
  - 基于 SKElement 实时渲染
  - 支持可见性优化
  - 支持自定义帧率
  - 支持循环播放
- PrerenderedLottieView 预渲染模式
  - 预渲染所有帧到图片序列
  - 完全避免 SKElement 的 InvalidateVisual 问题
  - 支持循环播放
- 依赖属性支持：FileName, IsPlaying, Fps, RepeatCount, Repeat
- XAML 样式和模板支持
- 基于 SkiaSharp.Skottie 3.119.1

### Known Issues
- RepeatMode.Reverse 模式尚未实现
- PrerenderedLottieView 对于长时间动画内存占用较高
- 同步加载可能在大型动画文件时阻塞 UI 线程

[Unreleased]: https://github.com/Hopesy/LottieNet/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/Hopesy/LottieNet/releases/tag/v1.0.0

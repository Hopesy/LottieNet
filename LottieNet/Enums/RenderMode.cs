namespace LottieNet.Enums;

/// <summary>
/// Lottie 动画的渲染模式
/// </summary>
public enum RenderMode
{
    /// <summary>
    /// 动态渲染模式 - 使用 SKElement 实时渲染每一帧（类似 LottieSharp）
    /// 优点：内存占用小，支持任意帧率
    /// 缺点：可能影响其他 WPF 控件的渲染
    /// </summary>
    Dynamic,

    /// <summary>
    /// 预渲染模式 - 启动时渲染所有帧到图片序列
    /// 优点：完全避免 SKElement 的 InvalidateVisual 问题，不影响其他控件
    /// 缺点：内存占用较大，初始化时间较长
    /// </summary>
    Prerendered
}

/// <summary>
/// 动画重复模式
/// </summary>
public enum RepeatMode
{
    /// <summary>
    /// 从头开始重复
    /// </summary>
    Restart,

    /// <summary>
    /// 反向重复
    /// </summary>
    Reverse
}

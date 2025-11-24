using System.Windows;
using System.Windows.Controls;
using LottieNet.Enums;

namespace LottieNet.Controls;

/// <summary>
/// 统一的 Lottie 动画控件
/// 可根据 RenderMode 自动选择动态渲染或预渲染模式
/// </summary>
public class LottieView : Control
{
    private ContentControl? _contentHost;
    private FrameworkElement? _activeControl;

    static LottieView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LottieView),
            new FrameworkPropertyMetadata(typeof(LottieView)));
    }

    #region 依赖属性

    public static readonly DependencyProperty RenderModeProperty =
        DependencyProperty.Register(
            nameof(RenderMode),
            typeof(RenderMode),
            typeof(LottieView),
            new PropertyMetadata(RenderMode.Prerendered, OnRenderModeChanged));

    public static readonly DependencyProperty FileNameProperty =
        DependencyProperty.Register(
            nameof(FileName),
            typeof(string),
            typeof(LottieView),
            new PropertyMetadata(null, OnFileNameChanged));

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.Register(
            nameof(IsPlaying),
            typeof(bool),
            typeof(LottieView),
            new PropertyMetadata(false, OnIsPlayingChanged));

    public static readonly DependencyProperty FpsProperty =
        DependencyProperty.Register(
            nameof(Fps),
            typeof(int),
            typeof(LottieView),
            new PropertyMetadata(30, OnFpsChanged));

    public static readonly DependencyProperty RepeatCountProperty =
        DependencyProperty.Register(
            nameof(RepeatCount),
            typeof(int),
            typeof(LottieView),
            new PropertyMetadata(-1, OnRepeatCountChanged));

    public static readonly DependencyProperty RepeatProperty =
        DependencyProperty.Register(
            nameof(Repeat),
            typeof(RepeatMode),
            typeof(LottieView),
            new PropertyMetadata(RepeatMode.Restart, OnRepeatChanged));

    /// <summary>
    /// 渲染模式
    /// </summary>
    public RenderMode RenderMode
    {
        get => (RenderMode)GetValue(RenderModeProperty);
        set => SetValue(RenderModeProperty, value);
    }

    /// <summary>
    /// 动画文件路径
    /// </summary>
    public string? FileName
    {
        get => (string?)GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }

    /// <summary>
    /// 是否正在播放
    /// </summary>
    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    /// <summary>
    /// 帧率（FPS）
    /// </summary>
    public int Fps
    {
        get => (int)GetValue(FpsProperty);
        set => SetValue(FpsProperty, value);
    }

    /// <summary>
    /// 重复次数（-1 表示无限循环）
    /// </summary>
    public int RepeatCount
    {
        get => (int)GetValue(RepeatCountProperty);
        set => SetValue(RepeatCountProperty, value);
    }

    /// <summary>
    /// 重复模式
    /// </summary>
    public RepeatMode Repeat
    {
        get => (RepeatMode)GetValue(RepeatProperty);
        set => SetValue(RepeatProperty, value);
    }

    #endregion

    #region 属性回调

    private static void OnRenderModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LottieView view)
        {
            view.UpdateControl();
        }
    }

    private static void OnFileNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LottieView view && view._activeControl != null)
        {
            SetPropertyValue(view._activeControl, "FileName", e.NewValue);
        }
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LottieView view && view._activeControl != null)
        {
            SetPropertyValue(view._activeControl, "IsPlaying", e.NewValue);
        }
    }

    private static void OnFpsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LottieView view && view._activeControl != null)
        {
            SetPropertyValue(view._activeControl, "Fps", e.NewValue);
        }
    }

    private static void OnRepeatCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LottieView view && view._activeControl != null)
        {
            SetPropertyValue(view._activeControl, "RepeatCount", e.NewValue);
        }
    }

    private static void OnRepeatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LottieView view && view._activeControl != null)
        {
            SetPropertyValue(view._activeControl, "Repeat", e.NewValue);
        }
    }

    private static void SetPropertyValue(FrameworkElement element, string propertyName, object? value)
    {
        var property = element.GetType().GetProperty(propertyName);
        property?.SetValue(element, value);
    }

    #endregion

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _contentHost = GetTemplateChild("PART_ContentHost") as ContentControl;
        UpdateControl();
    }

    /// <summary>
    /// 根据渲染模式更新内部控件
    /// </summary>
    private void UpdateControl()
    {
        if (_contentHost == null) return;

        // 创建新控件
        _activeControl = RenderMode switch
        {
            RenderMode.Dynamic => new DynamicLottieView(),
            RenderMode.Prerendered => new PrerenderedLottieView(),
            _ => new PrerenderedLottieView()
        };

        // 同步属性
        SetPropertyValue(_activeControl, "FileName", FileName);
        SetPropertyValue(_activeControl, "IsPlaying", IsPlaying);
        SetPropertyValue(_activeControl, "Fps", Fps);
        SetPropertyValue(_activeControl, "RepeatCount", RepeatCount);
        SetPropertyValue(_activeControl, "Repeat", Repeat);

        // 设置尺寸绑定
        _activeControl.SetBinding(WidthProperty, new System.Windows.Data.Binding(nameof(Width)) { Source = this });
        _activeControl.SetBinding(HeightProperty, new System.Windows.Data.Binding(nameof(Height)) { Source = this });

        _contentHost.Content = _activeControl;
    }
}

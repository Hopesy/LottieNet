using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using SkiaSharp;
using SkiaSharp.Skottie;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using LottieNet.Enums;

namespace LottieNet.Controls;

/// <summary>
/// 动态渲染 Lottie 动画控件
/// 使用 SKElement 实时渲染每一帧（类似 LottieSharp）
/// </summary>
public class DynamicLottieView : SKElement, IDisposable
{
    private readonly Stopwatch? _watch = new();
    private Animation? _animation;
    private DispatcherTimer? _timer;
    private int _loopCount;
    private bool _disposedValue;

    #region 依赖属性

    public static readonly DependencyProperty FileNameProperty =
        DependencyProperty.Register(
            nameof(FileName),
            typeof(string),
            typeof(DynamicLottieView),
            new PropertyMetadata(null, OnFileNameChanged));

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.Register(
            nameof(IsPlaying),
            typeof(bool),
            typeof(DynamicLottieView),
            new PropertyMetadata(false, OnIsPlayingChanged));

    public static readonly DependencyProperty RepeatCountProperty =
        DependencyProperty.Register(
            nameof(RepeatCount),
            typeof(int),
            typeof(DynamicLottieView),
            new PropertyMetadata(-1, OnRepeatCountChanged));

    public static readonly DependencyProperty RepeatProperty =
        DependencyProperty.Register(
            nameof(Repeat),
            typeof(RepeatMode),
            typeof(DynamicLottieView),
            new PropertyMetadata(RepeatMode.Restart));

    public static readonly DependencyProperty FpsProperty =
        DependencyProperty.Register(
            nameof(Fps),
            typeof(int),
            typeof(DynamicLottieView),
            new PropertyMetadata(30)); // 默认30fps降低CPU占用

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

    /// <summary>
    /// 帧率（FPS）
    /// </summary>
    public int Fps
    {
        get => (int)GetValue(FpsProperty);
        set => SetValue(FpsProperty, value);
    }

    #endregion

    #region 属性回调

    private static void OnFileNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicLottieView view && e.NewValue is string fileName)
        {
            view.LoadAnimation(fileName);
        }
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicLottieView view)
        {
            if ((bool)e.NewValue)
            {
                view.PlayAnimation();
            }
            else
            {
                view.StopAnimation();
            }
        }
    }

    private static void OnRepeatCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicLottieView view)
        {
            view._loopCount = (int)e.NewValue;
        }
    }

    #endregion

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == VisibilityProperty || e.Property == IsEnabledProperty || e.Property == IsVisibleProperty)
        {
            if (IsVisibleAndEnabled())
            {
                if (_animation != null && IsPlaying)
                {
                    PlayAnimation();
                }
            }
            else
            {
                StopAnimation();
            }
        }
    }

    private bool IsVisibleAndEnabled()
    {
        return Visibility == Visibility.Visible && IsEnabled && IsVisible;
    }

    /// <summary>
    /// 加载 Lottie 动画
    /// </summary>
    private void LoadAnimation(string fileName)
    {
        try
        {
            using var stream = File.OpenRead(fileName);
            using var skStream = new SKManagedStream(stream);

            if (!Animation.TryCreate(skStream, out _animation))
            {
                Debug.WriteLine($"[LottieNet] Failed to load animation from: {fileName}");
                return;
            }

            _animation.Seek(0);

            _watch?.Reset();
            if (_timer == null)
            {
                _timer = new DispatcherTimer(DispatcherPriority.Background);
                _timer.Interval = TimeSpan.FromSeconds(1.0 / Fps);
                _timer.Tick += (s, e) => { InvalidateVisual(); };
            }
            else
            {
                _timer.Stop();
                _timer.Interval = TimeSpan.FromSeconds(1.0 / Fps);
            }

            if (IsPlaying)
            {
                PlayAnimation();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LottieNet] Error loading animation: {ex.Message}");
        }
    }

    /// <summary>
    /// 开始播放动画
    /// </summary>
    public void PlayAnimation()
    {
        if (IsVisibleAndEnabled())
        {
            _timer?.Start();
            _watch?.Start();
            IsPlaying = true;
        }
        else
        {
            _timer?.Stop();
            _watch?.Stop();
        }
    }

    /// <summary>
    /// 停止播放动画
    /// </summary>
    public void StopAnimation()
    {
        _loopCount = RepeatCount;
        _timer?.Stop();
        _watch?.Reset();
        IsPlaying = false;
    }

    /// <summary>
    /// 渲染动画帧
    /// </summary>
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(SKColor.Empty);
        SKImageInfo info = e.Info;

        if (!IsVisibleAndEnabled())
        {
            StopAnimation();
            return;
        }

        if (_animation != null && _watch != null)
        {
            _animation.SeekFrameTime((float)_watch.Elapsed.TotalSeconds);

            if (_watch.Elapsed.TotalSeconds > _animation.Duration.TotalSeconds)
            {
                if (Repeat == RepeatMode.Restart)
                {
                    if (RepeatCount == -1) // 无限循环
                    {
                        _watch.Restart();
                    }
                    else if (RepeatCount > 0 && _loopCount > 0)
                    {
                        _loopCount--;
                        _watch.Restart();
                    }
                    else
                    {
                        StopAnimation();
                        return;
                    }
                }
            }

            _animation.Render(canvas, new SKRect(0, 0, info.Width, info.Height));
        }
    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _timer?.Stop();
                _watch?.Stop();
            }

            _timer = null;
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

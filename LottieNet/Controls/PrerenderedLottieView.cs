using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SkiaSharp;
using SkiaSharp.Skottie;
using LottieNet.Enums;

namespace LottieNet.Controls;

/// <summary>
/// 预渲染 Lottie 动画控件
/// 在加载时将所有帧预渲染为图片序列，播放时只切换图片，完全避免 SKElement 的 InvalidateVisual 问题
/// </summary>
public class PrerenderedLottieView : Control
{
    private Image? _imageControl;
    private DispatcherTimer? _timer;
    private List<BitmapSource>? _frames;
    private int _currentFrame = 0;
    private Animation? _animation;
    private bool _isLoaded = false;

    static PrerenderedLottieView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PrerenderedLottieView),
            new FrameworkPropertyMetadata(typeof(PrerenderedLottieView)));
    }

    #region 依赖属性

    public static readonly DependencyProperty FileNameProperty =
        DependencyProperty.Register(
            nameof(FileName),
            typeof(string),
            typeof(PrerenderedLottieView),
            new PropertyMetadata(null, OnFileNameChanged));

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.Register(
            nameof(IsPlaying),
            typeof(bool),
            typeof(PrerenderedLottieView),
            new PropertyMetadata(false, OnIsPlayingChanged));

    public static readonly DependencyProperty FpsProperty =
        DependencyProperty.Register(
            nameof(Fps),
            typeof(int),
            typeof(PrerenderedLottieView),
            new PropertyMetadata(30));

    public static readonly DependencyProperty RepeatCountProperty =
        DependencyProperty.Register(
            nameof(RepeatCount),
            typeof(int),
            typeof(PrerenderedLottieView),
            new PropertyMetadata(-1)); // -1 表示无限循环

    public static readonly DependencyProperty RepeatProperty =
        DependencyProperty.Register(
            nameof(Repeat),
            typeof(RepeatMode),
            typeof(PrerenderedLottieView),
            new PropertyMetadata(RepeatMode.Restart));

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

    private static void OnFileNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PrerenderedLottieView control && e.NewValue is string fileName)
        {
            if (control._isLoaded)
            {
                control.LoadAndPrerender(fileName);
            }
        }
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PrerenderedLottieView control)
        {
            if ((bool)e.NewValue)
            {
                control.StartPlaying();
            }
            else
            {
                control.StopPlaying();
            }
        }
    }

    #endregion

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        Console.WriteLine("[PrerenderedLottieView] OnApplyTemplate called");
        _imageControl = GetTemplateChild("PART_Image") as Image;
        Console.WriteLine($"[PrerenderedLottieView] _imageControl found: {_imageControl != null}");
        _isLoaded = true;

        Console.WriteLine($"[PrerenderedLottieView] FileName: {FileName}");
        if (!string.IsNullOrEmpty(FileName))
        {
            LoadAndPrerender(FileName);
        }
    }

    /// <summary>
    /// 加载并预渲染 Lottie 动画
    /// </summary>
    private void LoadAndPrerender(string fileName)
    {
        try
        {
            using var stream = File.OpenRead(fileName);
            using var skStream = new SKManagedStream(stream);

            if (!Animation.TryCreate(skStream, out _animation))
            {
                Console.WriteLine($"[LottieNet] Failed to load animation from: {fileName}");
                return;
            }
            Console.WriteLine($"[LottieNet] Animation loaded successfully from: {fileName}");

            // 预渲染所有帧
            var frameCount = (int)(_animation.Duration.TotalSeconds * Fps);
            _frames = new List<BitmapSource>(frameCount);

            var width = (int)Width;
            var height = (int)Height;

            if (width <= 0) width = 32;
            if (height <= 0) height = 32;

            Console.WriteLine($"[LottieNet] Prerendering {frameCount} frames at {width}x{height}...");

            for (int i = 0; i < frameCount; i++)
            {
                var frameTime = (float)i / Fps;
                var frame = RenderFrame(_animation, frameTime, width, height);
                if (frame != null)
                {
                    _frames.Add(frame);
                }
            }

            Console.WriteLine($"[LottieNet] Prerendering completed: {_frames.Count} frames");

            // 初始化定时器
            if (_timer == null)
            {
                _timer = new DispatcherTimer(DispatcherPriority.Background);
                _timer.Interval = TimeSpan.FromSeconds(1.0 / Fps);
                _timer.Tick += OnTimerTick;
            }
            else
            {
                _timer.Interval = TimeSpan.FromSeconds(1.0 / Fps);
            }

            // 显示第一帧
            if (_frames.Count > 0 && _imageControl != null)
            {
                _imageControl.Source = _frames[0];
                Console.WriteLine($"[LottieNet] First frame set to Image control");
            }

            if (IsPlaying)
            {
                Console.WriteLine($"[LottieNet] Starting playback (IsPlaying={IsPlaying})");
                StartPlaying();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LottieNet] Error loading Lottie: {ex.Message}");
            Console.WriteLine($"[LottieNet] Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// 渲染单帧到 BitmapSource
    /// </summary>
    private BitmapSource? RenderFrame(Animation animation, float timeInSeconds, int width, int height)
    {
        try
        {
            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;

            canvas.Clear(SKColor.Empty);
            animation.SeekFrameTime(timeInSeconds);
            animation.Render(canvas, new SKRect(0, 0, width, height));

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = data.AsStream();
            bitmap.EndInit();
            bitmap.Freeze(); // 冻结以便跨线程使用

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 定时器回调 - 切换到下一帧
    /// </summary>
    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_frames == null || _frames.Count == 0 || _imageControl == null)
        {
            return;
        }

        _currentFrame++;

        // 检查是否需要循环
        if (_currentFrame >= _frames.Count)
        {
            if (RepeatCount == -1) // 无限循环
            {
                _currentFrame = 0;
            }
            else if (RepeatCount > 0)
            {
                RepeatCount--;
                _currentFrame = 0;
            }
            else
            {
                StopPlaying();
                return;
            }
        }

        _imageControl.Source = _frames[_currentFrame];
    }

    /// <summary>
    /// 开始播放
    /// </summary>
    private void StartPlaying()
    {
        _timer?.Start();
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    private void StopPlaying()
    {
        _timer?.Stop();
        _currentFrame = 0;

        if (_frames != null && _frames.Count > 0 && _imageControl != null)
        {
            _imageControl.Source = _frames[0];
        }
    }
}

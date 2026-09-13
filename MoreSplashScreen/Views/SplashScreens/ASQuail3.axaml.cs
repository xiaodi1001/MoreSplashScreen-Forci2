using System.ComponentModel;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Abstractions.Views;
using ReactiveUI;
using Avalonia.Threading;

namespace MoreSplashScreen.Views.SplashScreens;

/// <summary>
/// ASQuail3.xaml 的交互逻辑
/// </summary>
public partial class ASQuail3 : SplashWindowBase
{
    public ISplashService SplashService { get; }
    public Plugin Plugin { get; }
    
    private double _lastProgress = 0.0;
    private IDisposable? _splashStatusObserver;

    private bool _canClose = false;

    public IImage? SplashImage { get; }

    public ASQuail3(ISplashService splashService, Plugin plugin)
    {
        SplashService = splashService;
        Plugin = plugin;
        SplashImage = GetSplashImage();
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var visualProgressBarFill = ElementComposition.GetElementVisual(ProgressBarFill);
        if (visualProgressBarFill != null)
        {
            visualProgressBarFill.Offset = visualProgressBarFill.Offset with { X = -640 };
            visualProgressBarFill.Opacity = 1.0f;
        }
    }

    private Bitmap GetBitmapFromUri(Uri uri)
    {
        using var stream = AssetLoader.Open(uri);
        var bitmap = new Bitmap(stream);
        return bitmap;
    }

private IImage? GetSplashImage()
    {
        try
        {
            return Plugin.Settings.IsCustomAndroidStudioSplashImageEnabled
                ? GetBitmapFromUri(new Uri("avares://MoreSplashScreen/Assets/AS-Quail-3/as.png"))
                : GetDefaultSplashImage();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private Bitmap GetDefaultSplashImage()
    {
        return GetBitmapFromUri(
            new Uri("avares://MoreSplashScreen/Assets/AS-Quail-3/as.png"));
    }


    private void SplashServiceOnProgressChanged(object? sender, double e)
    {
        _ = UpdateAnimationAsync(e);

    }

    private async Task UpdateAnimationAsync(double value, bool isFinal = false)
    {
        var visualProgressBarFill = ElementComposition.GetElementVisual(ProgressBarFill);
        if (visualProgressBarFill == null)
        {
            return;
        }
        
        var currentOffset = visualProgressBarFill.Offset;
        var compositor = visualProgressBarFill.Compositor;
        var progressAnimation = compositor.CreateVector3DKeyFrameAnimation();
        progressAnimation.InsertKeyFrame(1.0f, currentOffset with { X = -640 * (1 - value / 100) }, new CubicEaseOut());
        var duration = isFinal ? TimeSpan.FromSeconds(0.15) : TimeSpan.FromSeconds(Math.Max((value - _lastProgress ) / 8, 0.5));
        progressAnimation.Duration = duration;
        visualProgressBarFill.StartAnimation(nameof(visualProgressBarFill.Offset), progressAnimation);
        _lastProgress = value;

        await Task.Delay(duration);
    }
    

    private void AndroidStudioSplashScreen_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!_canClose)
        {
            e.Cancel = true;
        }
    }

    private void StartSplash()
    {
        SplashService.ProgressChanged += SplashServiceOnProgressChanged;

        _splashStatusObserver ??= SplashService.ObservableForProperty(x => x.SplashStatus)
            .Subscribe(_ => TryRunJobs());
    }

    public override async Task EndSplash()
    {
        SplashService.ProgressChanged -= SplashServiceOnProgressChanged;
        _splashStatusObserver?.Dispose();
        _splashStatusObserver = null;
        await UpdateAnimationAsync(100, true);
        _canClose = true;
        await base.EndSplash();
    }
}
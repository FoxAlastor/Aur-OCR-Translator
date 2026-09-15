using System.Windows;
using System.Windows.Input;
using OcrTranslator.Core;
using OcrTranslator.Models;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using static System.Windows.Media.ScaleTransform;

namespace OcrTranslator.UI;

public partial class ResultOverlayWindow : Window
{
    private readonly CaptureRegion _region;
    private readonly byte[] _image;
    private readonly IOcrProvider _ocr;
    private readonly ITranslationProvider _translator;
    private readonly ConfigService _config;
    private CancellationTokenSource? _cts;
    private bool _pinned;
    private double _zoom = 1.0;

    public ResultOverlayWindow(CaptureRegion region, byte[] image, IOcrProvider ocr, ITranslationProvider translator, ConfigService config)
    {
        InitializeComponent();
        _region = region; _image = image; _ocr = ocr; _translator = translator; _config = config;
        Opacity = config.Current.Opacity;
        Left = Math.Min(region.X + region.Width + 12, SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - Width - 8);
        Top = Math.Max(SystemParameters.VirtualScreenTop + 8, region.Y);
        Loaded += async (_, _) => await RunOcr();
    }

    private async Task RunOcr()
    {
        try
        {
            Status.Text = "Recognizing…";
            var result = await _ocr.RecognizeAsync(_image, _config.Current.SourceLanguage, CancellationToken.None);
            RecognizedText.Text = result.Text;
            Status.Text = string.IsNullOrWhiteSpace(result.Text) ? "Text not recognized" : "Translating…";
            await Translate();
        }
        catch (Exception ex) { Status.Text = ex.Message; }
    }

    private async Task Translate()
    {
        _cts?.Cancel(); _cts = new CancellationTokenSource();
        var text = RecognizedText.Text.Trim();
        if (text.Length == 0) { TranslatedText.Text = ""; return; }
        try
        {
            TranslatedText.Text = await _translator.TranslateAsync(text, _config.Current.SourceLanguage, _config.Current.TargetLanguage, _cts.Token);
            Status.Text = "Ready";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Status.Text = ex.Message; }
    }

    private async void TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        await Task.Delay(500);
        if (IsVisible) await Translate();
    }

    private async void ReTranslate(object sender, RoutedEventArgs e) => await Translate();

    private void Reselect(object sender, RoutedEventArgs e)
    {
        var selector = new RegionSelectorWindow();
        if (selector.ShowDialog() != true || !selector.Region.IsValid) return;
        var bytes = ((App)System.Windows.Application.Current).Capture.Capture(selector.Region);
        var result = new ResultOverlayWindow(selector.Region, bytes, _ocr, _translator, _config);
        result.Show(); Close();
    }

    private void Copy(object sender, RoutedEventArgs e) => CopyText(TranslatedText.Text, "Copied translation");

    private void CopyOcr(object sender, RoutedEventArgs e) => CopyText(RecognizedText.Text, "Copied OCR text");

    private void CopyText(string text, string successStatus)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                if (OpenClipboard(nint.Zero))
                {
                    try
                    {
                        EmptyClipboard();
                        var bytes = System.Text.Encoding.Unicode.GetBytes(text + "\0");
                        var handle = System.Runtime.InteropServices.Marshal.AllocHGlobal(bytes.Length);
                        System.Runtime.InteropServices.Marshal.Copy(bytes, 0, handle, bytes.Length);
                        if (SetClipboardData(13u, handle) != nint.Zero)
                        {
                            handle = nint.Zero;
                            Status.Text = successStatus;
                            return;
                        }
                    }
                    finally { CloseClipboard(); }
                }
            }
            catch { }
            System.Threading.Thread.Sleep(80);
        }
        Status.Text = "Clipboard is busy. Try Copy again.";
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(nint owner);
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool EmptyClipboard();
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern nint SetClipboardData(uint format, nint data);

    private void Pin(object sender, RoutedEventArgs e)
    {
        _pinned = !_pinned;
        PinButton.Content = _pinned ? "Unpin" : "Pin";
        PinButton.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_pinned ? "#496A9E" : "#303640"));
    }

    private void ZoomIn(object sender, RoutedEventArgs e) => SetZoom(_zoom + 0.1);
    private void ZoomOut(object sender, RoutedEventArgs e) => SetZoom(_zoom - 0.1);
    private void SetZoom(double value)
    {
        _zoom = Math.Clamp(Math.Round(value, 1), 0.8, 1.6);
        ContentScale.ScaleX = _zoom;
        ContentScale.ScaleY = _zoom;
        ZoomLabel.Text = $"{_zoom:P0}";
    }

    private void CloseWindow(object sender, RoutedEventArgs e) => Close();
    private void WindowKeyDown(object sender, WpfKeyEventArgs e) { if (e.Key == Key.Escape && !_pinned) Close(); }

    private void WindowPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed) return;
        if (FindParent<System.Windows.Controls.Button>(e.OriginalSource as DependencyObject) is not null) return;
        if (FindParent<System.Windows.Controls.TextBox>(e.OriginalSource as DependencyObject) is not null) return;
        try { DragMove(); } catch (InvalidOperationException) { }
    }

    private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child is not null)
        {
            if (child is T match) return match;
            child = System.Windows.Media.VisualTreeHelper.GetParent(child);
        }
        return null;
    }
}

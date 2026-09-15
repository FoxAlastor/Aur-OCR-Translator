using System.Net.Http;
using System.Windows;
using System.Windows.Interop;
using Forms = System.Windows.Forms;
using OcrTranslator.Core;
using OcrTranslator.UI;

namespace OcrTranslator;

public partial class App : System.Windows.Application
{
    public ConfigService Config { get; } = new();
    public ScreenCaptureService Capture { get; } = new();
    public WindowsOcrProvider Ocr { get; } = new();
    public ITranslationProvider Translator { get; private set; } = null!;
    private Forms.NotifyIcon? _tray;
    private MainWindow? _main;
    private const int HotkeyId = 701;
    private const int WmHotkey = 0x0312;
    [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool RegisterHotKey(nint hWnd, int id, uint modifiers, uint vk);
    [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool UnregisterHotKey(nint hWnd, int id);

    public App()
    {
        Config.Load();
        RefreshTranslator();
    }

    public void RefreshTranslator()
    {
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        Translator = string.Equals(Config.Current.TranslationProvider, "Google", StringComparison.OrdinalIgnoreCase)
            ? new GoogleTranslationProvider(http, Config.GetGoogleApiKey)
            : new DeepLTranslationProvider(http, Config.GetApiKey);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _main = new MainWindow();
        _main.Hide();
        _tray = new Forms.NotifyIcon { Icon = new System.Drawing.Icon("OCR Translate2.ico"), Visible = true, Text = "Aur OCR Translator" };
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Capture Now", null, (_, _) => CaptureNow());
        menu.Items.Add("Settings", null, (_, _) => new SettingsWindow(Config).ShowDialog());
        menu.Items.Add("Exit", null, (_, _) => Shutdown());
        _tray.ContextMenuStrip = menu;
        var source = HwndSource.FromHwnd(new WindowInteropHelper(_main).EnsureHandle());
        source.AddHook(WndProc);
        TryRegisterHotkey(source.Handle);
    }

    private void TryRegisterHotkey(nint handle)
    {
        if (!RegisterHotKey(handle, HotkeyId, 0x0002 | 0x0004 | 0x4000, 0x54))
            System.Windows.MessageBox.Show("Не вдалося зареєструвати Ctrl+Shift+T. Перевірте конфлікт у Settings.", "Aur OCR Translator");
    }
    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId) { CaptureNow(); handled = true; }
        return 0;
    }
    public void CaptureNow()
    {
        var selector = new RegionSelectorWindow();
        if (selector.ShowDialog() != true || !selector.Region.IsValid) return;
        var bytes = Capture.Capture(selector.Region);
        var result = new ResultOverlayWindow(selector.Region, bytes, Ocr, Translator, Config);
        result.Show();
    }
    protected override void OnExit(ExitEventArgs e)
    {
        if (_main is not null) UnregisterHotKey(new WindowInteropHelper(_main).Handle, HotkeyId);
        _tray?.Dispose(); Config.Save(); base.OnExit(e);
    }
}

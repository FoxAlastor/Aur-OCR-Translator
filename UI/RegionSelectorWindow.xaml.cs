using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using OcrTranslator.Models;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfPoint = System.Windows.Point;

namespace OcrTranslator.UI;
public partial class RegionSelectorWindow : Window
{
    public CaptureRegion Region { get; private set; }
    private WpfPoint _start; private bool _dragging;
    [System.Runtime.InteropServices.DllImport("user32.dll")] static extern int GetSystemMetrics(int nIndex);
    public RegionSelectorWindow()
    {
        InitializeComponent();
        Left = GetSystemMetrics(76); Top = GetSystemMetrics(77); Width = GetSystemMetrics(78); Height = GetSystemMetrics(79);
        Focusable = true; Loaded += (_, _) => Focus();
    }
    private void OnDown(object sender, WpfMouseButtonEventArgs e) { _start = e.GetPosition(this); _dragging = true; CaptureMouse(); Update(e.GetPosition(this)); }
    private void OnMove(object sender, WpfMouseEventArgs e) { if (_dragging) Update(e.GetPosition(this)); }
    private void OnUp(object sender, WpfMouseButtonEventArgs e) { if (!_dragging) return; _dragging = false; ReleaseMouseCapture(); var p = e.GetPosition(this); Update(p); if (Region.IsValid) { DialogResult = true; Close(); } }
    private void Update(WpfPoint current)
    {
        var x = Math.Min(_start.X, current.X); var y = Math.Min(_start.Y, current.Y); var w = Math.Abs(current.X - _start.X); var h = Math.Abs(current.Y - _start.Y);
        Canvas.SetLeft(Selection, x); Canvas.SetTop(Selection, y); Selection.Width = w; Selection.Height = h; Canvas.SetLeft(SizeLabel, x + 8); Canvas.SetTop(SizeLabel, Math.Max(0, y - 32)); SizeLabel.Text = $"{w:0} × {h:0}";
        Region = new CaptureRegion((int)(Left + x), (int)(Top + y), (int)w, (int)h);
    }
    private void OnKeyDown(object sender, WpfKeyEventArgs e)
    {
        if (e.Key != Key.Escape) return;
        if (IsMouseCaptured) ReleaseMouseCapture();
        DialogResult = false;
        e.Handled = true;
        Close();
    }
}

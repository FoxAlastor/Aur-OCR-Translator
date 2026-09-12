using System.Windows;
using OcrTranslator.Core;

namespace OcrTranslator.UI;
public partial class SettingsWindow : Window
{
    private readonly ConfigService _config;
    public SettingsWindow(ConfigService config)
    {
        InitializeComponent(); _config = config; Target.Text = config.Current.TargetLanguage; Source.Text = config.Current.SourceLanguage; OpacitySlider.Value = config.Current.Opacity;
    }
    private void Save(object sender, RoutedEventArgs e)
    {
        _config.Current.TargetLanguage = Target.Text.Trim().ToUpperInvariant(); _config.Current.SourceLanguage = Source.Text.Trim().ToUpperInvariant(); _config.Current.Opacity = OpacitySlider.Value;
        if (!string.IsNullOrWhiteSpace(ApiKey.Password)) _config.SetApiKey(ApiKey.Password); _config.Save(); DialogResult = true; Close();
    }
}

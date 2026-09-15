using System.Windows;
using OcrTranslator.Core;

namespace OcrTranslator.UI;

public partial class SettingsWindow : Window
{
    private readonly ConfigService _config;
    public SettingsWindow(ConfigService config)
    {
        InitializeComponent();
        _config = config;
        Provider.SelectedIndex = string.Equals(config.Current.TranslationProvider, "Google", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        Target.Text = config.Current.TargetLanguage;
        Source.Text = config.Current.SourceLanguage;
        OpacitySlider.Value = config.Current.Opacity;
    }

    private void Save(object sender, RoutedEventArgs e)
    {
        _config.Current.TranslationProvider = (Provider.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "DeepL";
        _config.Current.TargetLanguage = Target.Text.Trim().ToUpperInvariant();
        _config.Current.SourceLanguage = Source.Text.Trim().ToUpperInvariant();
        _config.Current.Opacity = OpacitySlider.Value;
        if (!string.IsNullOrWhiteSpace(DeepLKey.Password)) _config.SetApiKey(DeepLKey.Password);
        if (!string.IsNullOrWhiteSpace(GoogleKey.Password)) _config.SetGoogleApiKey(GoogleKey.Password);
        _config.Save();
        if (System.Windows.Application.Current is App app) app.RefreshTranslator();
        DialogResult = true;
        Close();
    }
}

# Aur OCR Translator

<img width="785" height="525" alt="image" src="https://github.com/user-attachments/assets/35695c01-4749-413c-97e1-38eb524bf7fd" />

Windows-first desktop OCR translator built with **C# / .NET 8 / WPF**. Select any region on the screen, recognize its text locally with Windows OCR, and translate the result in a lightweight floating window.

## Features

- Global `Ctrl+Shift+T` capture hotkey.
- Multi-monitor region selector with Escape cancellation.
- In-memory screen capture without saving screenshots by default.
- Windows OCR with Ukrainian, English, and Russian language-code mapping.
- Selectable DeepL or Google Cloud Translation provider with automatic source-language detection.
- Editable recognized text, `Copy OCR`, and `Re-Translate` for corrected text.
- Floating result window with copy, re-selection, pin/unpin, zoom, resizing, and dark theme.
- System tray menu for capture, settings, and exit.
- DPAPI-protected DeepL API key storage for the current Windows user.

## Requirements

- Windows 10/11.
- .NET 8 SDK.
- Windows SDK reference pack matching `10.0.26100.0` or a compatible newer SDK.
- Windows OCR language components for the languages you want to recognize.
- A DeepL API key for cloud translation.
- Or a Google Cloud Translation API key with the Cloud Translation API enabled.

## Build

From PowerShell in the repository directory:

```powershell
dotnet restore OcrTranslator.csproj
dotnet build OcrTranslator.csproj -c Release --platform x64
```

The executable is produced under:

```text
bin\x64\Release\net8.0-windows10.0.26100.0\
```

The repository includes a GitHub Actions workflow that restores and builds the project on `windows-latest` for pushes and pull requests.

## First run

1. Start `OcrTranslator.exe`.
2. Open the tray icon menu and select **Settings**.
3. Choose DeepL or Google and enter the corresponding API key. Use `UK` for Ukrainian, `EN` for English, or another supported language code.
4. Press `Ctrl+Shift+T`, select a screen region, and wait for OCR and translation. Press `Esc` to cancel selection.
5. Edit the recognized text if needed, then press **Re-Translate**.

The selected provider's API key is stored locally using Windows DPAPI with `CurrentUser` scope. Captured images are kept in memory and are not written to disk by default. Recognized text is sent to the selected provider when translation is requested.

## Windows OCR and packaging

`Windows.Media.Ocr` requires a desktop package identity. The current project is prepared for MSIX packaging, but the MSIX packaging project and signing configuration are intentionally not included yet. When running unpackaged, the application reports an OCR availability error if Windows cannot create the OCR engine.

## Current limitations

- MSIX packaging and installer signing remain deployment work.
- OCR language availability depends on Windows language components installed on the machine.
- Tesseract fallback, translation history, text-to-speech, and additional translation providers are not included in the current MVP.
- A live Windows build is required for final validation because WPF and Windows OCR cannot be compiled in a Linux environment.

## Privacy

The application does not save screen captures by default. The DeepL provider receives recognized text, not the original screenshot. Do not use cloud translation for sensitive material unless that data transfer is acceptable for your use case.

## Repository workflow

Create a feature branch, make the change, run the Windows build, and open a pull request. Do not commit API keys, certificates, build output, or local AppData files.

## License

This project is distributed under the MIT License. See [LICENSE](LICENSE).

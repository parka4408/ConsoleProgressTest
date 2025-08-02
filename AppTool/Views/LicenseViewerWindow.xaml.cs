using Markdig;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AppTool.Views
{
    /// <summary>
    /// ライセンス情報を表示するためのウィンドウ
    /// </summary>
    public sealed partial class LicenseViewerWindow : Window
    {
        public LicenseViewerWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;

            LoadLicenseMarkdownAsync();
        }

        private async void LoadLicenseMarkdownAsync()
        {
            string licensePath = Path.Combine(AppContext.BaseDirectory, "license.md");

            if (!File.Exists(licensePath))
            {
                await ShowErrorAsync("license.mdが見つかりません。");
                return;
            }

            try
            {
                string markdown = await File.ReadAllTextAsync(licensePath);
                
                // Markdownテーブル形式を修正
                markdown = FixMarkdownTableFormat(markdown);
                
                string html;
                try
                {
                    // 通常のパイプライン設定でテーブル表示を試行
                    var pipeline = new MarkdownPipelineBuilder()
                        .UsePipeTables()
                        .Build();
                    
                    html = Markdown.ToHtml(markdown, pipeline);
                }
                catch (Exception)
                {
                    // エラー時はプレーンテキストとして表示
                    html = $"<pre>{EscapeHtml(markdown)}</pre>";
                }

                string fullHtml = WrapHtml(html);

                await MarkdownWebView.EnsureCoreWebView2Async();
                MarkdownWebView.NavigateToString(fullHtml);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ShowErrorAsync(string message)
        {
            try
            {
                string escapedMessage = EscapeHtml(message);
                string html = WrapHtml($"<p style='color:red;'>{escapedMessage}</p>");
                await MarkdownWebView.EnsureCoreWebView2Async();
                MarkdownWebView.NavigateToString(html);
            }
            catch
            {
                // WebViewが利用できない場合のフォールバック
                var dialog = new ContentDialog
                {
                    Title = "エラー",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private static string EscapeHtml(string text)
        {
            return System.Net.WebUtility.HtmlEncode(text);
        }

        private static string FixMarkdownTableFormat(string markdown)
        {
            var lines = markdown.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            var result = new System.Text.StringBuilder();
            bool headerAdded = false;
            var tableRows = new System.Collections.Generic.List<string>();
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // タイトルをMarkdown見出しに変換
                if (trimmedLine.Contains("Analysis") && !trimmedLine.StartsWith("#"))
                {
                    result.AppendLine("# " + trimmedLine.Replace("...", ""));
                    continue;
                }

                // "References:" をサブ見出しに変換（最初の1回のみ）
                if (trimmedLine.Equals("References:") && !headerAdded)
                {
                    result.AppendLine();
                    result.AppendLine("## References");
                    result.AppendLine();
                    headerAdded = true;
                    continue;
                }
                
                // 2回目以降の"References:"はスキップ
                if (trimmedLine.Equals("References:") && headerAdded)
                {
                    continue;
                }

                // テーブル行を検出
                if (trimmedLine.Contains("|") && trimmedLine.Count(c => c == '|') >= 3)
                {
                    // テーブル区切り行やヘッダー行をスキップ
                    if (trimmedLine.All(c => c == '-' || c == '|' || char.IsWhiteSpace(c)) ||
                        (trimmedLine.Contains("Reference") && trimmedLine.Contains("Version") && trimmedLine.Contains("License")))
                    {
                        continue;
                    }

                    var parts = trimmedLine.Split('|').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray();
                    
                    if (parts.Length >= 4)
                    {
                        // テーブル行をリストに追加
                        tableRows.Add($"| {string.Join(" | ", parts)} |");
                    }
                    else if (parts.Length == 3)
                    {
                        // 3列の場合、License Type を空文字として4列にする
                        tableRows.Add($"| {parts[0]} | {parts[1]} | | {parts[2]} |");
                    }
                    continue;
                }
                else
                {
                    // テーブル以外の行で、空行でもテーブル区切り行でもない場合は出力
                    if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.All(c => c == '-' || c == '|' || char.IsWhiteSpace(c)))
                    {
                        result.AppendLine(line);
                    }
                    else if (string.IsNullOrWhiteSpace(trimmedLine) && !headerAdded)
                    {
                        // ヘッダー追加前の空行は保持
                        result.AppendLine(line);
                    }
                }
            }
            
            // テーブルが存在する場合、ヘッダーとデータ行を連続して出力
            if (tableRows.Count > 0 && headerAdded)
            {
                result.AppendLine("| Reference | Version | License Type | License |");
                result.AppendLine("|-----------|---------|--------------|---------|");
                foreach (var tableRow in tableRows)
                {
                    result.AppendLine(tableRow);
                }
            }
            
            return result.ToString();
        }

        private static string WrapHtml(string bodyContent)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: 'Segoe UI', sans-serif;
            padding: 20px;
            color: #333;
            line-height: 1.6;
            max-width: 100%;
            word-wrap: break-word;
        }}
        h1, h2, h3 {{
            border-bottom: 1px solid #eaecef;
            padding-bottom: 0.3em;
            margin-top: 24px;
            margin-bottom: 16px;
        }}
        pre {{
            background-color: #f6f8fa;
            padding: 16px;
            overflow-x: auto;
            border-radius: 6px;
            margin: 16px 0;
        }}
        code {{
            font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
            background-color: rgba(175, 184, 193, 0.2);
            padding: 2px 4px;
            border-radius: 3px;
        }}
        pre code {{
            background-color: transparent;
            padding: 0;
        }}
        a {{
            color: #0366d6;
            text-decoration: none;
        }}
        a:hover {{
            text-decoration: underline;
        }}
        blockquote {{
            padding: 0 1em;
            color: #6a737d;
            border-left: 0.25em solid #dfe2e5;
            margin: 0;
        }}
        table {{
            border-collapse: collapse;
            width: 100%;
            margin: 16px 0;
            border-spacing: 0;
            display: table;
        }}
        th, td {{
            border: 1px solid #dfe2e5;
            padding: 6px 13px;
            text-align: left;
            vertical-align: top;
        }}
        th {{
            background-color: #f6f8fa;
            font-weight: 600;
            border-bottom: 2px solid #dfe2e5;
        }}
        tbody tr:nth-child(even) {{
            background-color: #f8f9fa;
        }}
        tbody tr:hover {{
            background-color: #f1f8ff;
        }}

        /* ダークテーマ対応 */
        @media (prefers-color-scheme: dark) {{
            body {{
                background-color: #0d1117;
                color: #c9d1d9;
            }}
            h1, h2, h3 {{
                border-bottom-color: #21262d;
            }}
            pre {{
                background-color: #161b22;
            }}
            code {{
                background-color: rgba(110, 118, 129, 0.4);
            }}
            a {{
                color: #58a6ff;
            }}
            blockquote {{
                color: #8b949e;
                border-left-color: #30363d;
            }}
            th, td {{
                border-color: #30363d;
            }}
            th {{
                background-color: #161b22;
                border-bottom-color: #30363d;
            }}
            tbody tr:nth-child(even) {{
                background-color: #0d1117;
            }}
            tbody tr:hover {{
                background-color: #21262d;
            }}
        }}

        /* 高コントラストモード対応 */
        @media (prefers-contrast: high) {{
            body {{
                background-color: white;
                color: black;
            }}
            a {{
                color: #0000EE;
                text-decoration: underline;
            }}
            pre {{
                background-color: #f0f0f0;
                border: 1px solid #000;
            }}
        }}
    </style>
</head>
<body>
{bodyContent}
</body>
</html>";
        }
    }
}

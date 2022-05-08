using Microsoft.Web.WebView2.Core;
using GraphXrayDocCreator;
using GraphXrayDocCreator.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GraphXRayDocEditor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string AAD_PORTAL_URI = "https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade";
        private DocNavigator _docNavigator;
        private DocMap _currentDocMap;

        public MainWindow()
        {
            InitializeComponent();            
            Title = "Graph X-Ray";

            webView.Loaded += async (sender, e) =>
            {
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
                webView.CoreWebView2.Navigate(AAD_PORTAL_URI);
            };

        }

        private void CoreWebView2_SourceChanged(CoreWebView2 sender, CoreWebView2SourceChangedEventArgs args)
        {
            txtChromePortalUri.Text = webView.Source.ToString();
            PageChanged();
        }

        private async void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (webView != null && webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Navigate(txtChromePortalUri.Text);
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void btnOpenRepo_Click(object sender, RoutedEventArgs e)
        {
            PageChanged();
        }
        private void ShowError(string message)
        {
            lblErrorMessage.Visibility = Visibility.Visible;
            lblErrorMessage.Text = message;

        }
        private void ClearError()
        {
            lblErrorMessage.Visibility = Visibility.Collapsed;
        }
        private void PageChanged()
        {
            try
            {
                if (_docNavigator == null)
                {
                    _docNavigator = new DocNavigator(txtRepoPath.Text);
                }
                LoadPage(txtChromePortalUri.Text);

                ClearError();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }
        public void LoadPage(string chromePortalUri)
        {
            ClearContent();
            _currentDocMap = _docNavigator.GetDocMap(chromePortalUri);
            txtFileName.Text = _currentDocMap.Markdown;
            txtDocMapMarkdown.Text = _currentDocMap.MarkdownContent;
            txtDocMapPortalUri.Text = _currentDocMap.PortalUri;
            txtFileName.IsReadOnly = !string.IsNullOrEmpty(txtFileName.Text);
            txtDocMapPortalUri.IsReadOnly = !string.IsNullOrEmpty(txtFileName.Text);

            PreviewMarkdown();
        }

        private void ClearContent()
        {
            txtDocMapMarkdown.Text = string.Empty;
            txtDocMapPortalUri.Text = string.Empty;
            txtFileName.Text = string.Empty;

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _currentDocMap.Markdown = txtFileName.Text;
            _currentDocMap.MarkdownContent = txtDocMapMarkdown.Text;
            _currentDocMap.PortalUri = txtDocMapPortalUri.Text;

            _docNavigator.Save(_currentDocMap);

            PreviewMarkdown();
        }

        private void PreviewMarkdown()
        {
            webViewMarkdown.CoreWebView2.PostWebMessageAsString(_currentDocMap.MarkdownContent); //Refresh markdown
        }
    }
}

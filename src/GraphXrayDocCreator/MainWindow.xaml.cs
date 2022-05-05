using System;
using System.Windows;
using GraphXrayDocCreator;
using GraphXrayDocCreator.Model;
using Microsoft.Web.WebView2.Core;


namespace WPFSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DocNavigator _docNavigator;
        private DocMap _currentDocMap;

        public MainWindow()
        {
            InitializeComponent();
            webView.SourceChanged += WebView_SourceChanged;
            InitializeAsync();
        }

        private void WebView_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            txtChromePortalUri.Text = webView.Source.ToString();
            PageChanged();
        }

        async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(txtChromePortalUri.Text);
            }
        }

        private void btnOpenRepo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _docNavigator = new DocNavigator(txtRepoPath.Text);
                PageChanged();
                ClearError();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }
        private void ShowError(string message)
        {
            lblErrorMessage.Visibility = Visibility.Visible;
            lblErrorMessage.Content = message;

        }
        private void ClearError()
        {
            lblErrorMessage.Visibility = Visibility.Collapsed;
        }
        private void PageChanged()
        {
            if (_docNavigator == null) return;

            LoadPage(txtChromePortalUri.Text);
        }
        public void LoadPage(string chromePortalUri)
        {
            _currentDocMap = _docNavigator.GetDocMap(chromePortalUri);
            txtFileName.Text = _currentDocMap.Markdown;
            txtDocMapMarkdown.Text = _currentDocMap.MarkdownContent;
            txtDocMapPortalUri.Text = _currentDocMap.PortalUri;
            txtFileName.IsReadOnly = !string.IsNullOrEmpty(txtFileName.Text);
            txtDocMapPortalUri.IsReadOnly = !string.IsNullOrEmpty(txtFileName.Text);
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
            _currentDocMap.MarkdownContent= txtDocMapMarkdown.Text;
            _currentDocMap.PortalUri = txtDocMapPortalUri.Text;

            _docNavigator.Save(_currentDocMap);
        }
    }

}


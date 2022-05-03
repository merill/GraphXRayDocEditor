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
        private DocMapModelView _currentDocMapView;

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
            var docMap = _docNavigator.GetDocMap(chromePortalUri);
            if(docMap == null)
            {
                ClearContent();
            }
            else
            {
                _currentDocMapView = new DocMapModelView(docMap);
                txtDocMapMarkdown.Text = _currentDocMapView.MarkdownContent;
                txtDocMapPortalUri.Text = _currentDocMapView.PortalUri;
            }
        }

        private void ClearContent()
        {
            txtDocMapMarkdown.Text = string.Empty;
            txtDocMapPortalUri.Text = string.Empty;

        }
    }

}


using System;
using System.Windows;
using GraphXrayDocCreator;
using Microsoft.Web.WebView2.Core;


namespace WPFSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DocNavigator _docNavigator;
        public MainWindow()
        {
            InitializeComponent();
            webView.SourceChanged += WebView_SourceChanged;
            InitializeAsync();
        }

        private void WebView_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            addressBar.Text = webView.Source.ToString();
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
                webView.CoreWebView2.Navigate(addressBar.Text);
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

            txtMarkdown.Text = _docNavigator.GetMarkdown(addressBar.Text);
        }
    }

}


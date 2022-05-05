namespace GraphXrayDocCreator.Model
{
    internal class DocMapModelView
    {
        private DocNavigator _docNavigator;

        public string FileName { get; set; }
        public string MarkdownContent { get; set; }
        public string PortalUri { get; set; }
    }
}

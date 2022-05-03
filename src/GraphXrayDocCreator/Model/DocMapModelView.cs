namespace GraphXrayDocCreator.Model
{
    internal class DocMapModelView
    {
        public DocMapModelView(DocMap docMap)
        {
            MarkdownContent = docMap.MarkdownContent;
            PortalUri = docMap.PortalUri;
        }

        public string MarkdownContent { get; set; }
        public string PortalUri { get; set; }
    }
}

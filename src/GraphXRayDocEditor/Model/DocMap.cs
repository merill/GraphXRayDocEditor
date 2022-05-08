using System.Text.Json.Serialization;

namespace GraphXrayDocCreator.Model
{
    public class DocMap
    {
        public string PortalUri { get; set; }
        public string Markdown { get; set; }

        [JsonIgnore]
        public string MarkdownContent { get; set; }
    }

}

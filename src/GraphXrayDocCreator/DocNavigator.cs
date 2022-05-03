using GraphXrayDocCreator.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GraphXrayDocCreator
{
    internal class DocNavigator
    {
        string _docRepoFolderPath;
        const string MapRelativeFilePath = @"src\doc\map.json";
        const string MarkdownDocRelativeFolderPath = @"public\doc";
        private SortedDictionary<string, DocMap> _docMapList;
        private string MapFilePath { get { return Path.Combine(_docRepoFolderPath, MapRelativeFilePath); } }

        public DocNavigator(string docRepoFolderPath)
        {
            _docRepoFolderPath = docRepoFolderPath;
            if (!Directory.Exists(_docRepoFolderPath))
            {
                throw new ArgumentException("Incorrect folder path.");
            }
            if (!File.Exists(MapFilePath))
            {
                throw new FileNotFoundException($"map.json was not found at {MapFilePath}");
            }

            _docMapList = LoadDocMapCollection();
        }

        private SortedDictionary<string, DocMap> LoadDocMapCollection()
        {
            var json = File.ReadAllText(MapFilePath);
            var docMaps = JsonSerializer.Deserialize<List<DocMap>>(json);
            var docMapList = new SortedDictionary<string, DocMap>();
            foreach (var docMap in docMaps)
            {
                docMapList.Add(docMap.PortalUri, docMap);
            }
            return docMapList;
        }

        private void SaveDocMapCollection(SortedDictionary<string, DocMap> docMaps)
        {
            var list = (from p in docMaps.Values select p).ToArray();

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(list, options);
            File.WriteAllText(MapFilePath, json);
        }

        public DocMap? GetDocMap(string chromePortalUri)
        {
            var portalUri = GetClearnUri(chromePortalUri);
            _docMapList.TryGetValue(portalUri, out DocMap docMap);
            //Read markdown file only if it has not been read before
            if (docMap != null)
            {
                docMap.MarkdownContent = GetMarkdownContent(docMap.Markdown);
            }
            return docMap;
        }

        private string GetMarkdownContent(string markdownFileName)
        {
            if (string.IsNullOrEmpty(markdownFileName)) { return null; }
            var markdownFilePath = Path.Combine(_docRepoFolderPath, MarkdownDocRelativeFolderPath, markdownFileName);
            var content = File.ReadAllText(markdownFilePath);
            return content;
        }

        private string GetClearnUri(string portalUri)
        {
            var cleanUri = Regex.Replace(
                  portalUri,
                  @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?",
                  @"[ObjectId]",
                  RegexOptions.IgnoreCase
            );
            return cleanUri;
        }
    }
}

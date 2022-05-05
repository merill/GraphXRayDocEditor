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

        public void Save(DocMap currentDocMap)
        {
            SaveDocMap(currentDocMap);
            SaveDocMapCollection(_docMapList);
        }

        private void SaveDocMap(DocMap currentDocMap)
        {
            var filePath = GetMarkdownFullFilePath(currentDocMap.Markdown);
            File.WriteAllText(filePath, currentDocMap.Markdown);
        }

        public void SaveDocMapCollection(SortedDictionary<string, DocMap> docMaps)
        {
            var list = (from p in docMaps.Values select p).ToArray();

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(list, options);
            File.WriteAllText(MapFilePath, json);
        }

        public DocMap GetDocMap(string chromePortalUri)
        {
            var portalUri = GetClearnUri(chromePortalUri);
            _docMapList.TryGetValue(portalUri, out DocMap docMap);
            //Read markdown file only if it has not been read before
            if (docMap == null) //Create a new page for saving
            {
                //Create a new empty file that will be daved to
                var generatedNewMarkdownFileName = GenerateNewMarkdownFileNameFromUri(portalUri);
                var markdownFullFilePath = GetMarkdownFullFilePath(generatedNewMarkdownFileName);
                if (File.Exists(generatedNewMarkdownFileName)){
                    throw new Exception($"File already exists at {generatedNewMarkdownFileName} but is not mapped to Uri);");
                }
                docMap = new DocMap()
                {
                    PortalUri = portalUri,
                    Markdown = generatedNewMarkdownFileName,
                    MarkdownContent = String.Format(@"---
portalUri: ""{0}""
---
", portalUri)
                };
            }
            else
            {
                docMap.MarkdownContent = GetMarkdownContent(docMap.Markdown);
            }
            return docMap;
        }

        private string GenerateNewMarkdownFileNameFromUri(string portalUri)
        {
            var markdownFileName = portalUri.Replace("https://portal.azure.com/#blade/", "").Replace("/", "-").Replace("?", "-");
            markdownFileName += ".md";

            return markdownFileName;

        }

        private string GetMarkdownContent(string markdownFileName)
        {
            if (string.IsNullOrEmpty(markdownFileName)) { return null; }
            var markdownFilePath = GetMarkdownFullFilePath(markdownFileName);
            var content = File.ReadAllText(markdownFilePath);
            return content;
        }
        private string GetMarkdownFullFilePath(string markdownFileName)
        {
            return Path.Combine(_docRepoFolderPath, MarkdownDocRelativeFolderPath, markdownFileName);
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

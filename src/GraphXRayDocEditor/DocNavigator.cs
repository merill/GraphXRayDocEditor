using GraphXrayDocCreator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GraphXrayDocCreator
{
    internal class DocNavigator
    {
        private readonly string _mapFilePath;
        private readonly string _markdownFolderPath;

        private SortedDictionary<string, DocMap> _docMapList;
        

        /// <summary>
        /// If isEditMode = true then (file will be saved back for check-in)
        ///     docFolderPath should be = Root of Repo folder (eg. C:\GitHub\SaveAsScriptHackathon)
        /// else
        ///     docFolderPath should be = folder \Data\GraphXRayReactApp
        /// </summary>
        /// <param name="isEditMode"></param>
        /// <param name="docFolderPath"></param>
        public DocNavigator(bool isEditMode, string docFolderPath)
        {
            if (isEditMode) //Use the original map.json in the editor
            {
                _markdownFolderPath = Path.Combine(docFolderPath, @"public\doc");
                _mapFilePath = Path.Combine(docFolderPath, @"src\doc\map.json");
            }
            else //This is the npm 'build' package folder
            {
                _markdownFolderPath = Path.Combine(docFolderPath, "doc");
                _mapFilePath = Path.Combine(docFolderPath, @"doc\map.json");
            }

            Init(docFolderPath);
        }


        private void Init(string docFolderPath)
        {
            if (!Directory.Exists(docFolderPath))
            {
                throw new ArgumentException("Incorrect folder path.");
            }
            if (!File.Exists(_mapFilePath))
            {
                throw new FileNotFoundException($"map.json was not found at {_mapFilePath}");
            }

            _docMapList = LoadDocMapCollection();
        }

        private SortedDictionary<string, DocMap> LoadDocMapCollection()
        {
            var json = File.ReadAllText(_mapFilePath);
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
            _docMapList[currentDocMap.PortalUri] = currentDocMap;
            SaveDocMapCollection(_docMapList);
        }

        private void SaveDocMap(DocMap currentDocMap)
        {
            var filePath = GetMarkdownFullFilePath(currentDocMap.Markdown);
            File.WriteAllText(filePath, currentDocMap.MarkdownContent);
        }

        public void SaveDocMapCollection(SortedDictionary<string, DocMap> docMaps)
        {
            var list = (from p in docMaps.Values select p).ToArray();

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(list, options);
            File.WriteAllText(_mapFilePath, json);
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
                if (File.Exists(generatedNewMarkdownFileName))
                {
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
            return Path.Combine(_markdownFolderPath, markdownFileName);
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

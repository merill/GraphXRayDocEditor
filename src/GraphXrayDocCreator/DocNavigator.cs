using GraphXrayDocCreator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GraphXrayDocCreator
{
    internal class DocNavigator
    {
        string _folderPath;
        const string MapRelativeFilePath = @"src\doc\map.json";
        private List<DocMap> _docMaps;
        private string MapFilePath { get { return Path.Combine(_folderPath, MapRelativeFilePath); } }

        public DocNavigator(string folderPath)
        {
            _folderPath = folderPath;
            if (!Directory.Exists(_folderPath))
            {
                throw new ArgumentException("Incorrect folder path.");
            }
            if (!File.Exists(MapFilePath))
            {
                throw new FileNotFoundException($"map.json was not found at {MapFilePath}");
            }

            LoadDocMap();
        }

        private void LoadDocMap()
        {
            var json = File.ReadAllText(MapFilePath);
            _docMaps = JsonSerializer.Deserialize<List<DocMap>>(json);
        }

        private void SaveDocMap()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_docMaps, options);
            File.WriteAllText(MapFilePath, json);
        }

        public string GetMarkdown(string portalUri)
        {
            var cleanUri = GetClearnUri(portalUri);
            return cleanUri;
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

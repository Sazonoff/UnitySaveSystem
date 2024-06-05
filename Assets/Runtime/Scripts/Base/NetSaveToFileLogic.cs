using System.IO;

namespace UnitySaveSystem.Saves
{
    public class NetSaveToFileLogic : ISaveToFileLogic
    {
        private ISaveSystemLogger logger;

        public void SetLogger(ISaveSystemLogger logger)
        {
            this.logger = logger;
        }

        public bool DirectoryExist(string pathToDirectory)
        {
            return Directory.Exists(pathToDirectory);
        }

        public void CreateDirectory(string pathToDirectory)
        {
            Directory.CreateDirectory(pathToDirectory);
        }

        public string ReadTextFromFile(string path)
        {
            return File.ReadAllText(path);
        }

        public bool IsFileExist(string path)
        {
            return File.Exists(path);
        }

        public void WriteContentToFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}
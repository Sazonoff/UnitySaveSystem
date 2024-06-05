namespace UnitySaveSystem.Saves
{
    public interface ISaveToFileLogic
    {
        void SetLogger(ISaveSystemLogger logger);
        bool DirectoryExist(string pathToDirectory);
        void CreateDirectory(string pathToDirectory);
        string ReadTextFromFile(string path);
        bool IsFileExist(string path);
        void WriteContentToFile(string path, string content);
    }
}
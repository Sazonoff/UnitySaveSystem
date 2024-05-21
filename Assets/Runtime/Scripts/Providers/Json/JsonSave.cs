using System;

namespace UnitySaveSystem.Saves.Json
{
    public class JsonSave
    {
        public Type Type { get; }
        public int Id { get; }
        public string Json { get; }
        public string SaveName { get; }
        public bool RequiresSaveNotificationToUser { get; }

        public JsonSave(Type type, int id, string json, string saveName, bool requiresSaveNotificationToUser)
        {
            Type = type;
            Id = id;
            Json = json;
            SaveName = saveName;
            RequiresSaveNotificationToUser = requiresSaveNotificationToUser;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Id);
        }
    }
}
using System;
using LiteDB;

namespace UnitySaveSystem.Saves.LiteDB
{
    public struct BsonSave
    {
        public Type Type { get; }
        public int Id { get; }
        public BsonDocument Bson { get; }
        public string SaveName { get; }
        public bool NotifyUserAboutSaving { get; }

        public BsonSave(Type type, int id, BsonDocument bson, string saveName, bool notifyUserAboutSaving)
        {
            Type = type;
            Id = id;
            Bson = bson;
            SaveName = saveName;
            NotifyUserAboutSaving = notifyUserAboutSaving;
        }
    }
}
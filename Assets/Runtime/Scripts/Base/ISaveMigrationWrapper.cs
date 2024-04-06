namespace UnitySaveSystem.Saves
{
    public interface ISaveMigrationWrapper
    {
        void InjectLogger(SaveSystemLogger logger);

        /// <summary>
        /// Gets last migration ID before saving a save
        /// </summary>
        /// <returns></returns>
        int GetCurrentMigrationId();
    }
}
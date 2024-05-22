namespace UnitySaveSystem.Saves
{
    public interface ISaveMigrationHandler<T> : ISaveMigrationWrapper where T : Save
    {
        /// <summary>
        /// Checks if recently loaded save requires a migration and do it
        /// </summary>
        /// <param name="save"></param>
        void TryMigrate(T save);

        /// <summary>
        /// Add new migration rule for type to handler.
        /// It's more efficient to add them in ID order by Ascending but not restricted
        /// </summary>
        /// <param name="migrationRule"></param>
        void AddMigrationRule(ISaveMigrationRule<T> migrationRule);
    }
}
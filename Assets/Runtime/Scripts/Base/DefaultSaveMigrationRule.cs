namespace UnitySaveSystem.Saves
{
    public abstract class DefaultSaveMigrationRule<T> : ISaveMigrationRule<T> where T : SaveFile
    {
        protected SaveSystemLogger Logger;
        public abstract int Id { get; }

        public void InjectLogger(SaveSystemLogger logger)
        {
            this.Logger = logger;
        }

        public bool IsSaveWasWrittenWithThatRule(T save)
        {
            return save.SavedMigrationId == Id;
        }

        public abstract void Migrate(T save);

        public int CompareTo(ISaveMigrationRule<T> other)
        {
            if (other == null) return 1;
            if (ReferenceEquals(this, other)) return 0;
            return this.Id.CompareTo(other.Id);
        }
    }
}
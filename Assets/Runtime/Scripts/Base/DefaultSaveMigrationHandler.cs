using System.Collections.Generic;
using System.Linq;

namespace UnitySaveSystem.Saves
{
    public class DefaultSaveMigrationHandler<T> : ISaveMigrationHandler<T> where T : SaveFile
    {
        private readonly List<ISaveMigrationRule<T>> migrationRules = new();
        private SaveSystemLogger logger;

        public void InjectLogger(SaveSystemLogger logger)
        {
            this.logger = logger;
            foreach (var rule in migrationRules)
            {
                rule.InjectLogger(logger);
            }
        }

        public void TryMigrate(T save)
        {
            if (migrationRules.Count == 0) return;
            if (migrationRules.Last().IsSaveWasWrittenWithThatRule(save))
            {
                logger.Log($"Save doesnt require migration {save.GetType().Name} {save.Id}", SaveSystemLogType.Debug);
                return;
            }

            bool executingMigrations = false;
            if (save.SavedMigrationId == -1)
            {
                //the save was created before any migration for type existed
                executingMigrations = true;
            }

            for (int i = 0; i < migrationRules.Count; i++)
            {
                if (!executingMigrations)
                {
                    if (migrationRules[i].IsSaveWasWrittenWithThatRule(save))
                    {
                        executingMigrations = true;
                    }
                }
                else
                {
                    logger.Log($"Migrating save {save.GetType().Name} {save.Id} to migration: {migrationRules[i].Id}",
                        SaveSystemLogType.Debug);
                    migrationRules[i].Migrate(save);
                }
            }

            save.SetDirty();
        }

        public void AddMigrationRule(ISaveMigrationRule<T> migrationRule)
        {
            if (migrationRules.Any(r => r.Id == migrationRule.Id))
            {
                logger.Log($"You are trying to add migration rule with already added ID {migrationRule.Id}",
                    SaveSystemLogType.ErrorsOnly);
                return;
            }

            if (logger != null)
            {
                migrationRule.InjectLogger(logger);
            }

            migrationRules.Add(migrationRule);
            migrationRules.Sort();
        }

        public int GetCurrentMigrationId()
        {
            if (migrationRules.Count > 0)
            {
                return migrationRules.Last().Id;
            }

            return -1;
        }
    }
}
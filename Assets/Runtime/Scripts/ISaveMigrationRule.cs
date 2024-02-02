﻿using System;

namespace UnitySaveSystem.Saves
{
    public interface ISaveMigrationRule<T>: IComparable<ISaveMigrationRule<T>> where T : SaveFile
    {
        int Id { get; }
        void InjectLogger(SaveSystemLogger logger);
        bool IsSaveWasWrittenWithThatRule(T save);
        void Migrate(T save);
    }
}
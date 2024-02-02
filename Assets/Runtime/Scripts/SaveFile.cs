﻿using System;

namespace UnitySaveSystem.Saves
{
    public abstract class SaveFile
    {
        public int Id { get; set; }
        public bool JustCreatedFlag { get; set; }
        public bool IsDirty { get; private set; }
        public int SavedMigrationId { get; set; } = -1;

        public event Action<SaveFile> SaveChanged = delegate { };

        public void SetDirty()
        {
            IsDirty = true;
            SaveChanged?.Invoke(this);
        }

        public void ResetDirty()
        {
            IsDirty = false;
        }

        public void ResetJustCreatedFlag()
        {
            JustCreatedFlag = false;
            SetDirty();
        }

        /// <summary>
        /// You can override this method to init some default state for new save
        /// </summary>
        public virtual void OnNewInstanceCreated()
        {
            JustCreatedFlag = true;
        }
    }
}
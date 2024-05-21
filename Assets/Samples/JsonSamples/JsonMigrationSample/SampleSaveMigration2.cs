namespace UnitySaveSystem.Saves.Samples.JsonMigrationSample.Migration
{
    public class SampleSaveMigration2 : DefaultSaveMigrationRule<SampleSave>
    {
        public override int Id => 2;

        public override void Migrate(SampleSave save)
        {
            save.SomeNumberAnother -= 100;
            save.SetDirty();
        }
    }
}
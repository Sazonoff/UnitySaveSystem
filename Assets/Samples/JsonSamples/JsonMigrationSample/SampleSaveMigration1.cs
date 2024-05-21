namespace UnitySaveSystem.Saves.Samples.JsonMigrationSample.Migration
{
    public class SampleSaveMigration1 : DefaultSaveMigrationRule<SampleSave>
    {
        public override int Id => 1;

        public override void Migrate(SampleSave save)
        {
            save.SomeNumberAnother = save.SomeNumber;
            save.SetDirty();
        }
    }
}
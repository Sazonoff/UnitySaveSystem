namespace UnitySaveSystem.Saves.Samples.LiteDBMigrationSample
{
    [Save("SampleSaveName")]
    public class SampleSave : Save
    {
        public int SomeNumber { get; set; }

        public SampleSave()
        {
        }
    }
}
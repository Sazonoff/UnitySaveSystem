namespace UnitySaveSystem.Saves.Samples.LiteDBMigrationSample
{
    [Save("SampleSaveName")]
    public class SampleSave : SaveFile
    {
        public int SomeNumber { get; set; }

        public SampleSave()
        {
        }
    }
}
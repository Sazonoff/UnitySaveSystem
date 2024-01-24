namespace UnitySaveSystem.Saves
{
    [Save("SaveExample")]
    public class SaveExample : SaveFile
    {
        public int SomeExampleValue { get; set; }
        public int SomeExampleAnotherValue { get; set; }
    }
}
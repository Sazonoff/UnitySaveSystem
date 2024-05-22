namespace UnitySaveSystem.Saves
{
    [Save("SaveExample")]
    public class SaveExample : Save
    {
        public int SomeExampleValue { get; set; }
        public int SomeExampleAnotherValue { get; set; }
    }
}
namespace SaG.SaveSystem
{
    public static class SaveSystemSingleton
    {
        public static ISaveSystem Instance { get; set; } = new SaveSystem();
    }
}
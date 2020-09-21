using Newtonsoft.Json.Linq;

namespace SaG.SaveSystem.GameStateManagement
{
    public interface ISaveable
    {
        string Id { get; }
        
        string Context { get; }

        JObject Save();

        void Load(JObject state);
    }
}
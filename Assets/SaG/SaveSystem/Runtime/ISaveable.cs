using Newtonsoft.Json.Linq;

namespace SaG.SaveSystem
{
    public interface ISaveable
    {
        string Id { get; }
        
        string Context { get; }

        JObject Save();

        void Load(JObject state);
    }
}
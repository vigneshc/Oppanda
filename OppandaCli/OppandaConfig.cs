using System;
using Newtonsoft.Json;

namespace OppandaCli
{
    public class OppandaConfig{
        public string StorageConnectionString { get; set; }
        public OppandaCoreLib.TwitterIntegration.TwitterConfig TwitterConfig { get; set; }

        public static OppandaConfig Deserialize(string json) => JsonConvert.DeserializeObject<OppandaConfig>(json);
    }
}
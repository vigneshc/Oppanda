using System;
using Newtonsoft.Json;

namespace Oppanda.AzureFunctions
{
    public class OppandaConfig{
        public string StorageConnectionString { get; set; }
        public OppandaCoreLib.TwitterIntegration.TwitterConfig TwitterConfig { get; set; }
        public int MaxRequestsPerMinute { get; set; }

        public string Web3ApiKey { get; set; }

        public static OppandaConfig Deserialize(string json) => JsonConvert.DeserializeObject<OppandaConfig>(json);
    }
}
using System;
using Newtonsoft.Json;

namespace OppandaCli
{
    public class OppandaConfig{
        public StorageDetails StorageInfo { get; set; }
        public OppandaCoreLib.TwitterConfig TwitterConfig { get; set; }

        public static OppandaConfig Deserialize(string json) => JsonConvert.DeserializeObject<OppandaConfig>(json);

        public class StorageDetails{
            public string Uri { get; set; }
            public string AccountName { get; set; }
            public string AccountKey { get; set; }
        }
    }
}
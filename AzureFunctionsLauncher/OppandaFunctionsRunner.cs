using System;
using System.Net.Http;
using OppandaCoreLib; 

namespace Oppanda.AzureFunctions
{
    public class OppandaFunctionsRunner
    {
        private static RpcExecutor rpcExecutor;
        private static object lockObject = new Object();
        public static void Initialize(){
            if(rpcExecutor != null){
                return;
            }
            lock(lockObject){
                if(rpcExecutor == null){
                    using(var client = new HttpClient())
                    {
                        // download settings file.
                        using(HttpResponseMessage response = client.GetAsync(Settings.SettingsFileUrl).Result)
                        {
                            response.EnsureSuccessStatusCode();
                            string configContent = response.Content.ReadAsStringAsync().Result;
                            var config = OppandaConfig.Deserialize(configContent);
                            rpcExecutor =  RpcExecutorFactory.GetRpcExecutorAsync(config).Result;
                        }
                    }
                }
            }
        }
        
        public static RpcExecutor Executor => rpcExecutor;
    }
}

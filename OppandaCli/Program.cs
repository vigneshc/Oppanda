using System;
using System.IO;
using System.Threading.Tasks;
using Oppanda.AzureFunctions;
using System.Collections.Generic;

namespace OppandaCli
{
    class Program
    {
        // used for easier local testing.
        static async Task Main(string[] args)
        {
            string configFileUrlFile = args[0];
            var configFileUrl = await File.ReadAllTextAsync(configFileUrlFile);
            System.Environment.SetEnvironmentVariable("SettingsFileUrl", configFileUrl);
            OppandaFunctionsRunner.Initialize();
            var executor = OppandaFunctionsRunner.Executor;
            Dictionary<string, string> queryParameters = new Dictionary<string, string>(){
                { "type", "jsonrpc" }
            };

            while(true){
                Console.Write("Input RPC File: ");
                var fileName = Console.ReadLine();
                var rpcContent = await File.ReadAllTextAsync(fileName);
                var result = await executor.ExecuteAsync(queryParameters, rpcContent);
                Console.WriteLine("Result");
                Console.WriteLine(result.Item1);
                Console.WriteLine(result.Item2);
                Console.WriteLine();
            }
        }
    }
}

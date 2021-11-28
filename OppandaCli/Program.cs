using System;
using System.IO;
using System.Threading.Tasks;
using Oppanda.AzureFunctions;

namespace OppandaCli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string configFile = args[0];
            var executor = await RpcExecutorFactory.GetRpcExecutorAsync(OppandaConfig.Deserialize(await File.ReadAllTextAsync(configFile)));

            while(true){
                Console.Write("Input RPC File: ");
                var fileName = Console.ReadLine();
                var rpcContent = await File.ReadAllTextAsync(fileName);
                var result = await executor.ExecuteAsync(rpcContent);
                Console.WriteLine("Result");
                Console.WriteLine(result.Item1);
                Console.WriteLine(result.Item2);
                Console.WriteLine();
            }
        }
    }
}

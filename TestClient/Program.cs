using Neo4jClient;
using Neo4jClient.ReturnPoly;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace TestClient
{
    public class TypeA
    {
        public string PropertyA { get; set; }
    }

    public class TypeB : TypeA
    {
        public string PropertyB { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new BoltGraphClient("bolt://localhost:7687");
            await client.ConnectAsync();

            client.JsonConverters.Add(new PolymorphicJsonLabelConverter<TypeA>());

            var results = await client.Cypher
                .Match("(n)")
                .ReturnPolymorphic<TypeA>("n")
                .ResultsAsync;

            Console.Write(JsonConvert.SerializeObject(results, Formatting.Indented));
        }
    }
}
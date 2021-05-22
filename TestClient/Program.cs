using Neo4jClient;
using Neo4jClient.ReturnPoly;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using static Neo4jClient.ReturnPoly.PolyUtils;

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

            Console.WriteLine("ReturnPolymorphic<TypeA>");
            var results = await client.Cypher
                .Match("(n)")
                .ReturnPolymorphic<TypeA>("n")
                .ResultsAsync;
            foreach (var result in results)
                Print(result);

            Console.WriteLine("As<TypeA>");
            var results2 = await client.Cypher
                .Match("(n)")
                .Return(() => new
                {
                    Data = Return.As<TypeA>(Poly("n"))
                })
                .ResultsAsync;
            foreach (var result in results2)
                Print(result.Data);

        }

        static void Print(TypeA type)
        {
            Console.WriteLine(type.GetType().Name);
            Console.WriteLine("  A: " + type.PropertyA);
            if (type is TypeB b)
                Console.WriteLine("  B: " + b.PropertyB);
        }
    }
}
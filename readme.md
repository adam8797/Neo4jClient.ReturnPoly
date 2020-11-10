# Neo4jClient.ReturnPoly

A polymorphic return function for Neo4jClient

This library adds two extension methods, and some JsonConverters that allow you to return polymorphic entities from a Neo4j database.



There are two ways of using this library:

1. Label based polymorphism
2. Lambda based polymorphism



## Label Converter

```c#
/* Database

	(:TypeA { PropertyA: "PropA" })
	(:TypeA:TypeB { PropertyA: "PropA", PropertyB: "PropB" })
	(:TypeB { PropertyA: "PropA", PropertyB: "PropB" })

*/

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
```

### Available Functions

In order to use labels to determine the proper type, you must use one of these two return functions.

#### `ICypherFluentQuery<T> ReturnPolymorphic<T>(this ICypherFluentQuery, string)`

#### `ICypherFluentQuery<T> ReturnDistinctPolymorphic<T>(this ICypherFluentQuery, string)`



## Lambda Converter

```c#
/* Database

	Tags dont matter here
	({ PropertyA: "PropA" })
	({ PropertyA: "PropA", PropertyB: "PropB" })
	({ PropertyA: "PropA", PropertyB: "PropB" })

*/

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

        // Since this converter doesn't use labels, you can use the regular .Return<T> functions
        // from Neo4jClient
        client.JsonConverters.Add(new PolymorphicJsonLambdaConverter<TypeA>(jo => {
        	if (jo.ContainsKey("PropertyB"))
                return typeof(TypeB);
            return typeof(TypeA);
        }));

        var results = await client.Cypher
            .Match("(n)")
            .Return<TypeA>("n")
            .ResultsAsync;

        Console.Write(JsonConvert.SerializeObject(results, Formatting.Indented));
    }
}
```

There are no extension methods with this style. Just use Return like you normally would
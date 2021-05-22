# Neo4jClient.ReturnPoly

A polymorphic return function for Neo4jClient

This library adds two extension methods, and some JsonConverters that allow you to return polymorphic entities from a Neo4j database.



There are two ways of using this library:

1. Label based polymorphism
2. Lambda based polymorphism



## Getting Values

There are three primary snippets you can use to retrieve the polymorphic values from the graph

```c#
// Your standard way of doing things:
await graph.Cypher
            .Match("(n)")
            .ReturnPolymorphic<BaseType>("n")
            .ResultsAsync;

// Your standard way of doing things, but distinctly:
await graph.Cypher
            .Match("(n)")
            .ReturnDistinctPolymorphic<BaseType>("n")
            .ResultsAsync;

// The anonymous object/built return object. This method is a little more clunky, but
// there's not much to be done since the core Neo4jClient expression parser handles
// all of this
await graph.Cypher
            .Match("(n)")
            .Return(() => new {
                MyVariable = Return.As<BaseType>(PolyUtils.Poly("n"))
            })
            .ResultsAsync;

```



## Setting up the converters

### Label Converter

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

        foreach (var result in results)
            Print(result);
    }
    
    static void Print(TypeA type)
    {
        Console.WriteLine(type.GetType().Name);
        Console.WriteLine("  A: " + type.PropertyA);
        if (type is TypeB b)
            Console.WriteLine("  B: " + b.PropertyB);
    }
}
```

#### Available Functions

In order to use labels to determine the proper type, you must use one of these two return functions.

#### `ICypherFluentQuery<T> ReturnPolymorphic<T>(this ICypherFluentQuery, string)`

#### `ICypherFluentQuery<T> ReturnDistinctPolymorphic<T>(this ICypherFluentQuery, string)`

Or if using a built return object, the `PolyUtils.Poly(string)` method can be used to create the return text. It must be used in conjunction with `Return.As<T>` like so:

#### `Return.As<T>(PolyUtils.Poly(string))`

If you use it a log, add `using static Neo4jClient.ReturnPoly.PolyUtils;` to your imports, and then its just 

#### `Return.As<T>(Poly(string))`

And doesn't that just read a little better? (working with what I've got here, as I can't hook into the core expression parsing (yet))

### Lambda Converter

```c#
/* Database

	Labels dont matter here
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
        
        foreach (var result in results)
            Print(result);
    }
    
    static void Print(TypeA type)
    {
        Console.WriteLine(type.GetType().Name);
        Console.WriteLine("  A: " + type.PropertyA);
        if (type is TypeB b)
            Console.WriteLine("  B: " + b.PropertyB);
    }
}
```

There are no extension methods with this style. Just use Return like you normally would
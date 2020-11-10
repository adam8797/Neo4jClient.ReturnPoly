using Newtonsoft.Json;
using System.Text;

namespace Neo4JClient.ReturnPoly
{
    public abstract class ReadOnlyJsonConverter<T> : JsonConverter<T>
    {
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) { }
    }
}

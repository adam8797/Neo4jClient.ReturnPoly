using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Neo4jClient.ReturnPoly
{
    public class PolymorphicJsonLambdaConverter<T> : ReadOnlyJsonConverter<T>
    {
        private readonly Func<JObject, Type> _determineType;

        public PolymorphicJsonLambdaConverter(Func<JObject, Type> determineType)
        {
            _determineType = determineType;
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            if (jo.ContainsKey("data") && jo.Count == 1)
                jo = (JObject)jo["data"];

            if (jo == null || jo.Type == JTokenType.Null)
                return default;

            var type = _determineType(jo);
            return (T)jo.ToObject(type);
        }
    }
}

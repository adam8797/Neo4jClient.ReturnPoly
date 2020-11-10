using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.ReturnPoly
{
    public class PolymorphicJsonLabelConverter<T> : ReadOnlyJsonConverter<T>
    {
        private readonly bool _cachePotentialTypes;
        private readonly bool _exportedTypesOnly;
        private Type[] _potentialTypeCache;

        public PolymorphicJsonLabelConverter(bool cachePotentialTypes = true, bool exportedTypesOnly = true)
        {
            _potentialTypeCache = null;
            _cachePotentialTypes = cachePotentialTypes;
            _exportedTypesOnly = exportedTypesOnly;
        }

        public PolymorphicJsonLabelConverter(params Type[] typeCache)
        {
            _potentialTypeCache = typeCache;
        }

        private Type[] PotentialTypes()
        {
            if (_potentialTypeCache != null)
                return _potentialTypeCache;

            var baseTypeSet = _exportedTypesOnly ? typeof(T).Assembly.GetExportedTypes() : typeof(T).Assembly.GetTypes();

            var potential = baseTypeSet.Where(x => typeof(T).IsAssignableFrom(x)).ToArray();

            if (_cachePotentialTypes)
                _potentialTypeCache = potential;

            return potential;
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            if (jo.ContainsKey("Labels") && jo.ContainsKey("Node") && jo.Count == 2)
            {
                var labels = jo["Labels"].ToObject<List<string>>();
                var node = (JObject)jo["Node"];
                var data = (JObject)node["data"];

                labels.Remove(typeof(T).Name);

                if (labels.Count == 0)
                    return data.ToObject<T>();

                if (labels.Count == 1)
                {
                    var chosenType = PotentialTypes().Single(x => x.Name == labels[0]);
                    return (T)data.ToObject(chosenType);
                }

                return (T)data.ToObject(objectType);
            }
            else
            {
                var data = (JObject)jo["data"];
                return (T)data.ToObject(objectType);
            }
        }
    }
}

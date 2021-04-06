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
        private Dictionary<string, Type>? _potentialTypeCache;

        public PolymorphicJsonLabelConverter(bool cachePotentialTypes = true, bool exportedTypesOnly = true)
        {
            _potentialTypeCache = null;
            _cachePotentialTypes = cachePotentialTypes;
            _exportedTypesOnly = exportedTypesOnly;
        }

        public PolymorphicJsonLabelConverter(params Type[] typeCache)
        {
            _potentialTypeCache = typeCache.ToDictionary(x => x.Name, x => x);
        }

        private IDictionary<string, Type> PotentialTypes()
        {
            if (_potentialTypeCache != null)
                return _potentialTypeCache;

            var baseTypeSet = _exportedTypesOnly ? typeof(T).Assembly.GetExportedTypes() : typeof(T).Assembly.GetTypes();

            var potential = baseTypeSet
                .Where(x => typeof(T).IsAssignableFrom(x)) // public class x : T {}
                .Where(x => !x.IsInterface && !x.IsAbstract) // Concrete types only, we can't produce abstract classes or interfaces
                .ToDictionary(x => x.Name, x => x);

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

                var labelMap = PotentialTypes();

                // No label to look up, try our best
                if (labels.Count == 0)
                    return data.ToObject<T>();

                // If we have one label, this is a single pass
                // otherwise, sort the labels so its deterministic,
                // and take the first valid type we find
                foreach (var label in labels)
                {
                    if (labelMap.ContainsKey(label))
                    {
                        var chosenType = labelMap[label];
                        return (T) data.ToObject(chosenType);
                    }
                }

                // Something went quite wrong. Do our best
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

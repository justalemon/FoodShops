using GTA.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace FoodShops.Converters
{
    /// <summary>
    /// Converts a Vector3 to JSON and vice versa.
    /// </summary>
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject @object = JObject.Load(reader);
            int x = @object.ContainsKey("x") ? (int)@object["x"] : 0;
            int y = @object.ContainsKey("y") ? (int)@object["y"] : 0;
            int z = @object.ContainsKey("z") ? (int)@object["z"] : 0;
            return new Vector3(x, y, z);
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            JObject @object = new JObject
            {
                ["x"] = value.X,
                ["y"] = value.Y,
                ["z"] = value.Z
            };
            @object.WriteTo(writer);
        }
    }
}

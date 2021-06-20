using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using BangumiData.Models;

namespace BangumiData.JsonConverters
{
    public class BroadcastConverter : JsonConverter<Broadcast?>
    {
        public override Broadcast? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(Broadcast?));
            return Broadcast.TryParse(reader.GetString(), out var val) ? val : null;
        }

        public override void Write(Utf8JsonWriter writer, Broadcast? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }
    }
}

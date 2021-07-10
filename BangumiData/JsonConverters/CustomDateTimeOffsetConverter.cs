using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using BangumiData.Models;

namespace BangumiData.JsonConverters
{
    public class CustomDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTimeOffset?));
            return DateTimeOffset.TryParse(reader.GetString(), out var val) ? val : null;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString(RootObject.DateTimeFormat));
        }
    }
}

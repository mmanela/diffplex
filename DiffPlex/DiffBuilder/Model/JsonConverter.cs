#if !NET_TOO_OLD_VER
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiffPlex.DiffBuilder.Model;

internal class JsonDiffPieceConverter : JsonConverter<DiffPiece>
{
    /// <inheritdoc />
    public override DiffPiece Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
            case JsonTokenType.False:
                return default;
            case JsonTokenType.StartObject:
                var json = JsonElement.ParseValue(ref reader);
                var sub = ReadList(json, "sub");
                return Read(json, sub);
            default:
                throw new JsonException($"The token type is {reader.TokenType} but expect JSON object.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DiffPiece value, JsonSerializerOptions options)
    {
        Write(value, writer, options);
    }

    private static string GetString(JsonElement json, string property)
    {
        if (!json.TryGetProperty(property, out var prop)) return null;
        if (prop.ValueKind == JsonValueKind.String) return prop.GetString();
        if (prop.ValueKind == JsonValueKind.Number) return prop.GetDouble().ToString();
        return null;
    }

    private static int? GetInt32(JsonElement json, string property)
    {
        if (!json.TryGetProperty(property, out var prop)) return null;
        if (prop.TryGetInt32(out var i)) return i;
        return null;
    }

    private static ChangeType GetChangeType(JsonElement json, string property)
    {
        if (!json.TryGetProperty(property, out var prop)) return ChangeType.Imaginary;
        switch (prop.ValueKind)
        {
            case JsonValueKind.Null:
                return ChangeType.Unchanged;
            case JsonValueKind.False:
                return ChangeType.Deleted;
            case JsonValueKind.True:
                return ChangeType.Inserted;
            case JsonValueKind.String:
                var s = prop.GetString();
                if (string.IsNullOrWhiteSpace(s)) return ChangeType.Imaginary;
                return Enum.TryParse<ChangeType>(s, true, out var v) ? v : s.ToLowerInvariant() switch
                {
                    "+" or "add" or "insert" => ChangeType.Inserted,
                    "-" or "del" or "delete" => ChangeType.Deleted,
                    " " or "same" => ChangeType.Unchanged,
                    "m" or "mod" or "modify" => ChangeType.Modified,
                    _ => ChangeType.Imaginary
                };
            case JsonValueKind.Number:
                if (prop.TryGetInt32(out var i)) return (ChangeType)i;
                break;
        }

        return ChangeType.Imaginary;
    }

    private static DiffPiece Read(JsonElement json, IReadOnlyList<DiffPiece> sub)
    {
        if (json.ValueKind == JsonValueKind.Object) return new(GetString(json, "text"), GetChangeType(json, "type"), GetInt32(json, "position"), sub);
        if (json.ValueKind == JsonValueKind.Null || json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.False) return null;
        throw new JsonException($"Expect a JSON object");
    }

    internal static List<DiffPiece> ReadList(JsonElement json, string property)
    {
        if (!json.TryGetProperty(property, out var arr)) return null;
        return ReadList(arr);
    }

    internal static List<DiffPiece> ReadList(JsonElement arr)
    {
        if (arr.ValueKind != JsonValueKind.Array)
        {
            if (arr.ValueKind == JsonValueKind.Object) return ReadList(arr, "lines");
            return null;
        }

        var len = arr.GetArrayLength();
        var list = new List<DiffPiece>();
        for (var i = 0; i < len; i++)
        {
            var item = Read(arr[i], null);
            if (item != null) list.Add(item);
        }

        return list;
    }

    internal static void Write(DiffPiece value, Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        value.Write(writer, options);
    }

    internal static void Write(IEnumerable<DiffPiece> sub, Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        foreach (var item in sub)
        {
            Write(item, writer, options);
        }
    }
}

internal class JsonDiffPaneConverter : JsonConverter<DiffPaneModel>
{
    /// <inheritdoc />
    public override DiffPaneModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
            case JsonTokenType.False:
                return default;
            case JsonTokenType.StartArray:
                {
                    var arr = JsonElement.ParseValue(ref reader);
                    var list = JsonDiffPieceConverter.ReadList(arr);
                    return new(list);
                }
            case JsonTokenType.StartObject:
                {
                    var json = JsonElement.ParseValue(ref reader);
                    var list = JsonDiffPieceConverter.ReadList(json, "lines");
                    return new(list);
                }
            default:
                throw new JsonException($"The token type is {reader.TokenType} but expect JSON object.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DiffPaneModel value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        writer.WriteStartObject();
        writer.WriteStartArray("lines");
        JsonDiffPieceConverter.Write(value.Lines, writer, options);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}

internal class JsonSideBySideDiffConverter : JsonConverter<SideBySideDiffModel>
{
    /// <inheritdoc />
    public override SideBySideDiffModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return default;
            case JsonTokenType.StartArray:
                {
                    var arr = JsonElement.ParseValue(ref reader);
                    var list = JsonDiffPieceConverter.ReadList(arr);
                    return new(list, list);
                }
            case JsonTokenType.StartObject:
                {
                    var json = JsonElement.ParseValue(ref reader);
                    var oldText = JsonDiffPieceConverter.ReadList(json, "old");
                    var newText = JsonDiffPieceConverter.ReadList(json, "new");
                    return new(oldText, newText);
                }
            default:
                throw new JsonException($"The token type is {reader.TokenType} but expect JSON object.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SideBySideDiffModel value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        writer.WriteStartObject();
        writer.WriteStartArray("old");
        JsonDiffPieceConverter.Write(value.OldText.Lines, writer, options);
        writer.WriteEndArray();
        writer.WriteStartArray("new");
        JsonDiffPieceConverter.Write(value.NewText.Lines, writer, options);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
#endif
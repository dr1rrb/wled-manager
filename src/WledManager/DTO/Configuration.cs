using System.Text.Json.Serialization;

namespace WledManager.DTO;

internal record Configuration([property: JsonPropertyName("id")] Identity Identity, [property: JsonPropertyName("hw")] Hardware Hardware);

internal record Identity([property: JsonPropertyName("name")] string Name);

internal record Hardware([property: JsonPropertyName("led")] Leds Leds);

internal record Leds([property: JsonPropertyName("total")] int Total);
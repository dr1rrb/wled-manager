using System.Text.Json.Serialization;

namespace WledManager.DTO;

public record Configuration([property: JsonPropertyName("id")] Identity Identity, [property: JsonPropertyName("hw")] Hardware Hardware);

public record Identity([property: JsonPropertyName("name")] string Name);

public record Hardware([property: JsonPropertyName("led")] Leds Leds);

public record Leds([property: JsonPropertyName("total")] int Total);
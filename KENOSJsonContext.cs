using System.Text.Json.Serialization;
using KENOS.Bot.Serialization;

namespace KENOS.Bot.Serialization;

[JsonSerializable(typeof(JsonBinPayload))]
[JsonSerializable(typeof(ChangelogEntryDto))]
[JsonSerializable(typeof(InfoEntryDto))]
[JsonSerializable(typeof(List<long>))]          // для users.json в ChangelogService
[JsonSerializable(typeof(WebAppSupportPayload))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy   = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented          = false)]
public partial class KENOSJsonCtx : JsonSerializerContext { }

// ── DTO ───────────────────────────────────────────────────────

public sealed record ChangelogEntryDto(string Ver, string Date, string Text);

public sealed record InfoEntryDto(
    string Type, string Title, string Body, string Date, bool Active);

public sealed record JsonBinPayload(
    IEnumerable<ChangelogEntryDto> Changelog,
    IEnumerable<InfoEntryDto>      Info);

public sealed record WebAppSupportPayload(
    string Type, string Subject, string Message, long UserId);

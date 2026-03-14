using System.Text.Json.Serialization;
using KENOS.Bot.Models;
using KENOS.Bot.Services;

namespace KENOS.Bot.Serialization;

// ═══════════════════════════════════════════════════════════════
//  JSON Source Generator — НОЛЬ рефлексии при сериализации
//
//  Вместо JsonSerializer.Serialize(obj) → JsonSerializer.Serialize(obj, KENOSJsonCtx.Default.XXX)
//  AOT-совместимо, ~3x быстрее на первом вызове (нет DynamicCode)
// ═══════════════════════════════════════════════════════════════

[JsonSerializable(typeof(JsonBinPayload))]
[JsonSerializable(typeof(ChangelogEntryDto))]
[JsonSerializable(typeof(InfoEntryDto))]
[JsonSerializable(typeof(List<long>))]
[JsonSerializable(typeof(WebAppSupportPayload))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy        = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented               = false)]
public partial class KENOSJsonCtx : JsonSerializerContext { }

// ── DTO — плоские структуры без наследования (trim-safe) ──────

public sealed record ChangelogEntryDto(string Ver, string Date, string Text);

public sealed record InfoEntryDto(
    string Type, string Title, string Body, string Date, bool Active);

public sealed record JsonBinPayload(
    IEnumerable<ChangelogEntryDto> Changelog,
    IEnumerable<InfoEntryDto>      Info);

public sealed record WebAppSupportPayload(
    string Type, string Subject, string Message, long UserId);

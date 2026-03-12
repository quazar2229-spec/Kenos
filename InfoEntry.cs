namespace KENOS.Bot.Models;

/// <summary>
/// Объявление / тех-работа для раздела «Инфо» мини-приложения.
/// type: tech | info | ok | warn
/// </summary>
public sealed record InfoEntry(
    string Type,
    string Title,
    string Body,
    string Date,
    bool   Active = true);

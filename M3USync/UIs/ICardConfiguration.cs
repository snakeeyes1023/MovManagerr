﻿namespace M3USync.UIs
{
    public interface ICardConfiguration
    {
        string? TitleMap { get; set; }
        string? DescriptionMap { get; set; }
        Type Type { get; set; }
    }
}
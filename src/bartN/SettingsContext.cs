using System;
using System.IO;
using BartN.Domain;

namespace BartN;

public sealed class SettingsContext
{
    private SettingsContext(Settings settings)
    {
        Settings = settings;
    }

    public Settings Settings { get; }

    public static SettingsContext FromStream(Stream settingsStream)
    {
        Span<byte> size = stackalloc byte[4];
        settingsStream.ReadFull(size);

        return new SettingsContext(Settings.Parser.ParseFrom(settingsStream));
    }
}

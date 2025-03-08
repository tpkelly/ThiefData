using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace ThiefData.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private readonly ChatHandler chatHandler;
    private readonly SpreadsheetHandler spreadsheet;


    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, ChatHandler chat, SpreadsheetHandler sheet)
        : base("Debug Info", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        chatHandler = chat;
        this.plugin = plugin;
        spreadsheet = sheet;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // Do not use .Text() or any other formatted function like TextWrapped(), or SetTooltip().
        // These expect formatting parameter if any part of the text contains a "%", which we can't
        // provide through our bindings, leading to a Crash to Desktop.
        // Replacements can be found in the ImGuiHelpers Class

        var localPlayer = Plugin.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            ImGui.TextUnformatted("Our local player is currently not loaded.");
            return;
        }

        ImGui.TextUnformatted($"Found {spreadsheet.CurrentRow} rows");
        ImGui.TextUnformatted($"Latest room: {spreadsheet.CurrentRoom}");

        var territoryId = Plugin.ClientState.TerritoryType;
        if (territoryId != 725)
        {
            ImGui.TextUnformatted("Not currently in Hidden Canals.");
            return;
        }
        ImGui.TextUnformatted(chatHandler.LastMessage);
    }
}

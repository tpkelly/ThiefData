using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ThiefData.Windows;

namespace ThiefData;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/thief";

    public readonly WindowSystem WindowSystem = new("ThiefData");
    private MainWindow MainWindow { get; init; }
    private ChatHandler chatHandler { get; init; }

    public Plugin(IChatGui chat, IPartyList partylist)
    {
        /*
        // Fetch the spreadsheet
        var request = sheets.Spreadsheets.Get("1JbUrucVnqf4_z9DrOrzQmEYSyFraKdac7iTZho1AxjM");
        //var request = sheets.Spreadsheets.Values.Get("1JbUrucVnqf4_z9DrOrzQmEYSyFraKdac7iTZho1AxjM", "'Thief Maps'!A1:AF999");
        var response = request.Execute();
        var sheet = response.Sheets.First(x => x.Properties.Title == "Thief Maps");
        */

        chatHandler = new ChatHandler(partylist);
        MainWindow = new MainWindow(this, chatHandler);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Read chat log messages
        chat.ChatMessageUnhandled += chatHandler.ChatMessage;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [ThiefData] ===A cool log message from Sample Plugin===
        Log.Information($"=== {PluginInterface.Manifest.Name} finished loading ===");
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleMainUI() => MainWindow.Toggle();
}

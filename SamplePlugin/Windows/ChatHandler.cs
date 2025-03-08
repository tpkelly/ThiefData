using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using System;
using System.Linq;

namespace ThiefData.Windows
{
    public class ChatHandler
    {
        private readonly IPartyList partylist;
        private readonly ISpreadsheetHandler spreadsheet;
        public ChatHandler(IPartyList partyList, ISpreadsheetHandler sheet)
        {
            partylist = partyList;
            spreadsheet = sheet;
        }

        public void ChatMessage(XivChatType type, int timestamp, SeString sender, SeString message)
        {
            if (type != XivChatType.SystemMessage && (int)type < 200 && type != XivChatType.Echo) // Some special system messages have crazy high IDs
            {
                return;
            }

            var coord = "";

            switch (message.TextValue)
            {
                // Door numbers
                case "The Hidden Canals of Uznair has begun.": // New entry
                    spreadsheet.UpdateRoom(1);
                    break;
                case "The gate to the 2nd chamber opens.":
                    spreadsheet.UpdateRoom(2);
                    break;
                case "The gate to the 3rd chamber opens.":
                    spreadsheet.UpdateRoom(3);
                    break;
                case "The gate to the 4th chamber opens.":
                    spreadsheet.UpdateRoom(4);
                    break;
                case "The gate to the 5th chamber opens.":
                    spreadsheet.UpdateRoom(5);
                    break;
                case "The gate to the 6th chamber opens.":
                    spreadsheet.UpdateRoom(6);
                    break;
                case "The gate to the final chamber opens.":
                    spreadsheet.UpdateRoom(7);
                    break;

                // Door glow
                case "The gate on the left begins to glow.":
                case "The gate in the middle begins to glow.":
                case "The gate on the right begins to glow.":
                    spreadsheet.UpdateEvent("Glow");
                    break;

                // Left/Right/Mid
                case string direction when direction.Contains("hand on the gate"):
                    //TODO: Deal with "you"
                    var xCoord = partylist.FirstOrDefault(x => direction.StartsWith(x.Name.TextValue))?.Position.X
                        ?? Plugin.ClientState.LocalPlayer?.Position.X;
                    coord = xCoord switch
                    {
                        < -8f => "Left",
                        > 8f => "Right",
                        _ => "Mid"
                    };
                    spreadsheet.UpdateDoor(coord);
                    break;

                // Loot
                case string loot when loot.Contains("has been added to the loot list."):
                    spreadsheet.UpdateLoot(loot.Replace(" has been added to the loot list.", ""));
                    break;
                case "The Gambler's Lure activates!":
                    spreadsheet.UpdateEvent("Gamble");
                    break;

                // Adds
                case "Abharamu appears!":
                    spreadsheet.UpdateBonusMob("Abharamu");
                    break;
                case "The canal crew appear!":
                    spreadsheet.UpdateBonusMob("Mandragoras");
                    break;
                case "A Namazu stickywhisker appears!":
                    spreadsheet.UpdateBonusMob("Namazu");
                    break;

                default: return;
            }
        }
    }
}

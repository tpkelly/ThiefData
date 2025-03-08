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
        public ChatHandler(IPartyList partyList)
        {
            partylist = partyList;
        }

        public string LastMessage { get; private set; } = "";

        public void ChatMessage(XivChatType type, int timestamp, SeString sender, SeString message)
        {
            if (type != XivChatType.SystemMessage && (int)type < 200) // Some special system messages have crazy high IDs
            {
                return;
            }

            var coord = "";

            switch (message.TextValue)
            {
                // Door numbers
                case "The Hidden Canals of Uznair has begun.": // New entry
                    break;
                case "The gate to the 2nd chamber opens.":
                    break;
                case "The gate to the 3rd chamber opens.":
                    break;
                case "The gate to the 4th chamber opens.":
                    break;
                case "The gate to the 5th chamber opens.":
                    break;
                case "The gate to the 6th chamber opens.":
                    break;
                case "The gate to the final chamber opens.":
                    break;

                // Door glow
                case "The gate on the left begins to glow.":
                case "The gate in the middle begins to glow.":
                case "The gate on the right begins to glow.":
                    break;

                // Left/Right/Mid
                case string direction when direction.Contains("hand on the gate"):
                    var xCoord = partylist.First(x => direction.StartsWith(x.Name.TextValue)).Position.X;
                    coord = xCoord switch
                    {
                        < -8f => "Left",
                        > 8f => "Right",
                        _ => "Mid"
                    };
                    break;

                // Loot
                case string loot when loot.Contains("has been added to the loot list."):
                    break;
                case "The Gambler's Lure activates!":
                    break;

                // Adds
                case "Abharamu appears!":
                    break;
                case "The canal crew appear!":
                    break;
                case "A Namazu stickywhisker appears!":
                    break;

                default: return;
            }

            LastMessage = $"{message.TextValue}{Environment.NewLine}{coord}";
        }
    }
}

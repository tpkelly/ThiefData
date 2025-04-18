using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest;

namespace ThiefData
{
    public class SpreadsheetHandler : ISpreadsheetHandler
    {
        private const string SheetID = "";

        internal int CurrentRow { get; private set; }
        internal int CurrentRoom { get; private set; }

        private string lastEnemy = "";
        private string lastBonusMob = "";
        private string lastLoot = "";
        private string lastEvent = "";
        private string lastDoor = "";

        public SpreadsheetHandler()
        {
            var request = Service().Spreadsheets.Values.Get(SheetID, "'Thief Maps'!A1:AF999");
            var response = request.Execute();

            CurrentRow = response.Values.Count;
            CurrentRoom = int.TryParse(response.Values[CurrentRow-1][1].ToString(), out var room) ? room : 1;
        }

        private static SheetsService Service()
        {
            string[] scopes = [SheetsService.Scope.Spreadsheets];
            GoogleCredential credential;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ThiefData.credentials.json"))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
            }

            return new SheetsService(new BaseClientService.Initializer()
            {
                ApplicationName = "ThiefData",
                HttpClientInitializer = credential,
                HttpClientTimeout = TimeSpan.FromSeconds(5),
            });
        }

        public void UpdateRoom(int room)
        {
            var range = new ValueRange { Values = [[room]], Range = $"'Thief Maps'!B{CurrentRow}" };
            if (room == 1)
            {
                CurrentRow++;
                range = new ValueRange { Values = [[DateTime.Today.ToString("M/d/yyyy"), room]], Range = $"'Thief Maps'!A{CurrentRow}:B{CurrentRow}" };
            }
            else if (lastDoor == "")
            {
                UpdateDoor(Plugin.ClientState.LocalPlayer.Position.X);
            }

            // Reset the room
            CurrentRoom = room;
            lastEnemy = "";
            lastBonusMob = "";
            lastLoot = "";
            lastEvent = "";
            lastDoor = "";

            // Update the sheet
            var request = Service().Spreadsheets.Values.Update(range, SheetID, range.Range);
            request.ValueInputOption = ValueInputOptionEnum.USERENTERED;
            request.ExecuteAsync();
        }

        private static readonly string[] MobColumns = ["C", "H", "M", "R", "W", "AB"];
        private static readonly IDictionary<string, string> MobGroupNames = new Dictionary<string, string>
        {
            { "canal fluturini", "Fluturini" },
            { "canal halgai", "Halgai" },
            { "canal mossling", "Mossling" },
            { "canal sheep", "Sheep" },
            { "canal scorpion", "Scorpion" },
            { "canal wraith", "Wraith" },
            { "canal minotaur", "Minotaur" },
            { "canal rockfin", "Deep-sea Fish" },
            { "canal craklaw", "Hard-shells" },
            { "canal bloodglider", "Moths" },
            { "canal dhruva", "Wind Creatures" }, // Skip the Canal Fans because they come up in another mob set
            { "canal soblyn", "Earth Creatures" },
            { "canal anala", "Mixed Elementals" },
            { "canal gagana", "Safari" },
            { "canal bombfish", "Water Creatures" },
            { "canal chuluu", "Golems" },
            { "canal arachne", "Spiders" },
            { "canal shuck", "Ghosts" },
            { "canal tengu", "Living Armour" },
            { "canal grenade", "Ice Creatures" },
            { "canal treant", "Treant" },
            { "canal belladonna", "Morbol" },
        };
        public void UpdateMobType(string mobName)
        {
            if (CurrentRoom == 7) { return; }

            if (!MobGroupNames.TryGetValue(mobName.ToLower(), out var mobGroup)) { return; }

            lastEnemy = mobGroup;
            SendUpdate(MobColumns, mobGroup);
        }

        private static readonly string[] BonusColumns = ["D", "I", "N", "S", "X", "AC"];
        public void UpdateBonusMob(string mobName)
        {
            if (CurrentRoom == 7) { return; }

            if (lastBonusMob == "Namazu")
            {
                mobName = mobName == "Namazu" ? "Namazu x2" : "Abharamu, Namazu";
            }
            else if (lastBonusMob == "Abharamu")
            {
                mobName = "Abharamu, Namazu";
            }

            lastBonusMob = mobName;
            SendUpdate(BonusColumns, mobName);
        }

        private static readonly string[] LootColumns = ["E", "J", "O", "T", "Y", "AD"];
        public void UpdateLoot(string loot)
        {
            if (CurrentRoom == 7) { return; }

            var lootParsed = "";
            switch(loot.ToLower())
            {
                case string a when a.Contains("birdbath"):
                    lootParsed = "Birdbath";
                    break;
                case string a when a.Contains("capybara pup"):
                    lootParsed = "Capybara Pup";
                    break;
                case string a when a.Contains("cloth cointe"):
                    lootParsed = "Cloth Cointe";
                    break;
                case string a when a.Contains("crimson sunrise"):
                    lootParsed = "Crimson Sunrise";
                    break;
                case string a when a.Contains("crimson sunset"):
                    lootParsed = "Crimson Sunset";
                    break;
                case string a when a.Contains("gold whisker"):
                    lootParsed = "Gold Whisker";
                    break;
                case string a when a.Contains("hedgehoglet"):
                    lootParsed = "Hedgehoglet";
                    break;
                case string a when a.Contains("mother's pride"):
                    lootParsed = "Mother's Pride";
                    break;
                case string a when a.Contains("plush pile"):
                    lootParsed = "Plush Pile";
                    break;
                case string a when a.Contains("scholarly certitude"):
                    lootParsed = "Scholarly Certitude";
                    break;
                case string a when a.Contains("waterproof cloth"):
                    lootParsed = "Waterproof Cloth";
                    break;
                case string a when a.Contains("whisperfine fleece"):
                    lootParsed = "Whisperfine Fleece";
                    break;
                case string a when a.Contains("wind-up matanga"):
                    lootParsed = "Wind-up Matanga";
                    break;

                default: return;
            }

            lastLoot = lootParsed;
            SendUpdate(LootColumns, lootParsed);
        }

        private static readonly string[] DoorColumns = ["F", "K", "P", "U", "Z", "AE"];
        public void UpdateDoor(float? doorPos)
        {
            if (lastBonusMob == "")
            {
                UpdateBonusMob("No");
            }

            if (lastDoor != "")
            {
                return;
            }

            var door = doorPos switch
            {
                < -8f => "Left",
                > 8f => "Right",
                _ => "Mid"
            };

            lastDoor = door;
            SendUpdate(DoorColumns, door);
        }

        private static readonly string[] EventColumns = ["G", "L", "Q", "V", "AA", "AF"];
        public void UpdateEvent(string eventName)
        {
            if (CurrentRoom == 7) { return; }

            if (lastEvent != "")
            {
                eventName = "Glow, Gamble";
            }

            lastEvent = eventName;
            SendUpdate(EventColumns, eventName);
        }

        public override string ToString()
        {
            return @$"Treasure Map #{CurrentRow}
Current room: {CurrentRoom}
Mob Type: {lastEnemy}
Bonus Mob: {lastBonusMob}
Loot: {lastLoot}
Event: {lastEvent}
Door: {lastDoor}";
        }

        private void SendUpdate(string[] columnIds, string data)
        {
            var range = new ValueRange { Values = [[data]], Range = $"'Thief Maps'!{columnIds[CurrentRoom - 1]}{CurrentRow}" };
            var request = Service().Spreadsheets.Values.Update(range, SheetID, range.Range);
            request.ValueInputOption = ValueInputOptionEnum.USERENTERED;
            request.ExecuteAsync();
        }
    }
}

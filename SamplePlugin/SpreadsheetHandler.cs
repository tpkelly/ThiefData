using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
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
        public void UpdateMobType(string mobName)
        {
            if (CurrentRoom == 7) { return; }

            var mobGroup = "";
            switch (mobName)
            {
                case "Canal Fluturini":
                case "Canal Halgai":
                case "Canal Mossling":
                case "Canal Sheep":
                case "Canal Scorpion":
                case "Canal Wraith":
                case "Canal Minotaur":
                    mobGroup = mobName.Replace("Canal ", "");
                    break;

                case "Canal Rockfin":
                    mobGroup = "Deep-sea Fish";
                    break;
                case "Canal Craklaw":
                    mobGroup = "Hard-shells";
                    break;
                case "Canal Bloodglider":
                    mobGroup = "Moths";
                    break;
                case "Canal Dhruva": // Skip the Canal Fans because they come up in another mob set
                    mobGroup = "Wind Creatures";
                    break;
                case "Canal Soblyn":
                    mobGroup = "Earth Creatures";
                    break;
                case "Canal Anala":
                    mobGroup = "Mixed Elementals";
                    break;
                case "Canal Gagana":
                    mobGroup = "Safari";
                    break;
                case "Canal Bombfish":
                    mobGroup = "Water Creatures";
                    break;
                case "Canal Chuluu":
                    mobGroup = "Golems";
                    break;
                case "Canal Arachne":
                    mobGroup = "Spiders";
                    break;
                case "Canal Shuck":
                    mobGroup = "Ghosts";
                    break;
                case "Canal Grenade":
                    mobGroup = "Ice Creatures";
                    break;
                case "Canal Belladonna":
                    mobGroup = "Morbol";
                    break;

                default: return;
            }

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
            switch(loot)
            {
                case string a when a.Contains("Birdbath"):
                    lootParsed = "Birdbath";
                    break;
                case string a when a.Contains("Capybara Pup"):
                    lootParsed = "Capybara Pup";
                    break;
                case string a when a.Contains("Cloth Cointe"):
                    lootParsed = "Cloth Cointe";
                    break;
                case string a when a.Contains("Crimson Sunrise"):
                    lootParsed = "Crimson Sunrise";
                    break;
                case string a when a.Contains("Crimson Sunset"):
                    lootParsed = "Crimson Sunset";
                    break;
                case string a when a.Contains("Gold Whisker"):
                    lootParsed = "Gold Whisker";
                    break;
                case string a when a.Contains("Hedgehoglet"):
                    lootParsed = "Hedgehoglet";
                    break;
                case string a when a.Contains("Mother's Pride"):
                    lootParsed = "Mother's Pride";
                    break;
                case string a when a.Contains("Plush Pile"):
                    lootParsed = "Plush Pile";
                    break;
                case string a when a.Contains("Scholarly Certitude"):
                    lootParsed = "Scholarly Certitude";
                    break;
                case string a when a.Contains("Waterproof Cloth"):
                    lootParsed = "Waterproof Cloth";
                    break;
                case string a when a.Contains("Whisperfine Fleece"):
                    lootParsed = "Whisperfine Fleece";
                    break;
                case string a when a.Contains("Wind-up Matanga"):
                    lootParsed = "Wind-up Matanga";
                    break;

                default: return;
            }

            lastLoot = lootParsed;
            SendUpdate(LootColumns, lootParsed);
        }

        private static readonly string[] DoorColumns = ["F", "K", "P", "U", "Z", "AE"];
        public void UpdateDoor(string door)
        {
            if (lastBonusMob == "")
            {
                UpdateBonusMob("No");
            }

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

using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace ThiefData
{
    public class SpreadsheetHandler : ISpreadsheetHandler
    {
        private const string SheetID = "";
        private const string APIKey = "";

        internal int CurrentRow { get; private set; }
        internal int? CurrentRoom { get; private set; }

        public SpreadsheetHandler()
        {
            var request = Service().Spreadsheets.Values.Get(SheetID, "'Thief Maps'!A1:AF999");
            var response = request.Execute();

            CurrentRow = response.Values.Count;
            CurrentRoom = int.TryParse(response.Values[CurrentRow-1][1].ToString(), out var room) ? room : null;
        }

        private static SheetsService Service()
        {
            return new SheetsService(new BaseClientService.Initializer()
            {
                ApplicationName = "ThiefData",
                ApiKey = APIKey,
            });
        }
    }
}

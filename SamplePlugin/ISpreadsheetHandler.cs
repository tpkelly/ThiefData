namespace ThiefData
{
    public interface ISpreadsheetHandler
    {
        void UpdateRoom(int room);
        void UpdateMobType(string mobName);
        void UpdateBonusMob(string mobName);
        void UpdateLoot(string loot);
        void UpdateDoor(float? doorPos);
        void UpdateEvent(string eventName);
    }
}

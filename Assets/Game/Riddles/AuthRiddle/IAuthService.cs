namespace Game.Riddles.AuthRiddle
{
    public interface IAuthService
    {
        float GetTimeLeft();
        bool ValidateCode(string input);
        float CodeValidityTime { get; }
        string CurrentCode { get; }
        float SyncStartTime { get; }
        void DeactivateRiddleServerRpc();
    }
}
using Game.Player;
using UnityEngine;

namespace Game.Teleporter.Scripts
{
    public interface ITeleportService
    {
        bool TryAssignRole(PlayerRole role, ulong clientId);
        bool ResetRole(PlayerRole role, ulong clientId);
        Vector3 GetTeleportDestination(PlayerRole role);
    }
}
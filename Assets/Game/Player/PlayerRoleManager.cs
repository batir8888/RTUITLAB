using Game.Teleporter.Scripts;
using Unity.Netcode;
using UnityEngine;
using XRMultiplayer;

namespace Game.Player
{
    public enum PlayerRole
    {
        None,
        Executor,
        Operator
    }

    public class PlayerRoleManager : NetworkBehaviour
    {
        public NetworkVariable<PlayerRole> playerRole = new();

        [ServerRpc(RequireOwnership = true)]
        public void SetRoleServerRpc(PlayerRole newRole)
        {
            if (playerRole.Value != PlayerRole.None)
            {
                Debug.Log($"Игрок {OwnerClientId} уже выбрал роль {playerRole.Value}");
                return;
            }

            bool success = ServiceLocator.Instance.GetService<ITeleportService>().TryAssignRole(newRole, OwnerClientId);
            Debug.Log(success);
            if (success)
            {
                playerRole.Value = newRole;
                Vector3 destination = ServiceLocator.Instance.GetService<ITeleportService>().GetTeleportDestination(newRole);
                GetComponent<XRINetworkPlayer>().m_XROrigin.transform.position = destination;
                Debug.Log($"Игрок {OwnerClientId} получил роль {newRole} и телепортирован в {destination}");
            }
            else
            {
                Debug.Log($"Роль {newRole} уже занята. Игрок {OwnerClientId} не может выбрать её.");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetRoleServerRpc()
        {
            if (playerRole.Value == PlayerRole.None)
            {
                Debug.Log($"Игрок {OwnerClientId} не имеет назначенной роли для сброса.");
                return;
            }

            PlayerRole oldRole = playerRole.Value;
            bool success = ServiceLocator.Instance.GetService<ITeleportService>().ResetRole(oldRole, OwnerClientId);
            if (success)
            {
                playerRole.Value = PlayerRole.None;
                Debug.Log($"Игрок {OwnerClientId} сбросил свою роль {oldRole}");
            }
            else
            {
                Debug.Log($"Не удалось сбросить роль {oldRole} для игрока {OwnerClientId}");
            }
        }
    }
}
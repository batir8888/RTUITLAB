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
        
        private ITeleportService _teleportService;
        
        [SerializeField] private TeleportService teleportService;

        public override void OnNetworkSpawn()
        {
            teleportService = FindObjectOfType<TeleportService>();
            _teleportService = teleportService != null ? teleportService : ServiceLocator.Instance.GetService<ITeleportService>();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetRoleServerRpc(PlayerRole newRole)
        {
            if (playerRole.Value != PlayerRole.None)
            {
                Debug.Log($"Игрок {OwnerClientId} уже выбрал роль {playerRole.Value}");
                return;
            }

            bool success = _teleportService.TryAssignRole(newRole, OwnerClientId);
            Debug.Log(success);
            if (success)
            {
                playerRole.Value = newRole;
                Vector3 destination = _teleportService.GetTeleportDestination(newRole);
                TeleportClientRpc(destination);
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
            bool success = !_teleportService.TryAssignRole(oldRole, OwnerClientId);
            if (success)
            {
                teleportService.ResetRole(oldRole, OwnerClientId);
                playerRole.Value = PlayerRole.None;
                Debug.Log($"Игрок {OwnerClientId} сбросил свою роль {oldRole}");
            }
            else
            {
                Debug.Log($"Не удалось сбросить роль {oldRole} для игрока {OwnerClientId}");
            }
        }
        
        [ClientRpc]
        private void TeleportClientRpc(Vector3 destination)
        {
            XRINetworkPlayer xrPlayer = GetComponent<XRINetworkPlayer>();
            if (xrPlayer != null)
            {
                xrPlayer.m_XROrigin.transform.position = destination;
            }
        }
    }
}
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
        
        public override void OnNetworkDespawn()
        {
            ResetRoleServerRpc();
            var destination = _teleportService.GetTeleportDestination(PlayerRole.None);
            TeleportClientRpc(destination);
            teleportService = null;
            _teleportService = null;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetRoleServerRpc(PlayerRole newRole)
        {
            if (playerRole.Value != PlayerRole.None)
            {
                Debug.Log($"Игрок {OwnerClientId} уже выбрал роль {playerRole.Value}");
                return;
            }

            var success = _teleportService.TryAssignRole(newRole, OwnerClientId);
            Debug.Log(success);
            if (success)
            {
                playerRole.Value = newRole;
                var destination = _teleportService.GetTeleportDestination(newRole);
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

            var oldRole = playerRole.Value;
            var success = !_teleportService.TryAssignRole(oldRole, OwnerClientId);
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
            var xrPlayer = GetComponent<XRINetworkPlayer>();
            if (xrPlayer != null)
            {
                xrPlayer.m_XROrigin.transform.position = destination;
            }
        }
    }
}
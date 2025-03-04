using System.Collections.Generic;
using Game.Player;
using Unity.Netcode;
using UnityEngine;

namespace Game.Teleporter.Scripts
{
    public class TeleportService : NetworkBehaviour, ITeleportService
    {
        [Header("Teleport Destinations")]
        [SerializeField] private Transform executorDestination;
        [SerializeField] private Transform operatorDestination;
        [SerializeField] private Transform defaultDestination;
        [SerializeField] private Transform finalDestination;

        private readonly Dictionary<PlayerRole, ulong> _assignedRoles = new();

        public override void OnNetworkSpawn()
        {
            ServiceLocator.Instance.RegisterService<ITeleportService>(this);
        }

        private void OnDisable()
        {
            ServiceLocator.Instance.RegisterService<ITeleportService>(this);
        }

        public bool TryAssignRole(PlayerRole role, ulong clientId)
        {
            if (_assignedRoles.ContainsKey(role))
            {
                return false;
            }
            _assignedRoles[role] = clientId;
            return true;
        }

        public bool ResetRole(PlayerRole role, ulong clientId)
        {
            if (_assignedRoles.TryGetValue(role, out var assignedClient))
            {
                if (assignedClient == clientId)
                {
                    _assignedRoles.Remove(role);
                    return true;
                }
            }
            return false;
        }

        public Vector3 GetTeleportDestination(PlayerRole role)
        {
            return role switch
            {
                PlayerRole.Executor => executorDestination.position,
                PlayerRole.Operator => operatorDestination.position,
                PlayerRole.Final => finalDestination.position,
                _ => defaultDestination != null ? defaultDestination.position : Vector3.zero
            };
        }
    }
}

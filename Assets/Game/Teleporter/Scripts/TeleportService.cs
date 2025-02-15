using System.Collections.Generic;
using Game.Player;
using UnityEngine;

namespace Game.Teleporter.Scripts
{
    public class TeleportService : MonoBehaviour, ITeleportService
    {
        [Header("Teleport Destinations")]
        [SerializeField] private Transform executorDestination;
        [SerializeField] private Transform operatorDestination;
        [SerializeField] private Transform defaultDestination;

        private readonly Dictionary<PlayerRole, ulong> _assignedRoles = new();

        private void Awake()
        {
            ServiceLocator.Instance.RegisterService<ITeleportService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Instance.UnregisterService<ITeleportService>();
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
            if (_assignedRoles.TryGetValue(role, out ulong assignedClient))
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
            switch (role)
            {
                case PlayerRole.Executor:
                    return executorDestination.position;
                case PlayerRole.Operator:
                    return operatorDestination.position;
                default:
                    return defaultDestination != null ? defaultDestination.position : Vector3.zero;
            }
        }
    }
}

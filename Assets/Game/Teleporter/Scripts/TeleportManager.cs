using System;
using UnityEngine;
using Unity.Netcode;

namespace Game.Teleporter.Scripts
{
    public enum TeleportZoneType
    {
        Executor,
        Operator
    }

    public class TeleportManager : NetworkBehaviour
    {
        public static TeleportManager Instance;

        private NetworkObject _playerInExecutorZone;
        private Vector3 _executorZoneDestination;

        private NetworkObject _playerInOperatorZone;
        private Vector3 _operatorZoneDestination;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterPlayer(NetworkObject player, TeleportZoneType zoneType, Vector3 teleportDestination)
        {
            if (!IsServer)
                return;

            switch (zoneType)
            {
                case TeleportZoneType.Executor:
                    _playerInExecutorZone = player;
                    _executorZoneDestination = teleportDestination;
                    break;
                case TeleportZoneType.Operator:
                    _playerInOperatorZone = player;
                    _operatorZoneDestination = teleportDestination;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(zoneType), zoneType, null);
            }

            CheckAndTeleport();
        }

        public void UnregisterPlayer(NetworkObject player, TeleportZoneType zoneId)
        {
            if (!IsServer)
                return;

            switch (zoneId)
            {
                case TeleportZoneType.Executor when _playerInExecutorZone == player:
                    _playerInExecutorZone = null;
                    break;
                case TeleportZoneType.Operator when _playerInOperatorZone == player:
                    _playerInOperatorZone = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(zoneId), zoneId, null);
            }
        }

        private void CheckAndTeleport()
        {
            if (_playerInExecutorZone != null && _playerInOperatorZone != null &&
                _playerInExecutorZone != _playerInOperatorZone)
            {
                _playerInExecutorZone.transform.position = _executorZoneDestination;
                _playerInOperatorZone.transform.position = _operatorZoneDestination;

                _playerInExecutorZone = null;
                _playerInOperatorZone = null;
            }
        }
    }
}

using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Teleporter.Scripts
{
    public class TeleportZone : NetworkBehaviour
    {
        public Transform teleportDestination;
        
        [SerializeField] private TeleportZoneType zoneType;

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            
            var player = other.GetComponent<NetworkObject>();
            
            if (player == null) return;
            TeleportManager.Instance.RegisterPlayer(player, zoneType, teleportDestination.position);
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!IsServer) return;
            
            var player = other.GetComponent<NetworkObject>();
            
            if (player == null) return;
            TeleportManager.Instance.UnregisterPlayer(player, zoneType);
        }
    }
}
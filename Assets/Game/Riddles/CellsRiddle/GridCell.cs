using Unity.Netcode;
using UnityEngine;

namespace Game.Riddles.CellsRiddle
{
    [RequireComponent(typeof(CellInteractable)), RequireComponent(typeof(NetworkObject))]
    public class GridCell : NetworkBehaviour
    {
        public NetworkVariable<bool> isActive = new(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        private MeshRenderer _meshRenderer;
        
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material inactiveMaterial;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
        
        public override void OnNetworkSpawn()
        {
            UpdateColor(isActive.Value);
            isActive.OnValueChanged += OnIsActiveChanged;
        }

        public override void OnNetworkDespawn()
        {
            isActive.OnValueChanged -= OnIsActiveChanged;
        }

        private void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            UpdateColor(newValue);
        }
        
        private void UpdateColor(bool active)
        {
            _meshRenderer.material = active ? activeMaterial : inactiveMaterial;
        }

        public void ToggleState()
        {
            if (IsServer)
            {
                isActive.Value = !isActive.Value;
            }
            else
            {
                RequestToggleStateServerRpc();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RequestToggleStateServerRpc()
        {
            isActive.Value = !isActive.Value;
        }
    }
}
using Game.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.RoleSelectionUI
{
    public class RoleSelectionUI : MonoBehaviour
    {
        [SerializeField] private Button executorButton;
        [SerializeField] private Button operatorButton;
        [SerializeField] private Button resetButton;
    
        private PlayerRoleManager _localPlayerRoleManager;

        private void Start()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SpawnManager != null)
            {
                NetworkObject localPlayerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                if (localPlayerObject != null)
                {
                    _localPlayerRoleManager = localPlayerObject.GetComponent<PlayerRoleManager>();
                }
            }

            if (_localPlayerRoleManager == null)
            {
                Debug.LogWarning("Local PlayerRoleManager не найден!");
            }

            executorButton.onClick.AddListener(() => OnRoleSelected(PlayerRole.Executor));
            operatorButton.onClick.AddListener(() => OnRoleSelected(PlayerRole.Operator));
            resetButton.onClick.AddListener(OnResetRole);
        }

        private void OnRoleSelected(PlayerRole role)
        {
            _localPlayerRoleManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerRoleManager>();
            
            if (_localPlayerRoleManager != null)
            {
                _localPlayerRoleManager.SetRoleServerRpc(role);
            }
            else
            {
                Debug.LogWarning("Локальный PlayerRoleManager не найден или объект не является владельцем.");
            }
        }

        private void OnResetRole()
        {
            _localPlayerRoleManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerRoleManager>();
            
            if (_localPlayerRoleManager != null && _localPlayerRoleManager.IsOwner)
            {
                _localPlayerRoleManager.ResetRoleServerRpc();
            }
            else
            {
                Debug.LogWarning("Локальный PlayerRoleManager не найден или объект не является владельцем.");
            }
        }
    }
}
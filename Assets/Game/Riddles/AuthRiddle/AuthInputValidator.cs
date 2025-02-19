using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game.Riddles.AuthRiddle
{
    public class AuthInputValidator : NetworkBehaviour
    {
        [Header("Поле ввода кода")]
        public TMP_InputField codeInput;

        private IAuthService _authService;
        
        [SerializeField] private AuthService authService;
        public override void OnNetworkSpawn()
        {
            authService = FindObjectOfType<AuthService>();
            _authService = authService != null ? authService : ServiceLocator.Instance.GetService<IAuthService>();
        }

        public void ValidateCode()
        {
            if (_authService == null) return;
            var valid = _authService.ValidateCode(codeInput.text);
            if (valid) _authService.DeactivateRiddleServerRpc();
            Debug.Log(valid ? "Доступ разрешен!" : "Неверный код. Доступ запрещен!");
        }
    }
}
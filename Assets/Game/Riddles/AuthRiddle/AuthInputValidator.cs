using TMPro;
using UnityEngine;

namespace Game.Riddles.AuthRiddle
{
    public class AuthInputValidator : MonoBehaviour
    {
        [Header("Поле ввода кода")]
        public TMP_InputField codeInput;

        private IAuthService _authService;

        private void Start()
        {
            _authService = ServiceLocator.Instance.GetService<IAuthService>();
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
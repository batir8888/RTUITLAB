using UnityEngine;
using UnityEngine.UI;

namespace Game.Riddles.AuthRiddle
{
    public class AuthInputValidator : MonoBehaviour
    {
        [Header("Поле ввода кода")]
        public InputField codeInput;

        private IAuthService _authService;

        private void Start()
        {
            _authService = ServiceLocator.Instance.GetService<IAuthService>();
        }

        public void ValidateCode()
        {
            if (_authService == null) return;
            var valid = _authService.ValidateCode(codeInput.text);
            Debug.Log(valid ? "Доступ разрешен!" : "Неверный код. Доступ запрещен!");
        }
    }
}
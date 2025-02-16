using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Riddles.AuthRiddle
{
    public class AuthUIController : MonoBehaviour
    {
        [Header("UI компоненты")]
        public Text codeDisplay;

        public Text timerDisplay;
        public Image progressBar;

        private IAuthService _authService;

        private void Start()
        {
            _authService = ServiceLocator.Instance.GetService<IAuthService>();
            StartCoroutine(UpdateUICoroutine());
        }

        private IEnumerator UpdateUICoroutine()
        {
            while (true)
            {
                if (_authService != null)
                {
                    codeDisplay.text = _authService.CurrentCode;
                    var timeLeft = _authService.GetTimeLeft();
                    timerDisplay.text = Mathf.Ceil(timeLeft).ToString();
                    progressBar.fillAmount = timeLeft / _authService.CodeValidityTime;
                }
                yield return null;
            }
        }
    }
}
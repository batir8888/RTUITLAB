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
        
        private IAuthService _authService;
        
        [SerializeField] private AuthService authService;

        private void Start()
        {
            _authService = authService != null ? authService : ServiceLocator.Instance.GetService<IAuthService>();
            StartCoroutine(UpdateUICoroutine());
        }

        private IEnumerator UpdateUICoroutine()
        {
            while (true)
            {
                codeDisplay.text = _authService.CurrentCode;
                var timeLeft = _authService.GetTimeLeft();
                timerDisplay.text = Mathf.Ceil(timeLeft).ToString();
                yield return null;
            }
        }
    }
}
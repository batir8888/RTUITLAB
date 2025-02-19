using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Riddles.AuthRiddle
{
    public class AuthUIController : NetworkBehaviour
    {
        [Header("UI компоненты")]
        public Text codeDisplay;
        public Text timerDisplay;
        
        private IAuthService _authService;
        
        [SerializeField] private AuthService authService;

        public override void OnNetworkSpawn()
        {
            authService = FindObjectOfType<AuthService>();
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
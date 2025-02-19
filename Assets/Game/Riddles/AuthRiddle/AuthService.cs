using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Game.Riddles.AuthRiddle
{
    public class AuthService : NetworkBehaviour, IAuthService
    {
        [Header("Настройки аутентификации")]
        public bool isMaster;
        public string CurrentCode => _currentCode;
        public float SyncStartTime => _syncStartTime;
        [field: SerializeField] public float CodeValidityTime { get; private set; } = 10f;

        private string _currentCode;
        private float _syncStartTime;

        [SerializeField] private NetworkDoor door;
        
        private void Awake()
        {
            ServiceLocator.Instance.RegisterService<IAuthService>(this);
        }

        public override void OnNetworkSpawn()
        {
            ServiceLocator.Instance.RegisterService<IAuthService>(this);
            if (IsServer && isMaster)
            {
                StartCoroutine(MasterAuthCoroutine());
            }
        }

        private IEnumerator MasterAuthCoroutine()
        {
            while (true)
            {
                GenerateNewCode();
                _syncStartTime = Time.time;

                BroadcastNewCodeClientRpc(_currentCode, _syncStartTime);

                var startTime = Time.time;
                while (Time.time - startTime < CodeValidityTime)
                {
                    yield return null;
                }
            }
        }

        private void GenerateNewCode()
        {
            _currentCode = Random.Range(100000, 999999).ToString();
            Debug.Log("Сгенерирован новый код: " + _currentCode);
        }

        [ClientRpc]
        private void BroadcastNewCodeClientRpc(string code, float syncTime)
        {
            _currentCode = code;
            _syncStartTime = syncTime;
            Debug.Log("Получен новый код через RPC: " + _currentCode + " со временем: " + _syncStartTime);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void DeactivateRiddleServerRpc()
        {
            door.IsInteracted = true;
        }

        public float GetTimeLeft()
        {
            return Mathf.Clamp(CodeValidityTime - (Time.time - _syncStartTime), 0, CodeValidityTime);
        }
        
        public bool ValidateCode(string input)
        {
            return input == _currentCode;
        }
    }
}
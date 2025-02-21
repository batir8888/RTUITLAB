using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Game.Riddles
{
    [RequireComponent(typeof(NetworkTransform))]
    public class NetworkDoor : NetworkBehaviour
    {
        public bool IsInteracted 
        {
            set
            {
                if (IsServer)
                {
                    _isInteracted.Value = value;
                }
                else
                {
                    RequestInteractionServerRpc(value);
                }
            } 
        }

        private readonly NetworkVariable<bool> _isInteracted = new();

        [SerializeField] private float moveDistance = 2f;
        [SerializeField] private float moveTime = 1f;

        private Vector3 _closedPosition;
        private Vector3 _openPosition;
        private Coroutine _moveCoroutine;

        public override void OnNetworkSpawn()
        {
            _closedPosition = transform.position;
            _openPosition = _closedPosition + Vector3.up * moveDistance;

            if (IsServer)
            {
                _isInteracted.OnValueChanged += OnInteractionChanged;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                _isInteracted.OnValueChanged -= OnInteractionChanged;
            }
            base.OnNetworkDespawn();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestInteractionServerRpc(bool interact)
        {
            _isInteracted.Value = interact;
        }

        private void OnInteractionChanged(bool oldValue, bool newValue)
        {
            MoveDoorClientRpc(newValue);
        }

        [ClientRpc(RequireOwnership = false)]
        private void MoveDoorClientRpc(bool newValue)
        {
            if (_moveCoroutine != null)
                StopCoroutine(_moveCoroutine);

            var from = newValue ? _closedPosition : _openPosition;
            var to = newValue ? _openPosition : _closedPosition;

            _moveCoroutine = StartCoroutine(MoveDoor(from, to));
        }

        private IEnumerator MoveDoor(Vector3 from, Vector3 to)
        {
            var elapsed = 0f;
            while (elapsed < moveTime)
            {
                transform.position = Vector3.Lerp(from, to, elapsed / moveTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = to;
        }
    }
}

using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Game.Riddles
{
    [RequireComponent(typeof(NetworkTransform))]
    public class NetworkDoor : NetworkBehaviour
    {
        public bool IsInteracted { set => _isInteracted.Value = value; }

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

        private void OnInteractionChanged(bool oldValue, bool newValue)
        {
            if (_moveCoroutine != null)
                StopCoroutine(_moveCoroutine);

            _moveCoroutine = StartCoroutine(MoveDoor(newValue ? _closedPosition : _openPosition,
                newValue ? _openPosition : _closedPosition));
        }

        private IEnumerator MoveDoor(Vector3 from, Vector3 to)
        {
            float elapsed = 0f;
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
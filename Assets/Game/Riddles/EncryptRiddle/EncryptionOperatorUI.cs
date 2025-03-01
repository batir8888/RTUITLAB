using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game.Riddles.EncryptRiddle
{
    public class EncryptionOperatorUI : NetworkBehaviour
    {
        [SerializeField] private TMP_InputField inputField;

        public void OnSubmit()
        {
            var enteredText = inputField.text.Trim();
            SubmitEncryptedWordServerRpc(enteredText);
        }

        public override void OnNetworkSpawn()
        {
            inputField.text = "";
        }

        [ServerRpc]
        private void SubmitEncryptedWordServerRpc(string enteredText)
        {
            var encryptionManager = ServiceLocator.Instance.GetService<EncryptionManager>();
            if (encryptionManager == null) return;
            var correct = enteredText == encryptionManager.TargetEncryptedWord;
            if (correct) encryptionManager.OnCorrectWord();
            Debug.Log("Operator submitted: " + enteredText);
            Debug.Log("Expected: " + encryptionManager.TargetEncryptedWord);
            Debug.Log("Correct? " + correct);
        }
    }
}
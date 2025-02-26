using Unity.Netcode;
using UnityEngine;

namespace Game.Riddles.FilesRiddle
{
    public class FilesPuzzleManager : NetworkBehaviour
    {
        public static FilesPuzzleManager Instance;

        [SerializeField] private NetworkDoor door;

        private void Awake()
        {
            Instance = this;
        }

        public void OnCorrectDeletion(string fileName)
        {
            Debug.Log($"Вредоносный файл '{fileName}' удалён. Система разблокирована!");
            UpdateTerminalClientRpc($"Файл '{fileName}' успешно удалён. Система разблокирована!");
        }

        public void OnIncorrectDeletion(string fileName)
        {
            Debug.Log($"Файл '{fileName}' не был заражён. Наказание: добавляем штрафные файлы!");
            UpdateTerminalClientRpc($"Ошибка: Файл '{fileName}' не заражён. Наказание применено...");
        }

        public void OnFileNotFound(string fileName)
        {
            Debug.Log($"Файл '{fileName}' не найден!");
            UpdateTerminalClientRpc($"Файл '{fileName}' не найден!");
        }

        [ClientRpc]
        private void UpdateTerminalClientRpc(string message)
        {
            var terminal = FindObjectOfType<CommandTerminal>();
            if (terminal != null)
                terminal.AppendTerminalMessage(message);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeactivateRiddleServerRpc()
        {
            door.IsInteracted = true;
        }
    }
}
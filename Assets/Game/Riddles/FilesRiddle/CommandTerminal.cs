using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game.Riddles.FilesRiddle
{
    public class CommandTerminal : NetworkBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text terminalOutput;

        private void Start()
        {
            terminalOutput.text = "";
        }

        public void OnSubmitCommand()
        {
            var command = inputField.text.Trim();
            if (string.IsNullOrEmpty(command))
                return;

            inputField.text = "";
            ProcessCommand(command);
        }

        private void ProcessCommand(string command)
        {
            var output = new StringBuilder();

            if (command.ToLower() == "help")
            {
                output.AppendLine("Доступные команды:");
                output.AppendLine("delete <имя файла> - удалить файл");
                output.AppendLine("help - показать команды");
                terminalOutput.text += "\n" + output;
                return;
            }

            if (command.ToLower().StartsWith("delete"))
            {
                var parts = command.Split(' ');
                if (parts.Length < 2)
                {
                    output.AppendLine("Неверная команда. Пример: delete File_0");
                    terminalOutput.text += "\n" + output;
                    return;
                }
                var fileName = parts[1];

                var manager = FindObjectOfType<FileManager>();
                if (manager != null && manager.IsServer)
                {
                    manager.RequestDeleteFileServerRpc(fileName);
                }
                else if (manager != null)
                {
                    manager.RequestDeleteFileServerRpc(fileName);
                }
                else
                {
                    output.AppendLine("FileManager не найден!");
                }

                terminalOutput.text += "\n" + output;
                return;
            }

            output.AppendLine("Неизвестная команда. Используйте 'help' для списка команд.");
            terminalOutput.text += "\n" + output;
        }

        public void AppendTerminalMessage(string message)
        {
            terminalOutput.text += "\n" + message;
        }
    }
}
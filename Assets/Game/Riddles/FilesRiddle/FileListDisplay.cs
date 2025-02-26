using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game.Riddles.FilesRiddle
{
    public class FileListDisplay : NetworkBehaviour
    {
        [SerializeField] private TMP_Text fileListText;
        
        private WaitForSeconds _timer;
        private FileManager _manager;

        private void Start()
        {
            _manager = FindObjectOfType<FileManager>();
        }

        private void Update()
        {
            if (_manager == null || _manager.FileList == null)
                return;

            var files = _manager.FileList;
            var sb = new StringBuilder();
            sb.AppendLine("Список файлов:");

            foreach (var file in files)
            {
                var infected = file.isMalicious;
                var fileName = infected
                    ? $"<color=red>{file.fileName}</color>"
                    : file.fileName.ToString();

                sb.Append($"{fileName} | {file.creationDate} | {file.fileSize} байт");
                sb.AppendLine();
            }

            fileListText.text = sb.ToString();
        }
    }
}
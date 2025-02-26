using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Game.Riddles.FilesRiddle
{
    public class FileManager : NetworkBehaviour
    {
        public NetworkList<FileInfoEntry> FileList { get; private set; }

        [SerializeField] private int startFileCount = 25;
        [SerializeField] private int startMaliciousCount = 5;
        [SerializeField] private int penaltyCount = 5;
        [SerializeField] private int penaltyMaliciousCount = 1;

        private void Awake()
        {
            FileList = new NetworkList<FileInfoEntry>(
                readPerm: NetworkVariableReadPermission.Everyone,
                writePerm: NetworkVariableWritePermission.Server
            );
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                SpawnInitialFiles(startFileCount, startMaliciousCount);
            }
        }

        private void SpawnInitialFiles(int count, int maliciousCount)
        {
            var infectedIndices = new HashSet<int>();
            while (infectedIndices.Count < maliciousCount)
            {
                infectedIndices.Add(UnityEngine.Random.Range(0, count));
            }

            for (var i = 0; i < count; i++)
            {
                var entry = new FileInfoEntry
                {
                    fileName = new FixedString32Bytes(i < 10 ? $"File_0{i}" : $"File_{i}"),
                    fileSize = UnityEngine.Random.Range(1000, 10000),
                    creationDate = new FixedString32Bytes(DateTime.Now.AddMinutes(-UnityEngine.Random.Range(0, 60)).ToString("HH:mm")),
                    isMalicious = infectedIndices.Contains(i)
                };
                FileList.Add(entry);
            }
        }

        private void SpawnPenaltyFiles(int count, int maliciousCount)
        {
            var infectedIndices = new HashSet<int>();
            while (infectedIndices.Count < maliciousCount)
            {
                infectedIndices.Add(UnityEngine.Random.Range(0, count));
            }

            var currentCount = FileList.Count;
            for (var i = 0; i < count; i++)
            {
                var entry = new FileInfoEntry
                {
                    fileName = new FixedString32Bytes($"Penalty_File_{currentCount + i}"),
                    fileSize = UnityEngine.Random.Range(1000, 10000),
                    creationDate = new FixedString32Bytes(DateTime.Now.AddMinutes(-UnityEngine.Random.Range(0, 60)).ToString("HH:mm")),
                    isMalicious = infectedIndices.Contains(i)
                };
                FileList.Add(entry);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDeleteFileServerRpc(string fileName)
        {
            var index = -1;
            for (var i = 0; i < FileList.Count; i++)
            {
                if (!FileList[i].fileName.ToString().Equals(fileName, StringComparison.OrdinalIgnoreCase)) continue;
                index = i;
                break;
            }

            if (index < 0)
            {
                FilesPuzzleManager.Instance.OnFileNotFound(fileName);
                return;
            }

            var file = FileList[index];

            if (file.isMalicious)
            {
                FilesPuzzleManager.Instance.OnCorrectDeletion(file.fileName.ToString());
                FileList.RemoveAt(index);
                if (CheckMaliciousCount() == 0) FilesPuzzleManager.Instance.DeactivateRiddleServerRpc();
            }
            else
            {
                FilesPuzzleManager.Instance.OnIncorrectDeletion(file.fileName.ToString());
                FileList.RemoveAt(index);
                SpawnPenaltyFiles(penaltyCount, penaltyMaliciousCount);
            }
        }

        private int CheckMaliciousCount()
        {
            var count = 0;
            foreach (var file in FileList)
            {
                if (file.isMalicious) count++;
            }

            return count;
        }
    }
}
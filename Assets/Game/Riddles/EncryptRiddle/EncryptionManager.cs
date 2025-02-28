using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class EncryptionManager : NetworkBehaviour
{
    public event Action CorrectWord;
    public string TargetEncryptedWord { get; private set; }
    
    private readonly Dictionary<char, string> _letterCodes = new();
    private readonly HashSet<string> _usedCodes = new();
    private const string CodeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    
    [SerializeField] private string targetWord;
    [SerializeField] private TMP_Text[] executorBoards;

    public override void OnNetworkSpawn()
    {
        ServiceLocator.Instance.RegisterService(this);
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        if (!IsServer) return;
        GenerateLetterCodes();
        ComputeEncodedWord();
        SetupBoardsClientRpc();
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        ServiceLocator.Instance.UnregisterService<EncryptionManager>();
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            SetupBoardsClientRpc();
        }
    }

    private string GenerateUniqueCode()
    {
        string code;
        do
        {
            code = "";
            for (var i = 0; i < 4; i++)
            {
                var index = Random.Range(0, CodeCharacters.Length);
                code += CodeCharacters[index];
            }
        } 
        while (_usedCodes.Contains(code));
        
        _usedCodes.Add(code);
        return code;
    }

    private void GenerateLetterCodes()
    {
        _letterCodes.Clear();
        _usedCodes.Clear();
        for (var c = 'A'; c <= 'Z'; c++)
        {
            _letterCodes.Add(c, GenerateUniqueCode());
        }
    }

    private void ComputeEncodedWord()
    {
        var codeList = new List<string>();
        foreach (var c in targetWord)
        {
            if (_letterCodes.TryGetValue(c, out var code))
            {
                codeList.Add(code);
            }
        }

        TargetEncryptedWord = string.Join(" ", codeList);
    }

    [ClientRpc]
    private void SetupBoardsClientRpc()
    {
        var letters = new List<char>();
        for (var c = 'A'; c <= 'Z'; c++)
        {
            letters.Add(c);
        }

        var boardSizes = new[] { 7, 7, 6, 6 };
        var letterIndex = 0;
        for (var boardIndex = 0; boardIndex < executorBoards.Length; boardIndex++)
        {
            var count = boardSizes[boardIndex];
            var boardText = "";
            for (var i = 0; i < count; i++)
            {
                var letter = letters[letterIndex];
                var code = _letterCodes[letter];
                boardText += $"{letter}: {code}\n";
                letterIndex++;
            }

            UpdateBoardClientRpc(boardText, boardIndex);
        }
    }

    public void OnCorrectWord()
    {
        CorrectWord?.Invoke();
    }

    [ClientRpc]
    private void UpdateBoardClientRpc(string boardText, int boardIndex)
    {
        if (executorBoards != null && boardIndex < executorBoards.Length)
        {
            executorBoards[boardIndex].text = boardText;
        }
    }
}

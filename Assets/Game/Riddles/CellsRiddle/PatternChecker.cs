using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Riddles.CellsRiddle
{
    public class PatternChecker : NetworkBehaviour
    {
        [Header("Сетка исполнителя (Executor)")]
        [SerializeField] private GridCellRow[] executorGrid = new GridCellRow[7];

        [Header("Операторский узор (Reference Pattern)")]
        [SerializeField] private BoolRow[] operatorPattern = new BoolRow[7];

        [SerializeField] private Transform cellsParent;
        [SerializeField] private NetworkDoor door;

        private void OnValidate()
        {
            if (executorGrid.Length != 7) executorGrid = new GridCellRow[7];
            if (operatorPattern.Length != 7) operatorPattern = new BoolRow[7];

            for (var i = 0; i < 7; i++)
            {
                executorGrid[i] ??= new GridCellRow();
                if (executorGrid[i].row == null || executorGrid[i].row.Length != 7)
                    executorGrid[i].row = new GridCell[7];

                operatorPattern[i] ??= new BoolRow();
                if (operatorPattern[i].row == null || operatorPattern[i].row.Length != 7)
                    operatorPattern[i].row = new bool[7];
                if (operatorPattern[i].matched == null || operatorPattern[i].matched.Length != 7)
                    operatorPattern[i].matched = new bool[7];
            }

            if (cellsParent == null)
            {
                Debug.LogWarning("cellsParent не назначен в инспекторе!", this);
                return;
            }

            if (cellsParent.childCount < 7)
            {
                Debug.LogWarning($"Ожидается минимум 7 дочерних объектов (Row), а найдено {cellsParent.childCount}", this);
                return;
            }

            for (var rowIndex = 0; rowIndex < 7; rowIndex++)
            {
                var rowTransform = cellsParent.GetChild(rowIndex);

                if (rowTransform.childCount < 7)
                {
                    Debug.LogWarning($"В объекте {rowTransform.name} недостаточно дочерних объектов (Cells). Найдено {rowTransform.childCount}", rowTransform);
                    continue;
                }

                for (var cellIndex = 0; cellIndex < 7; cellIndex++)
                {
                    var cellTransform = rowTransform.GetChild(cellIndex);
                    var cell = cellTransform.GetComponent<GridCell>();

                    if (cell == null)
                    {
                        Debug.LogWarning($"В {cellTransform.name} нет компонента GridCell!", cellTransform);
                        continue;
                    }

                    executorGrid[rowIndex].row[cellIndex] = cell;
                }
            }
        }
        
        public override void OnNetworkSpawn()
        {
            for (var i = 0; i < 7; i++)
            {
                for (var j = 0; j < 7; j++)
                {
                    var cell = executorGrid[i].row[j];
                    if (cell == null) continue;

                    cell.isActive.OnValueChanged += OnCellStateChanged;
                }
            }
        }
        
        private void OnCellStateChanged(bool previousValue, bool newValue)
        {
            if (CheckPattern())
            {
                OpenDoor();
            }
        }
        
        private bool CheckPattern()
        {
            var allMatched = true;

            for (var i = 0; i < 7; i++)
            {
                for (var j = 0; j < 7; j++)
                {
                    var cell = executorGrid[i].row[j];
                    var cellActive = (cell != null && cell.isActive.Value);

                    operatorPattern[i].matched[j] = (cellActive == operatorPattern[i].row[j]);

                    if (!operatorPattern[i].matched[j])
                    {
                        allMatched = false;
                    }
                    
                    Debug.Log($"Cell [{i},{j}] - Executor: {cellActive}, Operator: {operatorPattern[i].row[j]}, Matched: {operatorPattern[i].matched[j]}");
                }
            }

            return allMatched;
        }
        
        private void OpenDoor()
        {
            Debug.Log("Дверь открыта!");
            OpenDoorClientRpc();
        }

        [ClientRpc]
        private void OpenDoorClientRpc()
        {
            door.IsInteracted = true;
            Debug.Log("Дверь открыта на клиенте!");
        }
    }

    [Serializable]
    public class Row<T>
    {
        public T[] row = new T[7];
    }

    [Serializable]
    public class GridCellRow : Row<GridCell> { }

    [Serializable]
    public class BoolRow : Row<bool>
    {
        public bool[] matched = new bool[7];
    }
    
}
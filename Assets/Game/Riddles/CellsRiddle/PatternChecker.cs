using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Riddles.CellsRiddle
{
    public class PatternChecker : NetworkBehaviour
    {
        [Header("Сетка исполнителя (Executor)")]
        [SerializeField] private GridCellRow[] executorGrid = new GridCellRow[7];

        [Header("Операторский узор (Reference Pattern)")]
        [SerializeField] private BoolRow[] operatorPattern = new BoolRow[7];

        [Header("Родитель для ячеек исполнителя")]
        [SerializeField] private Transform cellsParent;
        
        [Header("Дверь")]
        [SerializeField] private NetworkDoor door;

        [Header("Родитель для отображения оператора (монитор)")]
        [SerializeField] private Transform operatorCellsParent;
        [SerializeField, Range(0.1f, 0.9f)] private float chanceToCell;
        
        private Image[,] _operatorImages = new Image[7, 7];
        
        private void Awake()
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

            if (operatorCellsParent == null)
            {
                Debug.LogWarning("operatorCellsParent не назначен в инспекторе!", this);
                return;
            }

            if (operatorCellsParent.childCount < 7)
            {
                Debug.LogWarning($"Ожидается минимум 7 строк (Row) в operatorCellsParent, а найдено {operatorCellsParent.childCount}", this);
                return;
            }

            for (var i = 0; i < 7; i++)
            {
                var rowTransform = operatorCellsParent.GetChild(i);
                if (rowTransform.childCount < 7)
                {
                    Debug.LogWarning($"В объекте {rowTransform.name} недостаточно дочерних объектов (Circle). Найдено {rowTransform.childCount}", rowTransform);
                    continue;
                }

                for (var j = 0; j < 7; j++)
                {
                    var circleTransform = rowTransform.GetChild(j);
                    var circleImage = circleTransform.GetComponent<Image>();
                    if (circleImage == null)
                    {
                        Debug.LogWarning($"В {circleTransform.name} нет компонента Image!", circleTransform);
                        continue;
                    }

                    _operatorImages[i, j] = circleImage;
                }
            }
        }
        
        public override void OnNetworkSpawn()
        {
            GenerateRandomOperatorPattern();
            UpdateOperatorMonitorClientRpc(OperatorPatternToByteArray());
            
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
                    
                    //Debug.Log($"Cell [{i},{j}] - Executor: {cellActive}, Operator: {operatorPattern[i].row[j]}, Matched: {operatorPattern[i].matched[j]}");
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
        
        private void GenerateRandomOperatorPattern()
        {
            for (var i = 0; i < operatorPattern.Length; i++)
            {
                operatorPattern[i] ??= new BoolRow
                {
                    row = new bool[7],
                    matched = new bool[7]
                };
                for (var j = 0; j < operatorPattern[i].row.Length; j++)
                {
                    operatorPattern[i].row[j] = UnityEngine.Random.value < chanceToCell;
                }
            }
            
            Debug.Log("Сгенерирован случайный операционный узор на сервере.");
        }

        private void UpdateOperatorMonitor()
        {
            if (operatorCellsParent == null) return;
            
            for (var i = 0; i < 7; i++)
            {
                for (var j = 0; j < 7; j++)
                {
                    if (_operatorImages != null && _operatorImages[i, j] == null) continue;

                    if (_operatorImages != null)
                        _operatorImages[i, j].color = operatorPattern[i].row[j] ? Color.red : Color.white;
                }
            }
        }

        [ClientRpc]
        private void UpdateOperatorMonitorClientRpc(byte[] patternData)
        {
            var bools = ByteArrayToBoolArray(patternData);
            var index = 0;
            for (var i = 0; i < 7; i++)
            {
                for (var j = 0; j < 7; j++)
                {
                    operatorPattern[i].row[j] = bools[index];
                    index++;
                }
            }

            UpdateOperatorMonitor();
        }

        private byte[] OperatorPatternToByteArray()
        {
            var bools = new bool[7 * 7];
            var index = 0;
            for (var i = 0; i < 7; i++)
            {
                for (var j = 0; j < 7; j++)
                {
                    bools[index] = operatorPattern[i].row[j];
                    index++;
                }
            }

            return BoolArrayToByteArray(bools);
        }

        private byte[] BoolArrayToByteArray(bool[] boolArray)
        {
            var byteLength = (boolArray.Length + 7) / 8;
            var bytes = new byte[byteLength];

            var bitIndex = 0;
            for (var i = 0; i < boolArray.Length; i++)
            {
                if (boolArray[i])
                {
                    bytes[bitIndex / 8] |= (byte)(1 << (bitIndex % 8));
                }
                bitIndex++;
            }

            return bytes;
        }

        private bool[] ByteArrayToBoolArray(byte[] bytes)
        {
            var boolArray = new bool[7 * 7];
            var bitIndex = 0;
            for (var i = 0; i < boolArray.Length; i++)
            {
                boolArray[i] = (bytes[bitIndex / 8] & (1 << (bitIndex % 8))) != 0;
                bitIndex++;
            }
            return boolArray;
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
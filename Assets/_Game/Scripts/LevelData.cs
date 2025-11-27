using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "level", menuName = "Game/level", order = 0)]
    public class LevelData : ScriptableObject
    {
        [Tooltip("cols, rows")]
        [field: SerializeField]
        public Vector2Int GridSize { get; private set; }

        [field: SerializeField] public Collection<Sprite> Sprites { get; private set; }
        [field: SerializeField] public Sprite BackfaceSprite { get; private set; }

        [Tooltip("will be automatically incremented to keep even cells count")] [SerializeField]
        private int _emptyCellsCount;

        public int PairsCount
        {
            get
            {
                var totalCells = (GridSize.x * GridSize.y) - _emptyCellsCount;
                return totalCells / 2;
            }
        }
        
        public int CellsCount => GridSize.x * GridSize.y;

        public int EmptyCellsCount => CellsCount - PairsCount * 2;

        [ContextMenu("Validate")]
        public bool ValidateLevelData()
        {
            if (Sprites.Count < PairsCount)
            {
                Debug.LogError($"Sprites Count {Sprites.Count} is not large enough");
                return false;
            }

            return true;
        }
    }
}
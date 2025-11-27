using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "level", menuName = "Game/level", order = 0)]
    public class LevelData : ScriptableObject
    {
        [field: SerializeField] public Vector2Int GridSize { get; private set; }
        [field: SerializeField] public Collection<Sprite> Sprites { get; private set; }
        [field: SerializeField] public Sprite BackfaceSprite { get; private set; }

        [ContextMenu("Validate")]
        public bool ValidateLevelData()
        {
            var cellsCount = GridSize.x * GridSize.y;
            if (cellsCount % 2 != 0)
            {
                Debug.LogError($"CellsCount {cellsCount} is not even");
                return false;
            }

            if (Sprites.Count < cellsCount / 2)
            {
                Debug.LogError($"Sprites Count {Sprites.Count} is not large enough");
                return false;
            }
            return true;
        }
    }
}
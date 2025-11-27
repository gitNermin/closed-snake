using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class LevelGridUI : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup _grid;

        Vector2 _gridSize;
        private bool _isInitialized = false;

        public void Setup(Vector2Int size)
        {
            if (!_isInitialized)
            {
                RectTransform gridTransform = (RectTransform)_grid.transform;
                _gridSize = gridTransform.sizeDelta;
                _isInitialized = true;
            }

            //Todo: handle -ve size
            float sizeX = (_gridSize.x - (size.x - 1) * _grid.spacing.x) / size.x;
            float sizeY = (_gridSize.y - (size.y - 1) * _grid.spacing.y) / size.y;

            _grid.cellSize = new Vector2(sizeX, sizeY);
            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = size.x;
        }
    }
}
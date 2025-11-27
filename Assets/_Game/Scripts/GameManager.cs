using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelData[] _levels;
        [SerializeField] private LevelGridUI _levelGridUI;
        [SerializeField] private GridCellUI _gridCellPrefab;

        private GridCellUI _activeCell;
        private int _collectedCellsCount;
        private int _targetCollectedCellsCount;

        void Start()
        {
            LoadLevel(0);
        }

        private void LoadLevel(int levelIndex)
        {
            if(!_levels[levelIndex].ValidateLevelData())
                return;
            
            _levelGridUI.Setup(_levels[levelIndex].GridSize);
            
            //todo: use pooling
            _levelGridUI.transform.RemoveAllChildren();
            int cellsCount = _levels[levelIndex].GridSize.x * _levels[levelIndex].GridSize.y;
            _targetCollectedCellsCount = cellsCount / 2;
            _activeCell = null;
            var sprites = _levels[levelIndex].Sprites.GetRandomItems(_targetCollectedCellsCount);
            for (int i = 0; i < _targetCollectedCellsCount; i++)
            {
                Guid id = Guid.NewGuid();
                CreateCell(id, sprites[i], _levels[levelIndex].BackfaceSprite);
                CreateCell(id, sprites[i], _levels[levelIndex].BackfaceSprite);
            }
            _levelGridUI.transform.ShuffleChildren();
        }

        private void CreateCell(Guid id, Sprite front, Sprite back)
        { 
            GridCellUI cell = Instantiate(_gridCellPrefab, _levelGridUI.transform);
            cell.Setup(id, front, back, OnCellClicked);
        }


        private void OnCellClicked(GridCellUI cell)
        {
            cell.SetState(CellState.FrontFace);
            //flip back after a time
            if (!_activeCell)
            {
                _activeCell = cell;
            }

            else if(cell.Id == _activeCell.Id)
            {
                _collectedCellsCount++;
                cell.SetState(CellState.Collected);
                _activeCell.SetState(CellState.Collected);
                _activeCell = null;
            }
            
            else
            {
                cell.SetState(CellState.BackFace);
                _activeCell.SetState(CellState.BackFace);
                _activeCell = null;
                
            }
        }
        
    }
}

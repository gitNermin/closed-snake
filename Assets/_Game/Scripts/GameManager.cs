using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelData[] _levels;
        [SerializeField] private LevelGridUI _levelGridUI;
        [SerializeField] private GridCellUI _gridCellPrefab;
        [SerializeField] private Transform _emptyCellPrefab;

        [SerializeField] private UnityEvent _onLevelStart;
        [SerializeField] private UnityEvent _onLevelWin;
        [SerializeField] private UnityEvent _onLevelLose;
        

        [Header("Level Settings")]
        [SerializeField] private float _cellFlipTime;

        [SerializeField] private GridCellUI _lastRevealedCell;
        private int _matchedPairs;
        private int _pairsCount;
        
        int CurrentLevelIndex
        {
            get => PlayerPrefs.GetInt("CurrentLevelIndex", 0);
            set
            {
                if (value >= _levels.Length)
                {
                    PlayerPrefs.SetInt("CurrentLevelIndex", 0);
                }
                
                PlayerPrefs.SetInt("CurrentLevelIndex", value);
            }
        }

        void Start()
        {
            LoadLevel(CurrentLevelIndex);
        }

        private void LoadLevel(int levelIndex)
        {
            if (!_levels[levelIndex].ValidateLevelData())
                return;

            Clear();

            _levelGridUI.Setup(_levels[levelIndex].GridSize);
            
            _pairsCount = _levels[levelIndex].PairsCount;
            
            List<Sprite> sprites = _levels[levelIndex].Sprites.GetRandomItems(_pairsCount);
            for (int i = 0; i < _pairsCount; i++)
            {
                Guid id = Guid.NewGuid();
                CreateCell(id, sprites[i], _levels[levelIndex].BackfaceSprite);
                CreateCell(id, sprites[i], _levels[levelIndex].BackfaceSprite);
            }
            
            CreateEmptyCells(levelIndex);

            _levelGridUI.transform.ShuffleChildren();
        }

        private void CreateCell(Guid id, Sprite front, Sprite back)
        {
            GridCellUI cell = Instantiate(_gridCellPrefab, _levelGridUI.transform);
            cell.Setup(id, front, back, OnCellClicked);
        }


        private void CreateEmptyCells(int levelIndex)
        {
            var emptyCellsCount = _levels[levelIndex].EmptyCellsCount;
            for (int i = 0; i < emptyCellsCount; i++)
            {
                Instantiate(_emptyCellPrefab, _levelGridUI.transform);
            }
        }
        
        private void OnCellClicked(GridCellUI cell)
        {
            if (!_lastRevealedCell)
            {
                RevealCell(cell);
            }
            else if (cell.Id == _lastRevealedCell.Id)
            {
                OnMatch(cell);
            }
            else
            {
                OnMismatch(cell);
            }
        }
        

        private void RevealCell(GridCellUI cell)
        {
            cell.SetState(CellState.FrontFace);
            
            _lastRevealedCell = cell;
            
            cell.SetStateAsync(CellState.BackFace, _cellFlipTime, ResetLastRevealedCell);
        }

        private void ResetLastRevealedCell()
        {
            _lastRevealedCell = null;
        }

        private void OnMatch(GridCellUI cell)
        {
            _matchedPairs++;
            
            cell.SetState(CellState.Matched);
            _lastRevealedCell.SetState(CellState.Matched);
            
            _lastRevealedCell = null;

            if (_matchedPairs == _pairsCount)
            {
                // game win
            }
        }

        private void OnMismatch(GridCellUI cell)
        {
            cell.SetState(CellState.FrontFace);

            _lastRevealedCell.SetStateAsync(CellState.BackFace, _cellFlipTime);
            cell.SetStateAsync(CellState.BackFace, _cellFlipTime);

            _lastRevealedCell = null;
        }
        
        private void Clear()
        {
            _levelGridUI.transform.RemoveAllChildren();
            _lastRevealedCell = null;
        }
        
    }
}
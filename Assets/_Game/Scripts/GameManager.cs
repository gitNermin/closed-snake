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
        [SerializeField] private Transform _emptyCellPrefab;
        
        
        //todo: listen to events from code
        
        [SerializeField] private UnityEvent<int, FailCondition, float> _onLevelStart;
        [SerializeField] private UnityEvent _onLevelWin;
        [SerializeField] private UnityEvent _onLevelLose;

        [SerializeField] private UnityEvent<int> _onTurnsLeftUpdated;
        [SerializeField] private UnityEvent<int> _onMatchesUpdated;
        [SerializeField] private UnityEvent<int, int> _onScoreUpdated;

        [Header("Settings")] [SerializeField] private float _cellFlipTime;

        private int _pairsCount;
        private GridCellUI _lastRevealedCell;

        private int _turnsCount;
        private int _matchedPairs;
        private int _score;

        private LevelData _currentLevel;

        private int _combo = 1;

        int CurrentLevelIndex
        {
            get => PlayerPrefs.GetInt("CurrentLevelIndex", 0);
            set => PlayerPrefs.SetInt("CurrentLevelIndex", value >= _levels.Length ? 0 : value);
        }

        void Start()
        {
            LoadLevel(CurrentLevelIndex);
        }

        private void LoadLevel(int levelIndex)
        {
            if (!_levels[levelIndex].ValidateLevelData())
            {
                LoadNextLevel();
            }

            Clean();

            _currentLevel = _levels[levelIndex];

            _levelGridUI.Setup(_currentLevel.GridSize);

            _pairsCount = _currentLevel.PairsCount;

            List<Sprite> sprites = _currentLevel.Sprites.GetRandomItems(_pairsCount);
            for (int i = 0; i < _pairsCount; i++)
            {
                Guid id = Guid.NewGuid();
                CreateCellPair(id, sprites[i], _currentLevel.BackfaceSprite);
            }

            CreateEmptyCells();

            _levelGridUI.transform.ShuffleChildren();

            _onLevelStart?.Invoke(CurrentLevelIndex + 1, _currentLevel.FailCondition, _currentLevel.ConditionValue);

            if (_currentLevel.FailCondition == FailCondition.Time)
            {
                Invoke(nameof(FailLevel), _currentLevel.ConditionValue);
            }
        }

        private void CreateCellPair(Guid id, Sprite front, Sprite back)
        {
            GridCellUI cell = Instantiate(_gridCellPrefab, _levelGridUI.transform);
            cell.Setup(id, front, back, OnCellClicked);

            cell = Instantiate(_gridCellPrefab, _levelGridUI.transform);
            cell.Setup(id, front, back, OnCellClicked);
        }


        private void CreateEmptyCells()
        {
            var emptyCellsCount = _currentLevel.EmptyCellsCount;
            for (int i = 0; i < emptyCellsCount; i++)
            {
                Instantiate(_emptyCellPrefab, _levelGridUI.transform);
            }
        }

        private void OnCellClicked(GridCellUI cell)
        {
            if (cell == _lastRevealedCell)
                return;

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
            _onMatchesUpdated?.Invoke(_matchedPairs);
            
            int deltaScore = _combo * _currentLevel.PointsPerMatch;
            _score += deltaScore;
            
            _onScoreUpdated?.Invoke(_score, deltaScore);

            _combo++;
            
            cell.SetState(CellState.Matched);
            _lastRevealedCell.SetState(CellState.Matched);

            _lastRevealedCell = null;

            if (_matchedPairs == _pairsCount)
            {
                CancelInvoke(nameof(FailLevel));
                _onLevelWin?.Invoke();
            }

            else
            {
                IncrementTurns();
            }
            
        }

        private void OnMismatch(GridCellUI cell)
        {
            _combo = 1;
            
            cell.SetState(CellState.FrontFace);

            _lastRevealedCell.SetStateAsync(CellState.BackFace, _cellFlipTime);
            cell.SetStateAsync(CellState.BackFace, _cellFlipTime);

            _lastRevealedCell = null;

            IncrementTurns();
        }

        void IncrementTurns()
        {
            _turnsCount++;
            
            if (_currentLevel.FailCondition == FailCondition.Turns)
            {
                _onTurnsLeftUpdated?.Invoke((int)_currentLevel.ConditionValue - _turnsCount);
            }
            if (_currentLevel.FailCondition == FailCondition.Turns && _turnsCount >= _currentLevel.ConditionValue)
            {
                FailLevel();
            }
        }

        public void LoadNextLevel()
        {
            CurrentLevelIndex += 1;
            LoadLevel(CurrentLevelIndex);
        }

        public void Retry()
        {
            LoadLevel(CurrentLevelIndex);
        }

        private void FailLevel()
        {
            _onLevelLose?.Invoke();
        }

        private void Clean()
        {
            CancelInvoke(nameof(FailLevel));
            _levelGridUI.transform.RemoveAllChildren();
            _lastRevealedCell = null;
            _matchedPairs = 0;
            _turnsCount = 0;
        }


#if UNITY_EDITOR
        [ContextMenu("ResetLevelIndex")]
        private void ResetLevelIndex()
        {
            PlayerPrefs.DeleteKey("CurrentLevelIndex");
        }
#endif
    }
}
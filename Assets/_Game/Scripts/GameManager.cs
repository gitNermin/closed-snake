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
        private int _turnsCount;
        private int _matchedPairs;
        private int _score;
        private float _startTime = 0;
        private int _combo = 1;

        private GridCellUI _lastRevealedCell;

        private LevelData _currentLevel;

        private LevelState _state;

        int CurrentLevelIndex
        {
            get => PlayerPrefs.GetInt("CurrentLevelIndex", 0);
            set => PlayerPrefs.SetInt("CurrentLevelIndex", value >= _levels.Length ? 0 : value);
        }

        void Start()
        {
            LevelSaveData levelData = PlayerLocalDataManager.LoadData<LevelSaveData>();
            if (levelData != null)
            {
                LoadLevel(levelData);
            }
            else
                LoadLevel(CurrentLevelIndex);
        }

        void LoadLevel(LevelSaveData saveData)
        {
            if (!_levels[saveData.Index].ValidateLevelData())
            {
                LoadNextLevel();
                return;
            }

            ResetLevelData();

            SetCurrentLevelData(saveData.Index);

            CreateCellsFromCellData(saveData.CellsData);

            _score = saveData.Score;
            


            _turnsCount = saveData.TurnsCount;

            _matchedPairs = saveData.MatchedPairs;

            if (_currentLevel.FailCondition == FailCondition.Turns)
            {
                _onLevelStart?.Invoke(CurrentLevelIndex + 1, _currentLevel.FailCondition, _currentLevel.ConditionValue);
                _onTurnsLeftUpdated?.Invoke((int)_currentLevel.ConditionValue - _turnsCount);
            }
            else if (_currentLevel.FailCondition == FailCondition.Time)
            {
                var remainingTime = _currentLevel.ConditionValue - saveData.ElapsedTime;
                Invoke(nameof(FailLevel), remainingTime);
                _onLevelStart?.Invoke(CurrentLevelIndex + 1, _currentLevel.FailCondition, remainingTime);
                _startTime = Time.time - saveData.ElapsedTime;
            }
            _onScoreUpdated?.Invoke(_score, 0);
        }

        private void LoadLevel(int levelIndex)
        {
            if (!_levels[levelIndex].ValidateLevelData())
            {
                LoadNextLevel();
                return;
            }

            ResetLevelData();

            SetCurrentLevelData(levelIndex);

            CreateCellsFromLevelData();

            CreateEmptyCells();

            _levelGridUI.transform.ShuffleChildren();

            _onLevelStart?.Invoke(CurrentLevelIndex + 1, _currentLevel.FailCondition, _currentLevel.ConditionValue);

            if (_currentLevel.FailCondition == FailCondition.Time)
            {
                Invoke(nameof(FailLevel), _currentLevel.ConditionValue);
                _startTime = Time.time;
            }
        }

        private void CreateCellsFromLevelData()
        {
            List<Sprite> sprites = _currentLevel.Sprites.GetRandomItems(_pairsCount);
            for (int i = 0; i < _pairsCount; i++)
            {
                int id = _currentLevel.Sprites.IndexOf(sprites[i]);
                CreateCellPair(id, sprites[i], _currentLevel.BackfaceSprite);
            }
        }

        private void CreateCellsFromCellData(CellSaveData[] cellData)
        {
            for (int i = 0; i < cellData.Length; i++)
            {
                if (cellData[i] == null) continue;
                if (cellData[i].IsEmptyCell)
                {
                    Instantiate(_emptyCellPrefab, _levelGridUI.transform);
                }
                else
                {
                    GridCellUI cell = Instantiate(_gridCellPrefab, _levelGridUI.transform);
                    cell.Setup(cellData[i].Id, _currentLevel.Sprites[cellData[i].Id],
                        _currentLevel.BackfaceSprite, OnCellClicked);

                    if (cellData[i].IsMatched)
                    {
                        cell.SetState(CellState.Matched);
                    }
                }
            }
        }

        private void SetCurrentLevelData(int levelIndex)
        {
            _currentLevel = _levels[levelIndex];

            _levelGridUI.Setup(_currentLevel.GridSize);

            _pairsCount = _currentLevel.PairsCount;
        }

        private void CreateCellPair(int id, Sprite front, Sprite back)
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

            SoundManager.Instance.PlaySound(Audio.Flip);

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
            SoundManager.Instance.PlaySound(Audio.Match);

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
                WinLevel();
            }

            else
            {
                IncrementTurns();
            }
        }

        private void OnMismatch(GridCellUI cell)
        {
            SoundManager.Instance.PlaySound(Audio.Mismatch);

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
            SoundManager.Instance.PlaySound(Audio.GameOver);
            _onLevelLose?.Invoke();
            PlayerLocalDataManager.DeleteData<LevelSaveData>();

            _state = LevelState.Lose;
        }

        private void WinLevel()
        {
            CancelInvoke(nameof(FailLevel));
            _onLevelWin?.Invoke();
            PlayerLocalDataManager.DeleteData<LevelSaveData>();
            
            _state = LevelState.Win;
        }

        private void ResetLevelData()
        {
            CancelInvoke(nameof(FailLevel));
            _levelGridUI.transform.RemoveAllChildren();
            
            _pairsCount = 0;
            _turnsCount = 0;
            _matchedPairs = 0;
            _score = 0;
            _startTime = Time.time;
            _combo = 1;

            _lastRevealedCell = null;
            _currentLevel = null;
            _state = LevelState.Running;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveLevelDataData();
            }
            else if(_state == LevelState.Lose)
            {
                Retry();
            }
            else if(_state == LevelState.Win)
            {
                LoadNextLevel();
            }
        }


        private void OnApplicationQuit()
        {
            if(_state == LevelState.Running)
                SaveLevelDataData();
            else if (_state == LevelState.Win)
                CurrentLevelIndex++;
        }

        private void SaveLevelDataData()
        {
            if(_state != LevelState.Running)
                PlayerLocalDataManager.DeleteData<LevelSaveData>();

            var levelData = new LevelSaveData()
            {
                ElapsedTime = Time.time - _startTime,
                Index = CurrentLevelIndex,
                MatchedPairs = _matchedPairs,
                Score = _score,
                TurnsCount = _turnsCount,
                CellsData = new CellSaveData[_levelGridUI.transform.childCount],
            };

            int i = 0;
            foreach (Transform cell in _levelGridUI.transform)
            {
                if (cell.TryGetComponent(out GridCellUI cellUI))
                {
                    levelData.CellsData[i++] = cellUI.SaveData;
                }
                else
                {
                    levelData.CellsData[i++] = new CellSaveData
                    {
                        IsEmptyCell = true
                    };
                }
            }
            
            PlayerLocalDataManager.SaveData(levelData);
        }


#if UNITY_EDITOR
        [ContextMenu("ResetLevelIndex")]
        private void ResetLevelIndex()
        {
            PlayerPrefs.DeleteKey("CurrentLevelIndex");
        }
        
        [ContextMenu("ResetLevelData")]
        private void ResetLevelSaveData()
        {
            PlayerLocalDataManager.DeleteData<LevelSaveData>();
        }
#endif
    }

    public enum LevelState
    {
        Running,
        Win,
        Lose,
    }

    [Serializable]
    public class LevelSaveData
    {
        public int Index;
        public int Score;
        public int TurnsCount;
        public int MatchedPairs;
        public float ElapsedTime;
        public CellSaveData[] CellsData;
    }

    [Serializable]
    public class CellSaveData
    {
        public int Id;
        public bool IsMatched;
        public bool IsEmptyCell;
    }
}
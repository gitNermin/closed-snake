using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelData[] _levels;
        [SerializeField] private LevelGridUI _levelGridUI;
        [SerializeField] private GridCellUI _gridCellPrefab;

        [Header("Level Settings")] [SerializeField]
        private float _cellFlipTime;

        [SerializeField] private GridCellUI _revealedCell;
        private int _matchedPairs;
        private int _pairsCount;

        CancellationToken _flipCellCancellationToken;
        private CancellationTokenSource _cts;

        //todo: fire event
        private bool _isEnableInput;

        void Start()
        {
            LoadLevel(0);
        }

        private void LoadLevel(int levelIndex)
        {
            if (!_levels[levelIndex].ValidateLevelData())
                return;

            CancelFlip();
            
            _isEnableInput = true;
            _levelGridUI.Setup(_levels[levelIndex].GridSize);

            //todo: use pooling
            _levelGridUI.transform.RemoveAllChildren();
            int cellsCount = _levels[levelIndex].GridSize.x * _levels[levelIndex].GridSize.y;
            _pairsCount = cellsCount / 2;
            _revealedCell = null;
            var sprites = _levels[levelIndex].Sprites.GetRandomItems(_pairsCount);
            for (int i = 0; i < _pairsCount; i++)
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
            if (!_isEnableInput)
                return;
            
            CancelFlip();

            if (!_revealedCell)
            {
                RevealCell(cell);
            }
            else if (cell.Id == _revealedCell.Id)
            {
                ResolveMatch(cell);
            }
            else
            {
                ResetCells(cell);
            }
        }
        

        private void RevealCell(GridCellUI cell)
        {
            cell.SetState(CellState.FrontFace);

            _cts = new CancellationTokenSource();
            _flipCellCancellationToken = _cts.Token;
            _revealedCell = cell;

            FlipRevealedCell();
        }
        
        private void ResolveMatch(GridCellUI cell)
        {
            _matchedPairs++;
            cell.SetState(CellState.Matched);
            _revealedCell.SetState(CellState.Matched);
            _revealedCell = null;

            if (_matchedPairs == _pairsCount)
            {
                // game win
            }
        }

        private async void FlipRevealedCell()
        {
            try
            {
                await Task.Delay((int)(_cellFlipTime * 1000), _flipCellCancellationToken);
                if (_cts.IsCancellationRequested)
                    return;
            
                _revealedCell.SetState(CellState.BackFace);
                _revealedCell = null;
            }
            catch (Exception)
            {
                //flip is cancelled
            }
            
        }

        private void ResetCells(GridCellUI cell)
        {
            _isEnableInput = false;
            
            cell.SetState(CellState.FrontFace);
            
            _cts = new CancellationTokenSource();
            _flipCellCancellationToken = _cts.Token;

            FlipCells(cell);
        }

        private async void FlipCells(GridCellUI cell)
        {
            try
            {
                await Task.Delay((int)(_cellFlipTime * 1000), _flipCellCancellationToken);
            
                _revealedCell.SetState(CellState.BackFace);
                cell.SetState(CellState.BackFace);
            
                _revealedCell = null;
                _isEnableInput = true;
            }
            catch (Exception)
            {
                //flip is cancelled
            }
            
        }

        private void CancelFlip()
        {
            
            if (_cts == null) return;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        
    }
}
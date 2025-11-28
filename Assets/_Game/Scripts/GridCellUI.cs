using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    public class GridCellUI : MonoBehaviour, IPointerClickHandler
    {
        private static readonly int FrontFace = Animator.StringToHash("FrontFace");
        private static readonly int BackFace = Animator.StringToHash("BackFace");
        private static readonly int Matched = Animator.StringToHash("Matched");
        
        [SerializeField] private Graphic _targetGraphic;
        [SerializeField] private Image _frontFaceImage;
        [SerializeField] private Image _backFaceImage;
        [SerializeField] private Animator _anim;

        private int _id;
        private UnityAction<GridCellUI> _onClick;

        private CancellationTokenSource _tokenSource;
        private CancellationToken _cancellationToken;
        

        public int Id => _id;

        private CellState _state;

        public CellSaveData SaveData => new CellSaveData()
        {
            Id = _id,
            IsMatched = _state == CellState.Matched
        };

        public void Setup(int id, Sprite frontSprite, Sprite backSprite, UnityAction<GridCellUI> onClick)
        {
            _id = id;
            _onClick = onClick;
            _frontFaceImage.sprite = frontSprite;
            _backFaceImage.sprite = backSprite;
        }

        public async void SetStateAsync(CellState state, float delay, Action onFinish = null)
        {
            CancelSetState();
            
            _tokenSource = new CancellationTokenSource();
            _cancellationToken = _tokenSource.Token;
            
            try
            {
                await Task.Delay((int)(delay * 1000), _cancellationToken);
                SetState(state);
                onFinish?.Invoke();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void SetState(CellState state)
        {
            CancelSetState();
            
            switch (state)
            {
                case CellState.FrontFace:
                    _targetGraphic.raycastTarget = true;
                    ShowFrontFace();
                    break;
                case CellState.BackFace:
                    _targetGraphic.raycastTarget = true;
                    ShowBackFace();
                    break;
                case CellState.Matched:
                    _targetGraphic.raycastTarget = false;
                    ShowMatched();
                    break;
            }

            _state = state;
        }
        
        private void ShowFrontFace()
        {
            _anim.SetTrigger(FrontFace);
        }

        private void ShowBackFace()
        {
            _anim.SetTrigger(BackFace);
        }

        private void ShowMatched()
        {
            _anim.SetTrigger(Matched);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick?.Invoke(this);
        }


        private void CancelSetState()
        {
            if(_tokenSource == null) return;
            
            _tokenSource.Cancel();
            _tokenSource = null;
        }
        
        
    }
}

public enum CellState
{
    BackFace,
    FrontFace,
    Matched
}
using System;
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
        [SerializeField] private Image _image;
        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _matchedColor;

        private Guid _id;
        private bool _isFront;
        private Sprite _backSprite;
        private Sprite _frontSprite;
        private UnityAction<GridCellUI> _onClick;

        private CancellationTokenSource _tokenSource;
        private CancellationToken _cancellationToken;
        

        public Guid Id => _id;

        public void Setup(Guid id, Sprite frontSprite, Sprite backSprite, UnityAction<GridCellUI> onClick)
        {
            _id = id;
            _onClick = onClick;
            _backSprite = backSprite;
            _frontSprite = frontSprite;
            
            ShowBackFace();
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
                    ShowFrontFace();
                    _image.color = _normalColor;
                    _image.raycastTarget = true;
                    break;
                case CellState.BackFace:
                    ShowBackFace();
                    _image.color = _normalColor;
                    _image.raycastTarget = true;
                    break;
                case CellState.Matched:
                    ShowFrontFace();
                    _image.color = _matchedColor;
                    _image.raycastTarget = false;
                    break;
            }
        }
        
        private void ShowFrontFace()
        {
            if(_isFront) return;
            _image.sprite = _frontSprite;
            _isFront = true;
        }

        private void ShowBackFace()
        {
            if(!_isFront) return;
            _image.sprite = _backSprite;
            _isFront = false;
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
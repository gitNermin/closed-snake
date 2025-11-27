using System;
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
        

        public Guid Id => _id;

        public void Setup(Guid id, Sprite frontSprite, Sprite backSprite, UnityAction<GridCellUI> onClick)
        {
            _id = id;
            _onClick = onClick;
            _backSprite = backSprite;
            _frontSprite = frontSprite;
            
            ShowBackFace();
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

        public void SetState(CellState state)
        {
            switch (state)
            {
                case CellState.FrontFace:
                    ShowFrontFace();
                    _image.color = _normalColor;
                    _image.raycastTarget = false;
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

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick?.Invoke(this);
        }
    }
}

public enum CellState
{
    BackFace,
    FrontFace,
    Matched
}
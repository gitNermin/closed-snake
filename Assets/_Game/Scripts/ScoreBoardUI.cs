using System.Collections;
using TMPro;
using UnityEngine;

namespace Game
{
    public class ScoreBoardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _scoreToast;
        [SerializeField] private Transform _scoreToastParent;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _matchedPairsText;
        [SerializeField] private TMP_Text _timeLeftText;
        [SerializeField] private TMP_Text _turnsLeftText;
        [SerializeField] private GameObject _timeLeftObj;
        [SerializeField] private GameObject _turnsLeftObj;
        
        Coroutine _timerCoroutine;

        public void OnLevelStarted(int level, FailCondition condition, float value)
        {
            _levelText.text = level.ToString();
            _scoreText.text = "0";
            _matchedPairsText.text = "0";

            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
            }
            
            if (condition == FailCondition.Time)
            {
                _timeLeftObj.SetActive(true);
                _turnsLeftObj.SetActive(false);
                _timerCoroutine = StartCoroutine(UpdateTime(value));
            }
            else
            {
                _timeLeftObj.SetActive(false);
                _turnsLeftObj.SetActive(true);
            }
        }
        
        public void OnScoreUpdated(int score, int deltaValue)
        {
            _scoreText.text = score.ToString();
            
            if(deltaValue <= 0) 
                return;
            
            //todo: use pool
            var toast = Instantiate(_scoreToast, _scoreToastParent);
            toast.text = $"+{deltaValue}";
            Destroy(toast.gameObject, 1f);
        }

        public void OnMatchedPairsUpdated(int matchedPairs)
        {
            _matchedPairsText.text = matchedPairs.ToString();
        }

        public void OnTurnsUpdated(int turns)
        {
            _turnsLeftText.text = turns.ToString();
        }

        IEnumerator UpdateTime(float timeLeft)
        {
            while (timeLeft > 0)
            {
                _timeLeftText.text = timeLeft.ToClockFormat();
                yield return new WaitForSeconds(1);
                timeLeft -= 1;
            }
        }
    }
}
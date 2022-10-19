using System.Collections;
using UnityEngine;

namespace SystemProgramming
{
    public sealed class Unit : MonoBehaviour
    {
        [SerializeField] private bool _startHealing;
        [SerializeField] private int _health;

        private float _healingTime = 3.0f;
        private float _healingStepPeriod = 0.5f;
        private int _healthPointPerStep = 5;
        private int _maxHealth = 100;

        private bool _isHealingNow;

        private void Update()
        {
            if (_startHealing)
            {
                _startHealing = false;
                ReceiveHealing();
            }
        }

        public void ReceiveHealing()
        {
            if (_isHealingNow)
            {
                Debug.Log("Healing already is in progress");
            }
            else
            {
                _isHealingNow = true;
                StartCoroutine(DoHealing(_healingTime));
            }
        }

        private IEnumerator DoHealing(float time)
        {
            while (time > 0.0f)
            {
                time -= _healingStepPeriod;
                _health += _healthPointPerStep;

                if (_health >= _maxHealth)
                {
                    _health = _maxHealth;
                    _isHealingNow = false;
                    yield break;
                }

                yield return StartCoroutine(Tick(_healingStepPeriod));
            }
            _isHealingNow = false;
        }

        private IEnumerator Tick(float time)
        {
            yield return new WaitForSeconds(time);
        }
    }
}
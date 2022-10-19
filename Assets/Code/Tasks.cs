using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SystemProgramming
{
    public sealed class Tasks : MonoBehaviour
    {
        [SerializeField] private bool _startTasks;
        [SerializeField] private bool _cancel;
        [SerializeField] private int _secondsForTask1 = 1;
        [SerializeField] private int _framesForTask2 = 60;

        private CancellationTokenSource _tokenSource;

        private void Update()
        {
            if (_startTasks)
            {
                Debug.Log("Start");
                _tokenSource?.Cancel();
                _startTasks = false;
                _tokenSource = new CancellationTokenSource();
                Task1Acync(_tokenSource.Token);
                Task2Async(_tokenSource.Token);
            }

            if (_cancel)
            {
                Debug.Log("Call cancelation");
                _cancel = false;
                _tokenSource?.Cancel();
            }
        }

        private void OnDestroy()
        {
            _tokenSource?.Dispose();
        }

        private async void Task1Acync(CancellationToken token)
        {
            //if (token.IsCancellationRequested)
            //{
            //    Debug.Log("Task1 was canceled");
            //    return;
            //}
            //await Task.Delay(_secondsForTask1 * 1000);
            float timeLeft = _secondsForTask1;
            while (timeLeft >= 0.0f)
            {
                if (token.IsCancellationRequested)
                {
                    Debug.Log($"Task1 was canceled after {_secondsForTask1 - timeLeft} sec");
                    return;
                }
                timeLeft -= Time.deltaTime;
                await Task.Yield();
            }
            Debug.Log($"Task1 was completed after {_secondsForTask1} sec");
        }

        private async void Task2Async(CancellationToken token)
        {
            int frames = 0;
            while (frames < _framesForTask2)
            {
                if (token.IsCancellationRequested)
                {
                    Debug.Log($"Task2 was canceled after {frames} frames");
                    return;
                }
                await Task.Yield();
                frames++;
            }
            Debug.Log($"Task2 was completed after {frames}");
        }
    }
}
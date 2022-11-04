using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;


namespace SystemProgramming.Lesson2Jobs
{
    public sealed class TaskTwo : MonoBehaviour
    {
        //      Часть 2. Cоздайте задачу типа IJobParallelFor, которая будет принимать данные в
        //  виде двух контейнеров: Positions и Velocities — типа NativeArray<Vector3>.Также
        //  создайте массив FinalPositions типа NativeArray<Vector3>.
        //  Сделайте так, чтобы в результате выполнения задачи в элементы массива
        //  FinalPositions были записаны суммы соответствующих элементов массивов Positions
        //  и Velocities.
        //  Вызовите выполнение созданной задачи из внешнего метода и выведите в консоль
        //  результат.

        [SerializeField] private bool _run;

        [Space]
        [SerializeField] private bool _randomize;
        [SerializeField] private int _arrayLength;

        [Space]
        [SerializeField] private Vector3[] _positions;
        [SerializeField] private Vector3[] _velocities;

        private NativeArray<Vector3> _positionsNative;
        private NativeArray<Vector3> _velocitiesNative;
        private NativeArray<Vector3> _finalPositions;

        private int _frames;// Счётчик кадров, за которые выполняется задача
        private bool _inProgress;// Нужен только для подсчёта кадров, за которые выполнится задача

        private MyTaskTwo _myTask;
        private JobHandle _jobHandle;

        private void Update()
        {
            if (_randomize)
            {
                Debug.LogWarning("Clicked Randomize");
                CreateData();
                _randomize = false;
            }

            if (_run)
            {
                Debug.LogWarning("Clicked Run");
                StartJob();
                _run = false;
            }

            if (_inProgress)
            {
                CheckProgress();
            }
        }

        private void CheckProgress()
        {
            if (_jobHandle.IsCompleted)
            {
                _jobHandle.Complete();
                // _jobHandle.Complete() совсем не нужен.
                // Почти совсем.
                // Если задача выполняется дольше одного кадра, то не нужен.
                // Если задача выполняется один кадр и в этом же кадре попытаться освободить NativeArray(Dispose),
                // то без предварительно вызванного jobHandle.Complete() будет ошибка -система будет считать задачу
                // всё ещё невыполненной, хотя _jobHandle.IsCompleted уже будет в true...
                // _jobHandle.Complete() перенёс в то место, где освобождаю первый контейнер(сразу же перед ним).
                Debug.LogWarning("_jobHandle IsCompleted");
                _inProgress = false;
                _positionsNative.Dispose();
                _velocitiesNative.Dispose();

                // Тут основной поток подвисает при больших значениях (100 000)
                foreach (var item in _finalPositions)
                {
                    Debug.Log(item);
                }

                _finalPositions.Dispose();
            }
            else
            {
                Debug.LogWarning($"Frame = {Time.frameCount - _frames}");
                return;
            }
        }

        private void CreateData()
        {
            // Тут основной поток подвисает при больших значениях (100 000)
            _positions = new Vector3[_arrayLength];
            _velocities = new Vector3[_arrayLength];

            for (int i = 0; i < _arrayLength; i++)
            {
                _positions[i] = Random.insideUnitSphere;
                _velocities[i] = Random.insideUnitSphere;
            }
        }

        private void StartJob()
        {
            Debug.LogWarning("Create NativeArray's");
            _positionsNative = new NativeArray<Vector3>(_positions, Allocator.Persistent);
            _velocitiesNative = new NativeArray<Vector3>(_velocities, Allocator.Persistent);
            _finalPositions = new NativeArray<Vector3>(_positions.Length, Allocator.Persistent);

            Debug.LogWarning("Create new MyTaskTwo");
            _myTask = new MyTaskTwo
            {
                PositionsNative = _positionsNative,
                VelocitiesNative = _velocitiesNative,
                FinalPositions = _finalPositions,
                DeltaTime = Time.deltaTime,
            };
            
            _frames = Time.frameCount;

            Debug.LogWarning("_myTask Scheduled");
            _jobHandle = _myTask.Schedule(_positions.Length, 32);
            
            // Раньше тут был _jobHandle.Complete();
            
            _inProgress = true;
        }

        private void LateUpdate()
        {
            Debug.Log("Tick");// Сигнализатор того, что апдейт не подвис
        }

        private struct MyTaskTwo : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> PositionsNative;
            [ReadOnly] public NativeArray<Vector3> VelocitiesNative;
            [ReadOnly] public float DeltaTime;

            [WriteOnly] public NativeArray<Vector3> FinalPositions;

            public void Execute(int index)
            {
                Debug.LogWarning("Execute");// Сигнализатор выполнения задачи. Выводится всегда сразу пачкой
                FinalPositions[index] = PositionsNative[index] + VelocitiesNative[index] * DeltaTime;
            }
        }
    }
}
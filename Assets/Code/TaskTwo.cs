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

        [Space][SerializeField] private bool _randomize;
        [SerializeField] private int _arrayLength;

        [Space][SerializeField] private Vector3[] _positions;
        [SerializeField] private Vector3[] _velocities;

        private NativeArray<Vector3> _positionsNative;
        private NativeArray<Vector3> _velocitiesNative;
        private NativeArray<Vector3> _finalPositions;

        private int _frames;
        private bool _inProgress;
        private MyTaskTwo _myTask;
        private JobHandle _jobHandle;

        private void Update()
        {
            if (_randomize)
            {
                _randomize = false;
                CreateData();
            }

            if (_run)
            {
                _run = false;
                _inProgress = true;
                StartJob();
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
                _inProgress = false;
                _positionsNative.Dispose();
                _velocitiesNative.Dispose();

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
            _positionsNative = new NativeArray<Vector3>(_positions, Allocator.Persistent);
            _velocitiesNative = new NativeArray<Vector3>(_velocities, Allocator.Persistent);
            _finalPositions = new NativeArray<Vector3>(_positions.Length, Allocator.Persistent);

            _myTask = new MyTaskTwo
            {
                PositionsNative = _positionsNative,
                VelocitiesNative = _velocitiesNative,
                FinalPositions = _finalPositions,
                DeltaTime = Time.deltaTime,
            };

            _frames = Time.frameCount;
            _jobHandle = _myTask.Schedule(_positions.Length, 32);
            _jobHandle.Complete();
        }

        private void LateUpdate()
        {
            Debug.LogWarning("Tick");
        }

        private struct MyTaskTwo : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> PositionsNative;
            [ReadOnly] public NativeArray<Vector3> VelocitiesNative;
            [ReadOnly] public float DeltaTime;

            [WriteOnly] public NativeArray<Vector3> FinalPositions;

            public void Execute(int index)
            {
                //Debug.LogWarning($"Index = {index}");
                FinalPositions[index] = PositionsNative[index] + VelocitiesNative[index] * DeltaTime;
            }
        }
    }
}
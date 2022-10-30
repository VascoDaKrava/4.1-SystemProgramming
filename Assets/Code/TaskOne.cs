using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


namespace SystemProgramming.Lesson2Jobs
{
    public sealed class TaskOne : MonoBehaviour
    {
        //        ����� 1: �������� ������ ���� IJob, ������� ��������� ������ � �������
        //  NativeArray<int> � � ���������� ���������� ��� �������� ����� ������ ������
        //  ������� ����.
        //  �������� ���������� ���� ������ �� �������� ������ � �������� � �������
        //  ���������.

        [SerializeField] private int[] _intArray;
        [SerializeField] private int _zeroFactor = 10;
        [SerializeField] private bool _run;

        private bool _inProgress;
        private NativeArray<int> _intNative;
        private MyTask _myTask;
        private JobHandle _jobHandle;

        private void Update()
        {
            if (_run)
            {
                _run = false;
                _inProgress = true;
                _intNative = new NativeArray<int>(_intArray, Allocator.Persistent);

                _myTask = new MyTask
                {
                    IntArrayIn = _intNative,
                    ZeroFactor = _zeroFactor
                };

                _jobHandle = _myTask.Schedule();
            }

            if (_jobHandle.IsCompleted && _inProgress)
            {
                _inProgress = false;
                _jobHandle.Complete();
                _intArray = _myTask.IntArrayIn.ToArray();
                
                foreach (var item in _intArray)
                {
                    Debug.Log($"{item}");
                }
                
                _intNative.Dispose();
            }
        }

        private struct MyTask : IJob
        {
            public NativeArray<int> IntArrayIn;
            public int ZeroFactor;

            public void Execute()
            {
                for (int i = 0; i < IntArrayIn.Length; i++)
                {
                    if (IntArrayIn[i] > ZeroFactor)
                    {
                        IntArrayIn[i] = 0;
                    }
                }
            }
        }
    }
}
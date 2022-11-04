using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;


namespace SystemProgramming.Lesson2Jobs
{
    public struct MoveJob : IJobParallelForTransform
    {
        [ReadOnly] public float DeltaTime;

        public NativeArray<Vector3> Positions;
        public NativeArray<Vector3> Velocities;
        public NativeArray<Vector3> Accelerations;

        public void Execute(int index, TransformAccess transform)
        {
            Vector3 velocity = Velocities[index] + Accelerations[index];
            transform.position += velocity * DeltaTime;
            Positions[index] = transform.position;
            Velocities[index] = velocity;
            Accelerations[index] = Vector3.zero;
        }
    }
}
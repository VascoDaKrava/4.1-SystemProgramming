using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;


namespace SystemProgramming.Lesson2Jobs
{
    public class Galaxy : MonoBehaviour
    {
        [SerializeField] private int _scaleQuantity;
        [SerializeField] private int _numberOfEntities;
        [SerializeField] private float _maxDistance;
        [SerializeField] private float _maxVelocity;
        [SerializeField] private float _maxMass;
        [SerializeField] private float _gravitationModifier;
        [SerializeField] private GameObject _celestialBodyPrefab;

        private NativeArray<Vector3> _positions;
        private NativeArray<Vector3> _velocities;
        private NativeArray<Vector3> _accelerations;
        private NativeArray<float> _masses;

        private TransformAccessArray _transformAccessArray;


        private void Start()
        {

            _positions = new NativeArray<Vector3>(_numberOfEntities, Allocator.Persistent);
            _velocities = new NativeArray<Vector3>(_numberOfEntities, Allocator.Persistent);
            _accelerations = new NativeArray<Vector3>(_numberOfEntities, Allocator.Persistent);

            _masses = new NativeArray<float>(_numberOfEntities, Allocator.Persistent);

            Transform[] transforms = new Transform[_numberOfEntities];

            for (int i = 0; i < _numberOfEntities; i++)
            {
                _positions[i] = Random.insideUnitSphere * Random.Range(0.01f, _maxDistance);
                _velocities[i] = Random.insideUnitSphere * Random.Range(0.01f, _maxVelocity);
                _accelerations[i] = Vector3.zero;

                _masses[i] = Random.Range(1, _maxMass);

                transforms[i] = Instantiate(_celestialBodyPrefab, _positions[i], Random.rotation).transform;
                transforms[i].GetComponent<Rigidbody>().mass = _masses[i];
                transforms[i].localScale = Vector3.one * Mathf.Clamp(_masses[i] / _maxMass * _scaleQuantity, 1.0f, _scaleQuantity);
            }

            _transformAccessArray = new TransformAccessArray(transforms);
        }

        private void Update()
        {
            GravitationJob gravitationJob = new()
            {
                Positions = _positions,
                Velocities = _velocities,
                Accelerations = _accelerations,
                Masses = _masses,
                GravitationModifier = _gravitationModifier,
                DeltaTime = Time.deltaTime
            };

            JobHandle gravitationHandle = gravitationJob.Schedule(_numberOfEntities, 0);

            MoveJob moveJob = new()
            {
                Positions = _positions,
                Velocities = _velocities,
                Accelerations = _accelerations,
                DeltaTime = Time.deltaTime
            };

            JobHandle moveHandle = moveJob.Schedule(_transformAccessArray, gravitationHandle);
            moveHandle.Complete();
        }

        private void OnDestroy()
        {
            _positions.Dispose();
            _velocities.Dispose();
            _accelerations.Dispose();
            _masses.Dispose();
            _transformAccessArray.Dispose();
        }
    }
}
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    [SerializeField] private int _particleCount = 1000;
    [SerializeField] private float3 _simulationBounds;
    [SerializeField, Range(0, 0.2f)] private float _diffusionalCoefficient = 0.1f;
    [SerializeField, Range(0, 0.1f)] private float _epsilon = 0.01f;
    [SerializeField, Range(0, 10)] private uint _solverIterations = 5;
    [SerializeField] private float _gravity = 9.81f;
    [Header("Particle Properties")]
    [SerializeField, Range(0, 1)] private float _neighbourRadius;
    [SerializeField, Range(0, 2)] private float _crCoeff = 0.5f;
    [Header("Rendering")]
    [SerializeField] private float _particleSize = 0.1f;
    [SerializeField] private Mesh _particleMesh;
    [SerializeField] private Material _particleMaterial;

    private NativeArray<Particle> _particles;
    private ExternalForcesJob _externalForcesJob;
    private FindNeighboursJob _findNeighboursJob;
    private PhaseJob _phaseJob;
    private DensityConstraintJob _densityConstraintJob;
    private UpdateParticlesJob _updateParticlesJob;
    private EnvironmentCollisionJob _environmentCollisionJob;

    private NativeArray<NativeList<int>> _neighbours;
    private float _timeScale = 1f;
    private SpatialHashGrid _spatialHashGrid;

    #region Lifecycle

    private void Start()
    {
        Time.timeScale = 0;
        ResetSimulation();
    }

    private void Update()
    {
        DrawParticles();
    }

    private void FixedUpdate()
    {
        _externalForcesJob.DeltaTime = Time.fixedDeltaTime;
        _findNeighboursJob.Radius = _neighbourRadius;
        _phaseJob.Radius = _neighbourRadius;
        _phaseJob.DiffusionalCoefficient = _diffusionalCoefficient;
        _phaseJob.Epsilon = _epsilon;
        _densityConstraintJob.DeltaTime = Time.fixedDeltaTime;
        _densityConstraintJob.Radius = _neighbourRadius;
        _updateParticlesJob.DeltaTime = Time.fixedDeltaTime;
        _environmentCollisionJob.CRCoeff = _crCoeff;

        JobHandle efHandle = _externalForcesJob.Schedule(_particles.Length, 64);
        efHandle.Complete();
        _spatialHashGrid.Clear();
        for (int i = 0; i < _particles.Length; i++)
        {
            _spatialHashGrid.AddParticle(i, _particles[i].PredictedPosition);
        }
        JobHandle fnHandle = _findNeighboursJob.Schedule(_particles.Length, 64, efHandle);
        fnHandle.Complete();
        _phaseJob.Execute();
        for (int i = 0; i < _solverIterations; i++)
        {
            _densityConstraintJob.Execute();
        }
        JobHandle upHandle = _updateParticlesJob.Schedule(_particles.Length, 64);
        JobHandle ecHandle = _environmentCollisionJob.Schedule(_particles.Length, 64, upHandle);
        ecHandle.Complete();
    }

    private void OnDestroy()
    {
        if (_particles.IsCreated)
        {
            _particles.Dispose();
        }
        if (_externalForcesJob.Forces.IsCreated)
        {
            _externalForcesJob.Forces.Dispose();
        }
        if (_neighbours.IsCreated)
        {
            for (int i = 0; i < _neighbours.Length; i++)
            {
                if (_neighbours[i].IsCreated)
                {
                    _neighbours[i].Dispose();
                }
            }
            _neighbours.Dispose();
        }
        _spatialHashGrid.Dispose();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        for (int i = 0; i < _particles.Length; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(_particles[i].Position, _particleSize);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_particles[i].Position, _particles[i].DensityConstraintGradient);
        }
    }

    #endregion

    #region UI Callbacks

    public void SetParticleCount(string count)
    {
        _particleCount = int.Parse(count);
    }

    public void SetTimeScale(float timeScale)
    {
        _timeScale = timeScale / 5;
    }

    public void SetDiffusionalCoefficient(float coef)
    {
        _diffusionalCoefficient = coef / 50;
    }

    public void SetEpsilon(float epsilon)
    {
        _epsilon = epsilon / 100;
    }

    public void SetSolverIterations(float iterations)
    {
        _solverIterations = (uint)iterations;
    }

    public void SetGravity(string gravity)
    {
        _gravity = float.Parse(gravity);
    }

    public void SetNeighbourRadius(float radius)
    {
        _neighbourRadius = radius / 10;
    }

    public void SetCRCoeff(float crCoeff)
    {
        _crCoeff = crCoeff / 5;
    }

    #endregion

    #region Public Methods

    public void PlayPause()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = _timeScale;
        }
        else
        {
            Time.timeScale = 0;
        }
    }

    [ContextMenu("Reset Simulation")]
    public void ResetSimulation()
    {
        Time.timeScale = 0;
        OnDestroy();
        _particles = new NativeArray<Particle>(_particleCount, Allocator.Persistent);
        _neighbours = new NativeArray<NativeList<int>>(_particles.Length, Allocator.Persistent);
        for (int i = 0; i < _neighbours.Length; i++)
        {
            _neighbours[i] = new NativeList<int>(Allocator.Persistent);
        }
        SetInitialParticles();

        _spatialHashGrid = new SpatialHashGrid(_neighbourRadius, _particles.Length, Allocator.Persistent);
        for (int i = 0; i < _particles.Length; i++)
        {
            _spatialHashGrid.AddParticle(i, _particles[i].Position);
        }

        _externalForcesJob = new ExternalForcesJob
        {
            Particles = _particles,
            Forces = new NativeArray<ForceEmitter>(1, Allocator.Persistent)
        };
        _externalForcesJob.Forces[0] = new ForceEmitter(ForceEmitter.Type.Gravity, new float3(0, -1, 0), _gravity);
        _findNeighboursJob = new FindNeighboursJob
        {
            Particles = _particles,
            Neighbours = _neighbours,
            SpatialHashGrid = _spatialHashGrid,
        };
        _phaseJob = new PhaseJob
        {
            Particles = _particles,
            Neighbours = _neighbours
        };
        _densityConstraintJob = new DensityConstraintJob
        {
            Particles = _particles,
            Neighbours = _neighbours
        };
        _updateParticlesJob = new UpdateParticlesJob
        {
            Particles = _particles
        };
        _environmentCollisionJob = new EnvironmentCollisionJob
        {
            Particles = _particles,
            SimulationBounds = _simulationBounds
        };
    }

    #endregion

    #region Private Methods

    private void SetInitialParticles()
    {
        int particlesPerAxis = Mathf.CeilToInt(Mathf.Pow(_particleCount, 1f / 3f));
        float spacingX = _simulationBounds.x / particlesPerAxis;
        float spacingY = _simulationBounds.y / particlesPerAxis;
        float spacingZ = _simulationBounds.z / particlesPerAxis;

        int index = 0;
        for (int x = 0; x < particlesPerAxis; x++)
        {
            for (int y = 0; y < particlesPerAxis; y++)
            {
                for (int z = 0; z < particlesPerAxis; z++)
                {
                    if (index >= _particleCount) return;

                    Particle particle = ParticleFactory.CreateParticle(0, ParticleFactory.ParticleType.Air);
                    particle.Position = new float3((x + 0.5f) * spacingX, (y + 0.5f) * spacingY, (z + 0.5f) * spacingZ) - _simulationBounds / 2;
                    _particles[index] = particle;
                    index++;
                }
            }
        }
    }

    private void DrawParticles()
    {
        if (!_particles.IsCreated) return;
        for (int i = 0; i < _particles.Length; i++)
        {
            Particle particle = _particles[i];
            Vector3 position = particle.Position;
            Quaternion rotation = Quaternion.LookRotation(position - Camera.main.transform.position);
            Graphics.DrawMesh(_particleMesh, Matrix4x4.TRS(position, rotation, Vector3.one * _particleSize), _particleMaterial, 0, Camera.main);
        }
    }

    #endregion
}

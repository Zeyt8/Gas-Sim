using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    [SerializeField] private int _particleCount = 1000;
    [SerializeField] private float3 _simulationBounds;
    [SerializeField] private float _diffusionalCoefficient = 0.1f;
    [SerializeField] private float _epsilon = 0.01f;
    [SerializeField] private uint _solverIterations = 5;
    [SerializeField] private float _gravity = 9.81f;
    [Header("Particle Properties")]
    [SerializeField] private float _neighbourRadius;
    [SerializeField] private float _crCoeff = 0.5f;
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

    #region Lifecycle

    private void Start()
    {
        ResetSimulation();
    }

    private void Update()
    {
        DrawParticles();
    }

    private void FixedUpdate()
    {
        _externalForcesJob.DeltaTime = Time.fixedDeltaTime;
        _densityConstraintJob.DeltaTime = Time.fixedDeltaTime;
        _updateParticlesJob.DeltaTime = Time.fixedDeltaTime;

        JobHandle efHandle = _externalForcesJob.Schedule(_particles.Length, 64);
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
        for (int i = 0; i < _particleCount; i++)
        {
            if (_neighbours[i].IsCreated)
            {
                _neighbours[i].Dispose();
            }
        }
        if (_neighbours.IsCreated)
        {
            _neighbours.Dispose();
        }
    }

    #endregion

    #region UI Callbacks

    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    #endregion

    #region Public Methods

    [ContextMenu("Reset Simulation")]
    public void ResetSimulation()
    {
        OnDestroy();
        _neighbours = new NativeArray<NativeList<int>>(_particleCount, Allocator.Persistent);
        for (int i = 0; i < _particleCount; i++)
        {
            _neighbours[i] = new NativeList<int>(Allocator.Persistent);
        }
        _particles = new NativeArray<Particle>(_particleCount, Allocator.Persistent);
        SetInitialParticles();

        _externalForcesJob = new ExternalForcesJob
        {
            Particles = _particles,
            Forces = new NativeArray<ForceEmitter>(1, Allocator.Persistent)
        };
        _findNeighboursJob = new FindNeighboursJob
        {
            Particles = _particles,
            Radius = _neighbourRadius,
            Neighbours = new NativeArray<NativeList<int>>(_particleCount, Allocator.Persistent)
        };
        //_externalForcesJob.Forces[0] = new ForceEmitter(ForceEmitter.Type.Gravity, new float3(0, -1, 0), 9.81f);
        _phaseJob = new PhaseJob
        {
            Particles = _particles,
            DiffusionalCoefficient = _diffusionalCoefficient,
            Epsilon = _epsilon,
        };
        _densityConstraintJob = new DensityConstraintJob
        {
            Particles = _particles,
            DeltaTime = Time.fixedDeltaTime,
            Radius = _neighbourRadius,
            Neighbours = _neighbours
        };
        _updateParticlesJob = new UpdateParticlesJob
        {
            Particles = _particles
        };
        _environmentCollisionJob = new EnvironmentCollisionJob
        {
            Particles = _particles,
            SimulationBounds = _simulationBounds,
            CRCoeff = _crCoeff
        };
    }

    #endregion

    #region Private Methods

    private void SetInitialParticles()
    {
        int particlesPerAxis = Mathf.CeilToInt(Mathf.Pow(_particleCount, 1f / 3f));
        float spacingX = _simulationBounds.x / particlesPerAxis / 2;
        float spacingY = _simulationBounds.y / particlesPerAxis / 2;
        float spacingZ = _simulationBounds.z / particlesPerAxis / 2;

        int index = 0;
        for (int x = 0; x < particlesPerAxis; x++)
        {
            for (int y = 0; y < particlesPerAxis; y++)
            {
                for (int z = 0; z < particlesPerAxis; z++)
                {
                    if (index >= _particleCount) return;

                    Particle particle = ParticleFactory.CreateParticle(0, ParticleFactory.ParticleType.Air, _particleCount, _simulationBounds.x * _simulationBounds.y * _simulationBounds.z);
                    particle.Position = new float3((x + 0.5f) * spacingX, (y + 0.5f) * spacingY, (z + 0.5f) * spacingZ) - _simulationBounds / 2;
                    _particles[index] = particle;
                    index++;
                }
            }
        }
    }

    private void DrawParticles()
    {
        for (int i = 0; i < _particleCount; i++)
        {
            Particle particle = _particles[i];
            Vector3 position = particle.Position;
            Quaternion rotation = Quaternion.LookRotation(position - Camera.main.transform.position);
            //Material mat = new Material(_particleMaterial);
            //mat.color = new Color(particle.Density / (2 * particle.RestDensity), 0, 0);
            Graphics.DrawMesh(_particleMesh, Matrix4x4.TRS(position, rotation, Vector3.one * _particleSize), _particleMaterial, 0, Camera.main);
        }
    }

    #endregion
}

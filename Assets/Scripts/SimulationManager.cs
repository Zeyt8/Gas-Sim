using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    [SerializeField] private int _particleCount = 1000;
    [SerializeField] private float3 _simulationBounds;
    [SerializeField] private int _solverIterations = 5;
    [SerializeField] private float _gravity = 9.81f;
    [Header("Particle Properties")]
    [SerializeField] private float _neighbourRadius;
    [SerializeField] private float _drag = 0.99f;
    [SerializeField] private float _collisionDamping = 0.5f;
    [Header("Rendering")]
    [SerializeField] private float _particleSize = 0.1f;
    [SerializeField] private Mesh _particleMesh;
    [SerializeField] private Material _particleMaterial;

    private NativeArray<Particle> _particles;
    private ExternalForcesJob _externalForcesJob;
    private FindNeighboursJob _findNeighboursJob;
    private PhaseJob _phaseJob;
    private UpdateParticlesJob _updateParticlesJob;
    private EnvironmentCollisionJob _environmentCollisionJob;

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
        _updateParticlesJob.DeltaTime = Time.fixedDeltaTime;

        JobHandle efHandle = _externalForcesJob.Schedule(_particles.Length, 64);
        JobHandle fnHandle = _findNeighboursJob.Schedule(_particles.Length, 64, efHandle);
        JobHandle phHandle = _phaseJob.Schedule(_particles.Length, 64, fnHandle);
        phHandle.Complete();
        for (int i = 0; i < _solverIterations; i++)
        {

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
    }

    #endregion

    #region Public Methods

    [ContextMenu("Reset Simulation")]
    public void ResetSimulation()
    {
        if (_particles.IsCreated)
        {
            _particles.Dispose();
        }
        _particles = new NativeArray<Particle>(_particleCount, Allocator.Persistent);
        SetInitialParticles();

        if (_externalForcesJob.Forces.IsCreated)
        {
            _externalForcesJob.Forces.Dispose();
        }
        _externalForcesJob = new ExternalForcesJob
        {
            Particles = _particles,
            Forces = new NativeArray<Force>(1, Allocator.Persistent)
        };
        _externalForcesJob.Forces[0] = new Force(new float3(0, -1, 0), _gravity);
        _findNeighboursJob = new FindNeighboursJob
        {
            Particles = _particles,
            Radius = _neighbourRadius
        };
        _phaseJob = new PhaseJob
        {
            Particles = _particles,
            DeltaTime = Time.fixedDeltaTime
        };
        _updateParticlesJob = new UpdateParticlesJob
        {
            Particles = _particles,
            Drag = _drag,
        };
        _environmentCollisionJob = new EnvironmentCollisionJob
        {
            Particles = _particles,
            SimulationBounds = _simulationBounds,
            CollisionDamping = _collisionDamping
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

                    Particle particle = ParticleFactory.CreateParticle(ParticleFactory.ParticleType.Air);
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
            Graphics.DrawMesh(_particleMesh, Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * _particleSize), _particleMaterial, 0, Camera.main);
        }
    }

    #endregion
}

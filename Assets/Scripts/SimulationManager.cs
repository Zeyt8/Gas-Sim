using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    [SerializeField] private int _particleCount = 1000;
    [SerializeField] private float3 _simulationSize;
    [SerializeField] private int _solverIterations = 5;
    [Header("Particle Properties")]
    [SerializeField] private float _smoothRadius;
    [Header("Rendering")]
    [SerializeField] private float _particleSize = 0.1f;
    [SerializeField] private Mesh _particleMesh;
    [SerializeField] private Material _particleMaterial;

    private NativeArray<Particle> _particles;
    private ExternalForcesJob _externalForcesJob;
    private FindNeighboursJob _findNeighboursJob;
    private PhaseJob _phaseJob;
    private UpdateParticlesJob _updateParticlesJob;

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
        _updateParticlesJob.DeltaTime = Time.fixedDeltaTime;

        JobHandle efHandle = _externalForcesJob.Schedule(_particles.Length, 64);
        JobHandle fnHandle = _findNeighboursJob.Schedule(_particles.Length, 64, efHandle);
        JobHandle phHandle = _phaseJob.Schedule(_particles.Length, 64, fnHandle);
        phHandle.Complete();
        for (int i = 0; i < _solverIterations; i++)
        {

        }
        JobHandle upHandle = _updateParticlesJob.Schedule(_particles.Length, 64);
        upHandle.Complete();
    }

    private void OnDestroy()
    {
        if (_particles.IsCreated)
        {
            _particles.Dispose();
        }
    }

    #endregion

    #region Public Methods

    [ContextMenu("Reset Simulation")]
    public void ResetSimulation()
    {
        _particles = new NativeArray<Particle>(_particleCount, Allocator.Persistent);
        SetInitialParticles();

        _externalForcesJob = new ExternalForcesJob
        {
            Particles = _particles
        };
        _findNeighboursJob = new FindNeighboursJob
        {
            Particles = _particles
        };
        _phaseJob = new PhaseJob
        {
        };
        _updateParticlesJob = new UpdateParticlesJob
        {
            Particles = _particles
        };
    }

    #endregion

    #region Private Methods

    private void SetInitialParticles()
    {
        int particlesPerAxis = Mathf.CeilToInt(Mathf.Pow(_particleCount, 1f / 3f));
        float spacingX = _simulationSize.x / particlesPerAxis;
        float spacingY = _simulationSize.y / particlesPerAxis;
        float spacingZ = _simulationSize.z / particlesPerAxis;

        int index = 0;
        for (int x = 0; x < particlesPerAxis; x++)
        {
            for (int y = 0; y < particlesPerAxis; y++)
            {
                for (int z = 0; z < particlesPerAxis; z++)
                {
                    if (index >= _particleCount) return;

                    Particle particle = new Particle
                    {
                        Position = new float3(x * spacingX, y * spacingY, z * spacingZ) - _simulationSize / 2,
                        Velocity = float3.zero,
                        Mass = 1f,
                        Density = 1f
                    };

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

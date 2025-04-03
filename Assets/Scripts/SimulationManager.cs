using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] private int _particleCount = 1000;

    private NativeArray<Particle> _particles;
    private ExternalForcesJob _externalForcesJob;
    private FindNeighboursJob _findNeighboursJob;
    private UpdateParticlesJob _updateParticlesJob;
    private DrawParticlesJob _drawParticlesJob;

    private void Start()
    {
        _particles = new NativeArray<Particle>(_particleCount, Allocator.Persistent);

        _externalForcesJob = new ExternalForcesJob
        {
            Particles = _particles
        };
        _findNeighboursJob = new FindNeighboursJob
        {
            Particles = _particles
        };
        _updateParticlesJob = new UpdateParticlesJob
        {
            Particles = _particles
        };
        _drawParticlesJob = new DrawParticlesJob
        {
            Particles = _particles
        };
    }

    private void FixedUpdate()
    {
        _updateParticlesJob.DeltaTime = Time.fixedDeltaTime;

        JobHandle jobHandle = _drawParticlesJob.Schedule(_particles.Length, 64);
        _externalForcesJob.Schedule(_particles.Length, 64).Complete();
        _findNeighboursJob.Schedule(_particles.Length, 64).Complete();
        _updateParticlesJob.Schedule(_particles.Length, 64).Complete();
        jobHandle.Complete();
    }

    private void OnDestroy()
    {
        if (_particles.IsCreated)
        {
            _particles.Dispose();
        }
    }
}

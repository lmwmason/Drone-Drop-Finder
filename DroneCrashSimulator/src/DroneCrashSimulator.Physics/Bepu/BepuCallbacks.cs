using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using BepuSim = BepuPhysics.Simulation;

namespace DroneCrashSimulator.Physics.Bepu;

public struct DroneForceCallbacks : IPoseIntegratorCallbacks
{
    public readonly AngularIntegrationMode AngularIntegrationMode =>
        AngularIntegrationMode.Nonconserving;

    public readonly bool AllowSubstepsForUnconstrainedBodies => true;
    public readonly bool IntegrateVelocityForKinematics => false;

    public void Initialize(BepuSim simulation) { }

    public void PrepareForIntegration(float dt) { }

    public void IntegrateVelocity(
        Vector<int> bodyIndices,
        Vector3Wide position,
        QuaternionWide orientation,
        BodyInertiaWide localInertia,
        Vector<int> integrationMask,
        int workerIndex,
        Vector<float> dt,
        ref BodyVelocityWide velocity)
    {
    }
}

public struct NoCollisionCallbacks : INarrowPhaseCallbacks
{
    public void Initialize(BepuSim simulation) { }

    public bool AllowContactGeneration(
        int workerIndex,
        CollidableReference a,
        CollidableReference b,
        ref float speculativeMargin) => false;

    public bool AllowContactGeneration(
        int workerIndex,
        CollidablePair pair,
        int childIndexA,
        int childIndexB) => false;

    public bool ConfigureContactManifold<TManifold>(
        int workerIndex,
        CollidablePair pair,
        ref TManifold manifold,
        out PairMaterialProperties pairMaterial)
        where TManifold : unmanaged, IContactManifold<TManifold>
    {
        pairMaterial = default;
        return false;
    }

    public bool ConfigureContactManifold(
        int workerIndex,
        CollidablePair pair,
        int childIndexA,
        int childIndexB,
        ref ConvexContactManifold manifold) => false;

    public void Dispose() { }
}

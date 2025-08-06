namespace Quantum
{
	using Photon.Deterministic;
	using Quantum.Collections;

	/// <summary>
	/// Teleport moves the KCC upon collision to a specific destination defined by NavigationSource entity.
	/// </summary>
	public unsafe class TeleportProcessor : KCCProcessor
	{
		public override bool OnEnter(KCCContext context, KCCProcessorInfo processorInfo, KCCOverlapHit overlapHit)
		{
			if (processorInfo.HasEntity == false || context.Frame.TryGet<NavigationSource>(processorInfo.Entity, out NavigationSource navigationSource) == false)
				return false;

			QList<EntityRef> targets = context.Frame.ResolveList(navigationSource.Targets);
			if (targets.Count == 0)
				return false;

			int targetIndex = context.Frame.RNG->Next(0, targets.Count);
			if (context.Frame.TryGet<Transform3D>(targets[targetIndex], out Transform3D navigationTarget) == false)
				return false;

			// Clear kinematic and dynamic velocity entirely.
			context.KCC->SetKinematicVelocity(FPVector3.Zero);
			context.KCC->SetDynamicVelocity(FPVector3.Zero);

			context.KCC->Teleport(context.Frame, navigationTarget.Position);
			context.KCC->SetLookRotation(navigationTarget.Rotation, true, false);

			return true;
		}
	}
}

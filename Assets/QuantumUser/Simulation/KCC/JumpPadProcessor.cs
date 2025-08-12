namespace Quantum
{
	using Photon.Deterministic;
	using Quantum.Collections;

	/// <summary>
	/// JumpPad adds an impulse to a player KCC upon collision.
	/// </summary>
	public unsafe class JumpPadProcessor : KCCProcessor, IBeforeMove
	{
		// This processor has higher priority and will be executed first.
		public override FP GetPriority(KCCContext context, KCCProcessorInfo processorInfo) => 1000;

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

			// Explicitly set current position, this kills any remaining movement (CCD might be active).
			context.KCC->Teleport(context.Frame, context.KCC->Data.TargetPosition);

			// Force un-ground KCC.
			context.KCC->Data.IsGrounded = false;

			// Calculate how long it takes to reach apex.
			FPVector3 offset   = navigationTarget.Position - context.KCC->Data.TargetPosition;
			FP        apexTime = FPMath.Sqrt(-2 * offset.Y / context.KCC->Data.Gravity.Y) + context.KCC->Data.DeltaTime * FP._0_50;

			// Calculate initial velocity.
			FPVector3 velocity = new FPVector3(offset.X, 0, offset.Z) / apexTime - new FPVector3(0, context.KCC->Data.Gravity.Y, 0) * apexTime;

			context.KCC->SetDynamicVelocity(velocity);

			// Returning true = the KCC starts tracking collision with this processor/collider.
			// The OnExit() callback will be called when the KCC leaves (probably next frame).
			return true;
		}

		public override bool OnExit(KCCContext context, KCCProcessorInfo processorInfo)
		{
			// The interaction will be stopped after landing. Meantime other processors will be suppressed from BeforeMove(), effectively ignoring all other movement.
			return context.KCC->Data.IsGrounded == true && context.KCC->Data.WasGrounded == true;
		}

		public void BeforeMove(KCCContext context, KCCProcessorInfo processorInfo)
		{
			// Applying gravity.
			context.KCC->Data.DynamicVelocity += context.KCC->Data.Gravity * context.KCC->Data.DeltaTime;

			// All transient properties are consumed.
			context.KCC->Data.ExternalDelta   = default;
			context.KCC->Data.ExternalForce   = default;
			context.KCC->Data.ExternalImpulse = default;

			// Suppress all other processors.
			context.StageInfo.SuppressProcessors<KCCProcessor>();
		}
	}
}

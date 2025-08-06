namespace Quantum
{
	using Photon.Deterministic;

	/// <summary>
	/// Example processor - applying external force to attract the KCC to a specific target.
	/// </summary>
	public unsafe class AttractorProcessor : KCCProcessor, IBeforeMove
	{
		public FPAnimationCurve Curve;
		public FP Force;

		public void BeforeMove(KCCContext context, KCCProcessorInfo processorInfo)
		{
			if (processorInfo.HasEntity == false)
				return;
			if (context.Frame.TryGet<Transform3D>(processorInfo.Entity, out Transform3D transform) == false)
				return;
			if (context.Frame.TryGet<PhysicsCollider3D>(processorInfo.Entity, out PhysicsCollider3D physicsCollider) == false)
				return;

			// Calculate direction and power of the attraction.

			FPVector3 direction = transform.Position - context.KCC->Data.BasePosition;
			FP        distance  = direction.Magnitude;
			FP        power     = 0;

			if (distance > FP.EN3)
			{
				direction /= distance;
				power = Force * Curve.Evaluate(FPMath.Clamp01(distance / physicsCollider.Shape.Sphere.Radius));
			}

			// Apply calculated force.
			context.KCC->AddExternalForce(direction * power);
		}
	}
}

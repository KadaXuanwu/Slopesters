namespace Quantum
{
	using Photon.Deterministic;

	/// <summary>
	/// A simpler alternative to EnvironmentProcessor, based on different move parameters.
	/// This is used for NPCs, but can be used also for players.
	/// </summary>
	public unsafe class SimpleMoveProcessor : KCCProcessor, IBeforeMove, IAfterMoveStep
	{
		public FP UpGravity          = 15;
		public FP DownGravity        = 25;
		public FP GroundAcceleration = 55;
		public FP GroundDeceleration = 25;
		public FP AirAcceleration    = 25;
		public FP AirDeceleration    = FP._1 + FP._0_20 + FP._0_10;

		public void BeforeMove(KCCContext context, KCCProcessorInfo processorInfo)
		{
			KCCData data = context.KCC->Data;

			// Set default properties for correct depenetration.
			data.MaxGroundAngle = 60;
			data.MaxWallAngle   = 5;
			data.MaxHangAngle   = 30;

			// It feels better when player falls quicker.
			data.Gravity = new FPVector3(0, data.RealVelocity.Y >= 0 ? -UpGravity : -DownGravity, 0);

			// Reusing some of the EnvironmentProcessor logic, calculating KCCData.DynamicVelocity.
			EnvironmentProcessor.SetDynamicVelocity(context, ref data, 1, GroundDeceleration, AirDeceleration);

			FP acceleration;

			if (data.InputDirection == FPVector3.Zero)
			{
				// No desired move velocity - we are stopping.
				acceleration = data.IsGrounded ? GroundDeceleration : AirDeceleration;
			}
			else
			{
				acceleration = data.IsGrounded ? GroundAcceleration : AirAcceleration;
			}

			data.KinematicVelocity = FPVector3.Lerp(data.KinematicVelocity, data.InputDirection * data.KinematicSpeed, acceleration * context.Frame.DeltaTime);

			context.KCC->Data = data;
		}

		public void AfterMoveStep(KCCContext context, KCCProcessorInfo processorInfo, KCCOverlapInfo overlapInfo)
		{
			// Reusing move post-processing from the EnvironmentProcessor.
			EnvironmentProcessor.ProcessAfterMoveStep(context, processorInfo, overlapInfo);
		}
	}
}

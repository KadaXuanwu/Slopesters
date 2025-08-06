namespace Quantum
{
	using Photon.Deterministic;

	/// <summary>
	/// Adds offset to KCC position before move and reverts back on the end.
	/// This approach keeps KCC and Transform3D origin in sync.
	/// </summary>
	public unsafe class PositionOffsetProcessor : KCCProcessor, IBeforeMove, IAfterMove
	{
		public FPVector3 PositionOffset = FPVector3.Zero;

		public void BeforeMove(KCCContext context, KCCProcessorInfo processorInfo)
		{
			context.KCC->Data.BasePosition    += PositionOffset;
			context.KCC->Data.DesiredPosition += PositionOffset;
			context.KCC->Data.TargetPosition  += PositionOffset;

			// Optionally you can modify also GroundPosition if needed.
			/*if (context.KCC->Data.IsGrounded == true)
			{
				context.KCC->Data.GroundPosition += PositionOffset;
			}*/
		}

		public void AfterMove(KCCContext context, KCCProcessorInfo processorInfo)
		{
			// We have to revert the position offset change because the KCCData.TargetPosition propagates to Transform3D after move.

			context.KCC->Data.BasePosition    -= PositionOffset;
			context.KCC->Data.DesiredPosition -= PositionOffset;
			context.KCC->Data.TargetPosition  -= PositionOffset;

			// Optionally you can modify also GroundPosition if needed.
			/*if (context.KCC->Data.IsGrounded == true)
			{
				context.KCC->Data.GroundPosition -= PositionOffset;
			}*/
		}
	}
}

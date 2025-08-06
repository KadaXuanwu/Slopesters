namespace Quantum
{
	using Photon.Deterministic;

	/// <summary>
	/// Example processor - setting custom gravity (absolute value or multiplier).
	/// This processor uses interface defined by EnvironmentProcessor and works only if it is active.
	/// </summary>
	public unsafe class GravityProcessor : KCCProcessor, EnvironmentProcessor.IPrepareData
	{
		public bool IsMultiplier    = true;
		public FP   GravityModifier = 1;

		public void PrepareData(KCCContext context, KCCProcessorInfo processorInfo)
		{
			if (IsMultiplier == true)
			{
				context.KCC->Data.Gravity *= GravityModifier;
			}
			else
			{
				context.KCC->Data.Gravity = new FPVector3(0, GravityModifier, 0);
			}
		}
	}
}

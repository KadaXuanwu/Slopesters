namespace Quantum
{
	using Quantum.Collections;

	/// <summary>
	/// This processor registers KCC entity to Platform to receive position deltas.
	/// </summary>
	public unsafe class PlatformProcessor : KCCProcessor
	{
		public override bool OnEnter(KCCContext context, KCCProcessorInfo processorInfo, KCCOverlapHit overlapHit)
		{
			Platform* platform = context.Frame.Unsafe.GetPointer<Platform>(processorInfo.Entity);

			QHashSet<EntityRef> platformEntities = context.Frame.ResolveHashSet(platform->Entities);
			platformEntities.Add(context.Entity);

			return true;
		}

		public override bool OnExit(KCCContext context, KCCProcessorInfo processorInfo)
		{
			Platform* platform = context.Frame.Unsafe.GetPointer<Platform>(processorInfo.Entity);

			QHashSet<EntityRef> platformEntities = context.Frame.ResolveHashSet(platform->Entities);
			platformEntities.Remove(context.Entity);

			return true;
		}
	}
}

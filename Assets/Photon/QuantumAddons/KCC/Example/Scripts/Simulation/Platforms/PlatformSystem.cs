namespace Quantum
{
	using Photon.Deterministic;
	using Quantum.Collections;
	using UnityEngine.Scripting;

	/// <summary>
	/// Platform system moves all platforms between waypoints and propagates the movement to entities attached to them (registration is maintained by PlatformProcessor).
	/// </summary>
	[Preserve]
	public unsafe class PlatformSystem : SystemMainThreadFilter<PlatformSystem.Filter>, ISignalOnComponentAdded<Platform>, ISignalOnComponentRemoved<Platform>
	{
		public struct Filter
		{
			public EntityRef    Entity;
			public Transform3D* Transform;
			public Platform*    Platform;
		}

		public override void Update(Frame frame, ref Filter filter)
		{
			Transform3D* transform = filter.Transform;
			Platform*    platform  = filter.Platform;

			platform->CurrentDelay = FPMath.Max(0, platform->CurrentDelay - frame.DeltaTime);
			if (platform->CurrentDelay > 0)
				return;

			QList<EntityRef> waypoints = frame.ResolveList(platform->Waypoints);
			if (waypoints.Count <= 1)
				return;

			FPVector3 basePosition       = transform->Position;
			FP        remainingLength    = platform->Speed * frame.DeltaTime;
			int       remainingWaypoints = waypoints.Count;

			while (remainingLength > 0 && remainingWaypoints > 0)
			{
				--remainingWaypoints;

				Transform3D* currentWaypoint = frame.Unsafe.GetPointer<Transform3D>(waypoints[platform->CurrentWaypoint]);

				FPVector3 directionToWaypoint = FPVector3.Normalize(currentWaypoint->Position - transform->Position, out FP distanceToWaypoint);

				// If we are too close, let's update to next waypoint and continue.
				if (distanceToWaypoint <= 0)
				{
					bool hasSwitchedDirection = SetNextWaypoint(platform, waypoints);

					if (platform->Delay > 0 && hasSwitchedDirection == true)
					{
						platform->CurrentDelay = platform->Delay;
						break;
					}

					continue;
				}

				if (distanceToWaypoint > remainingLength)
				{
					transform->Position += directionToWaypoint * remainingLength;
					remainingLength = 0;
				}
				else
				{
					transform->Position += directionToWaypoint * remainingLength;
					remainingLength -= distanceToWaypoint;

					bool hasSwitchedDirection = SetNextWaypoint(platform, waypoints);

					if (platform->Delay > 0 && hasSwitchedDirection == true)
					{
						platform->CurrentDelay = platform->Delay;
						break;
					}
				}
			}

			// Visual entity has different collision setup compared to Platform entity. We need to synchronize position.
			Transform3D* visualTransform = frame.Unsafe.GetPointer<Transform3D>(platform->Visual);
			visualTransform->Position = transform->Position;

			FPVector3 positionDelta = transform->Position - basePosition;
			if (positionDelta != FPVector3.Zero)
			{
				// Propagate movement delta to all attached entities.

				QHashSet<EntityRef> platformEntities = frame.ResolveHashSet(platform->Entities);
				foreach (EntityRef platformEntity in platformEntities)
				{
					if (frame.Unsafe.TryGetPointer(platformEntity, out Transform3D* platformEntityTransform) == true)
					{
						platformEntityTransform->Position += positionDelta;
					}
				}
			}
		}

		void ISignalOnComponentAdded<Platform>.OnAdded(Frame frame, EntityRef entity, Platform* platform)
		{
			platform->Entities = frame.AllocateHashSet<EntityRef>();
		}

		void ISignalOnComponentRemoved<Platform>.OnRemoved(Frame frame, EntityRef entity, Platform* platform)
		{
			frame.FreeHashSet(ref platform->Entities);
		}

		private static bool SetNextWaypoint(Platform* platform, QList<EntityRef> waypoints)
		{
			if (platform->CurrentDirection == 0)
			{
				++platform->CurrentWaypoint;
				if (platform->CurrentWaypoint >= waypoints.Count)
				{
					platform->CurrentWaypoint  = waypoints.Count - 2;
					platform->CurrentDirection = 1;

					return true;
				}
			}
			else
			{
				--platform->CurrentWaypoint;
				if (platform->CurrentWaypoint < 0)
				{
					platform->CurrentWaypoint  = 1;
					platform->CurrentDirection = 0;

					return true;
				}
			}

			return false;
		}
	}
}

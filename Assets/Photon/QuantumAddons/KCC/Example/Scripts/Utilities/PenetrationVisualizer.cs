namespace Quantum
{
	using Photon.Deterministic;
	using Quantum.Physics3D;
	using UnityEngine;

	public sealed class PenetrationVisualizer : MonoBehaviour
	{
		public float           MaxGroundAngle = 60.0f;
		public CapsuleCollider Collider;
		public Transform       Origin;
		public Transform       Target;

		private void OnDrawGizmos()
		{
			QuantumRunner runner = QuantumRunner.Default;
			if (runner == null)
				return;
			if (runner.Session == null)
				return;

			Frame frame = runner.Session.FrameVerified as Frame;
			if (frame == null)
				return;

			if (Target != null)
			{
				Target.position = transform.position;
			}

			OverlapCapsule(frame, transform.position, Collider.radius, Collider.height, MaxGroundAngle, true, Origin, Target);
		}

		private static void OverlapCapsule(Frame frame, Vector3 position, float radius, float height, float maxGroundAngle, bool projectPenetration, Transform origin, Transform target)
		{
			FPVector3 positionDelta     = default;
			FP        minGroundDot      = FPMath.Cos(maxGroundAngle.ToFP() * FP.Deg2Rad);
			FP        minPenetrationDot = FPMath.Cos((90.0f - maxGroundAngle).ToFP() * FP.Deg2Rad);

			if (origin != null)
			{
				positionDelta = (position - origin.position).ToFPVector3();
			}

			float   extent         = height * 0.5f - radius;
			Vector3 bottomPosition = position - Vector3.up * height * 0.5f;

			Gizmos.color = Color.white;
			Gizmos.DrawLine(bottomPosition, bottomPosition + Quaternion.Euler(-maxGroundAngle, 0.0f, 0.0f) * Vector3.forward);
			Gizmos.DrawLine(bottomPosition, bottomPosition + Quaternion.Euler(maxGroundAngle, 0.0f, 0.0f) * Vector3.back);
			Gizmos.DrawLine(bottomPosition, bottomPosition + Quaternion.Euler(0.0f, 0.0f, -maxGroundAngle) * Vector3.left);
			Gizmos.DrawLine(bottomPosition, bottomPosition + Quaternion.Euler(0.0f, 0.0f, maxGroundAngle) * Vector3.right);

			Shape3D capsuleShape = Shape3D.CreateCapsule(radius.ToFP(), extent.ToFP());

			HitCollection3D hits = frame.Physics3D.OverlapShape(position.ToFPVector3(), FPQuaternion.Identity, capsuleShape, -1, QueryOptions.HitAll | QueryOptions.ComputeDetailedInfo);
			if (hits.Count <= 0)
				return;

			KCCPenetrationSolver penetrationSolver = KCCThreadStaticCache.Get<KCCPenetrationSolver>();
			penetrationSolver.Reset();

			for (int i = 0; i < hits.Count; ++i)
			{
				Hit3D hit = hits[i];

				bool    isValid            = true;
				Vector3 baseHitPoint       = hit.Point.ToUnityVector3();
				Vector3 baseHitNormal      = hit.Normal.ToUnityVector3();
				float   baseHitPenetration = hit.OverlapPenetration.AsFloat;

				if (hit.StaticColliderIndex >= 0 && hit.TriangleIndex >= 0)
				{
					bool forceProjectPenetration = projectPenetration == true && KCCPhysicsUtility.ShouldForceProjectPenetration(hits, hit.StaticColliderIndex, minPenetrationDot);

					isValid = KCCPhysicsUtility.ComputeMeshPenetration(ref hit, bottomPosition.ToFPVector3(), radius.ToFP(), height.ToFP(), projectPenetration, forceProjectPenetration, minGroundDot, positionDelta);
				}

				Gizmos.color = isValid == true ? Color.blue : Color.red;
				Gizmos.DrawLine(baseHitPoint, baseHitPoint + baseHitNormal * baseHitPenetration * 1.1f);

				if (isValid == false)
					continue;

				penetrationSolver.AddCorrection(hit.Normal, hit.OverlapPenetration);

				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(hit.Point.ToUnityVector3(), hit.Point.ToUnityVector3() + hit.Normal.ToUnityVector3() * hit.OverlapPenetration.AsFloat);
			}

			FPVector3 penetrationDirection = FPVector3.Normalize(penetrationSolver.CalculateBest(8, FP.EN3), out FP penetrationDistance);

			if (projectPenetration == true && penetrationDistance >= FP.EN4)
			{
				FP upDirectionDot = FPVector3.Dot(penetrationDirection, FPVector3.Up);
				if (upDirectionDot > FP._0 && upDirectionDot < minGroundDot)
				{
					FP movementDot = FPVector2.Dot(positionDelta.XZ, penetrationDirection.XZ);
					if (movementDot < FP._0)
					{
						KCCPhysicsUtility.ProjectVerticalPenetration(ref penetrationDirection, ref penetrationDistance);
					}
				}
			}

			Gizmos.color = Color.green;
			Gizmos.DrawLine(bottomPosition, bottomPosition + (penetrationDirection * penetrationDistance).ToUnityVector3());

			if (target != null)
			{
				target.localPosition = (penetrationDirection * penetrationDistance).ToUnityVector3();
			}

			penetrationSolver.Reset();
			KCCThreadStaticCache.Return(penetrationSolver);

			Gizmos.color = Color.white;
		}
	}
}

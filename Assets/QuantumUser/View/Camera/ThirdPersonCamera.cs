namespace Quantum
{
	using UnityEngine;

	public class ThirdPersonCamera : QuantumEntityViewComponent<SceneContext>
	{
		public Transform Pivot;
		public Transform Handle;

		private float _cameraDistance;

		public void Awake()
		{
			_cameraDistance = Handle.localPosition.z;
		}

		public override void OnLateUpdateView()
		{
			KCC kcc = GetPredictedQuantumComponent<KCC>();

			float lookPitch = kcc.Data.LookPitch.AsFloat;
			float lookYaw   = kcc.Data.LookYaw.AsFloat;

			if (EntityRef == ViewContext.LocalPlayerEntity)
			{
				// The simulation runs with a fixed tick rate which is not aligned with render rate.
				// For local player we also need to add look rotation accumulated since last fixed update to get super smooth look.
				Vector2 pendingLookRotationDelta = ViewContext.Input.GetPendingLookRotationDelta(EntityView.Game);
				lookPitch += pendingLookRotationDelta.x;
				lookYaw   += pendingLookRotationDelta.y;

				lookPitch = Mathf.Clamp(lookPitch, -90.0f, 90.0f);

				while (lookYaw >  180.0f) { lookYaw -= 360.0f; }
				while (lookYaw < -180.0f) { lookYaw += 360.0f; }

				// For local player we also set transform rotation => this overrides default interpolation.
				transform.rotation = Quaternion.Euler(0.0f, lookYaw, 0.0f);
			}

			// Pivot and Handle transforms are updated for every player.
			Pivot.localRotation  = Quaternion.Euler(lookPitch, 0.0f, 0.0f);
			Handle.localPosition = new Vector3(0.0f, 0.0f, _cameraDistance);

			if (EntityRef == ViewContext.LocalPlayerEntity)
			{
				// Only local player propagates Handle transform to main Camera.
				Handle.GetPositionAndRotation(out Vector3 cameraPosition, out Quaternion cameraRotation);
				Camera.main.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
			}
		}
	}
}

namespace Quantum
{
	using UnityEngine;

	/// <summary>
	/// Updates visual for local/remote players.
	/// </summary>
	public class PlayerView : QuantumEntityViewComponent<SceneContext>
	{
		public GameObject LocalPlayerVisual;
		public GameObject RemotePlayerVisual;

		public override void OnActivate(Frame frame)
		{
			Player player = GetPredictedQuantumComponent<Player>();

			bool isLocal = Game.PlayerIsLocal(player.PlayerRef);
			if (isLocal == true)
			{
				Cursor.visible   = false;
				Cursor.lockState = CursorLockMode.Locked;

				ViewContext.LocalPlayerView   = this;
				ViewContext.LocalPlayerEntity = EntityRef;
				ViewContext.LocalPlayer       = player.PlayerRef;
			}

			RefreshVisuals(isLocal);
		}

		public override void OnDeactivate()
		{
			if (ViewContext.LocalPlayerView == this)
			{
				ViewContext.LocalPlayerView = null;
				ViewContext.LocalPlayerEntity = EntityRef.None;
			}
		}

		private void RefreshVisuals(bool isLocal)
		{
			if (LocalPlayerVisual  != null) { LocalPlayerVisual.SetActive(isLocal);           }
			if (RemotePlayerVisual != null) { RemotePlayerVisual.SetActive(isLocal == false); }
		}
	}
}

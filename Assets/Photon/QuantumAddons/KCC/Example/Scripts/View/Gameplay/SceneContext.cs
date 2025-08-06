namespace Quantum
{
	using UnityEngine;

	/// <summary>
	/// Context for accessing general components from view scripts.
	/// </summary>
	public sealed class SceneContext : MonoBehaviour, IQuantumViewContext
	{
		public PlayerInput Input;

		public PlayerRef  LocalPlayer;
		public EntityRef  LocalPlayerEntity;
		public PlayerView LocalPlayerView;
	}
}

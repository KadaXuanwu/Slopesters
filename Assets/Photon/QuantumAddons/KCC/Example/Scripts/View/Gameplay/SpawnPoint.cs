namespace Quantum
{
	using UnityEngine;

	/// <summary>
	/// Usef for spawning players.
	/// </summary>
	public class SpawnPoint : MonoBehaviour
	{
		public SpawnPointData Bake()
		{
			SpawnPointData spawnPointData = new SpawnPointData();
			spawnPointData.Position = transform.position.ToFPVector3();
			spawnPointData.Rotation = transform.rotation.ToFPQuaternion();
			return spawnPointData;
		}
	}
}

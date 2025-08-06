namespace Quantum
{
	using UnityEngine;

	/// <summary>
	/// Bakes scene objects to UserAsset in Quantum Map asset.
	/// </summary>
	public class GameplayDataBaker : MapDataBakerCallback
	{
		public override void OnBeforeBake(QuantumMapData data)
		{
		}

		public override void OnBake(QuantumMapData data)
		{
#if UNITY_EDITOR
			GameplayData gameplayData = QuantumUnityDB.GetGlobalAssetEditorInstance<GameplayData>((Quantum.AssetRef)data.Asset.UserAsset.Id);

			SpawnPoint[] spawnPoints = GameObject.FindObjectsOfType<SpawnPoint>(true);
			gameplayData.SpawnPoints = new SpawnPointData[spawnPoints.Length];
			for (int i = 0; i < spawnPoints.Length; ++i)
			{
				gameplayData.SpawnPoints[i] = spawnPoints[i].Bake();
			}

			Waypoint[] waypoints = GameObject.FindObjectsOfType<Waypoint>(true);
			gameplayData.Waypoints = new WaypointData[waypoints.Length];
			for (int i = 0; i < waypoints.Length; ++i)
			{
				gameplayData.Waypoints[i] = waypoints[i].Bake();
			}

			UnityEditor.EditorUtility.SetDirty(gameplayData);

			Debug.Log($"Baked {spawnPoints.Length} spawn points.");
			Debug.Log($"Baked {waypoints.Length} waypoints.");
#endif
		}
	}
}

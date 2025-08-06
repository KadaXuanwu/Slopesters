namespace Quantum
{
	public unsafe partial class Frame
	{
		public EntityRef GetPlayerEntity(PlayerRef playerRef)
		{
			foreach (EntityComponentPair<Player> pair in GetComponentIterator<Player>())
			{
				if (pair.Component.PlayerRef == playerRef)
					return pair.Entity;
			}

			return EntityRef.None;
		}

		public SpawnPointData GetRandomSpawnPoint()
		{
			GameplayData gameplayData = FindAsset<GameplayData>(Map.UserAsset);
			return gameplayData.SpawnPoints[RNG->Next(0, gameplayData.SpawnPoints.Length)];
		}

		public WaypointData GetRandomWaypoint()
		{
			GameplayData gameplayData = FindAsset<GameplayData>(Map.UserAsset);
			return gameplayData.Waypoints[RNG->Next(0, gameplayData.Waypoints.Length)];
		}

#if UNITY_ENGINE
#endif
	}
}

namespace Quantum
{
	using System;
	using Photon.Deterministic;

	/// <summary>
	/// Contains data baked from scene objects.
	/// </summary>
	public unsafe partial class GameplayData : AssetObject
	{
		public SpawnPointData[] SpawnPoints;
		public WaypointData[]   Waypoints;
	}

	[Serializable]
	public struct SpawnPointData
	{
		public FPVector3    Position;
		public FPQuaternion Rotation;
	}

	[Serializable]
	public struct WaypointData
	{
		public FPVector3 Position;
	}
}

namespace Quantum
{
	using UnityEngine;

	/// <summary>
	/// Usef for simple NPC navigation.
	/// </summary>
	public class Waypoint : MonoBehaviour
	{
		public WaypointData Bake()
		{
			WaypointData waypointData = new WaypointData();
			waypointData.Position = transform.position.ToFPVector3();
			return waypointData;
		}
	}
}

namespace Quantum
{
	using UnityEngine.Scripting;

	/// <summary>
	/// Manages player spawning/despawning on connect/disconnect.
	/// </summary>
	[Preserve]
	public unsafe class GameplaySystem : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerRemoved
	{
		void ISignalOnPlayerAdded.OnPlayerAdded(Frame frame, PlayerRef playerRef, bool firstTime)
		{
			RuntimePlayer runtimePlayer = frame.GetPlayerData(playerRef);

			EntityRef playerEntity = frame.Create(runtimePlayer.PlayerAvatar);

			frame.AddOrGet<Player>(playerEntity, out var player);
			player->PlayerRef = playerRef;

			SpawnPointData spawnPoint = frame.GetRandomSpawnPoint();

			Transform3D* playerTransform = frame.Unsafe.GetPointer<Transform3D>(playerEntity);
			playerTransform->Position = spawnPoint.Position;
			playerTransform->Rotation = spawnPoint.Rotation;

			KCC* playerKCC = frame.Unsafe.GetPointer<KCC>(playerEntity);
			playerKCC->SetLookRotation(spawnPoint.Rotation.AsEuler.XY);
		}

		void ISignalOnPlayerRemoved.OnPlayerRemoved(Frame frame, PlayerRef playerRef)
		{
			EntityRef playerEntity = frame.GetPlayerEntity(playerRef);
			if (playerEntity.IsValid)
			{
				frame.Destroy(playerEntity);
			}
		}
	}
}

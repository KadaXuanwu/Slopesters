namespace Quantum
{
	using Photon.Deterministic;
	using UnityEngine.Scripting;

	/// <summary>
	/// Player system propagates player input to KCC.
	/// </summary>
	[Preserve]
	public unsafe class PlayerSystem : SystemMainThreadFilter<PlayerSystem.Filter>
	{
		public struct Filter
		{
			public EntityRef Entity;
			public Player*   Player;
			public KCC*      KCC;
		}

		public override void Update(Frame frame, ref Filter filter)
		{
			KCC*    kcc    = filter.KCC;
			Player* player = filter.Player;

			if (player->PlayerRef.IsValid == false)
				return;

			BasePlayerInput input = *frame.GetPlayerInput(player->PlayerRef);

			kcc->AddLookRotation(input.LookRotationDelta.X, input.LookRotationDelta.Y);
			kcc->SetInputDirection(kcc->Data.TransformRotation * input.MoveDirection.XOY);

			if (input.Jump.WasPressed == true && kcc->IsGrounded == true)
			{
				kcc->Jump(FPVector3.Up * player->JumpForce);
			}
		}
	}
}

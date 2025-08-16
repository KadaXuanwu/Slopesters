// Create this file at: Assets/QuantumUser/Simulation/Processors/SprintProcessor.cs

namespace Quantum
{
	using Photon.Deterministic;
	using UnityEngine;

	/// <summary>
	/// Handles sprinting mechanics for the KCC.
	/// </summary>
	[CreateAssetMenu(menuName = "Quantum/KCC/Processors/Sprint Processor")]
	public unsafe class SprintProcessor : KCCProcessor, IBeforeMove
	{
		[KCCHeader("Sprint Settings")]
		[KCCTooltip("Normal walking speed")]
		public FP WalkSpeed = 8;
		
		[KCCTooltip("Speed when sprinting")]
		public FP SprintSpeed = 12;
		
		[KCCTooltip("Sprint speed multiplier for air movement (0.8 = 80% of ground sprint speed)")]
		public FP AirSprintMultiplier = FP._0_50;
		
		[KCCTooltip("Minimum input magnitude required to sprint")]
		public FP MinInputMagnitudeForSprint = FP._0_50;

		/// <summary>
		/// This callback is invoked before KCC movement.
		/// </summary>
		public void BeforeMove(KCCContext context, KCCProcessorInfo processorInfo)
		{
			if (context.Frame.Unsafe.TryGetPointer(context.KCC->Entity, out Player* player) == false)
				return;
				
			if (player->PlayerRef.IsValid == false)
				return;

			BasePlayerInput input = *context.Frame.GetPlayerInput(player->PlayerRef);
			
			// Determine if we should sprint
			bool canSprint = input.Sprint.WasPressed 
				&& context.KCC->Data.InputDirection.Magnitude >= MinInputMagnitudeForSprint;
			
			// Calculate target speed
			FP targetSpeed = canSprint ? SprintSpeed : WalkSpeed;
			
			// Apply air movement penalty if not grounded
			if (!context.KCC->IsGrounded && canSprint)
			{
				targetSpeed *= AirSprintMultiplier;
			}
			
			// Set the kinematic speed
			context.KCC->SetKinematicSpeed(targetSpeed);
		}
	}
}
namespace Quantum
{
	using System.Runtime.InteropServices;
	using Photon.Deterministic;

	unsafe partial struct BasePlayerInput
	{
		public static implicit operator Input(BasePlayerInput playerInput)
		{
			Input input = default;

			input._a = playerInput.Jump;
			input.ThumbSticks.HighRes->_leftThumb = playerInput.MoveDirection;
			input.ThumbSticks.HighRes->_rightThumb.Pitch = playerInput.LookRotationDelta.X;
			input.ThumbSticks.HighRes->_rightThumb.Yaw   = playerInput.LookRotationDelta.Y;

			return input;
		}

		public static implicit operator BasePlayerInput(Input input)
		{
			BasePlayerInput playerInput = default;

			playerInput.Jump = input._a;
			playerInput.MoveDirection = input.ThumbSticks.HighRes->_leftThumb;
			playerInput.LookRotationDelta.X = input.ThumbSticks.HighRes->_rightThumb.Pitch;
			playerInput.LookRotationDelta.Y = input.ThumbSticks.HighRes->_rightThumb.Yaw;

			// There's a bug in operator FPVector2(InputDirectionMagnitude).
			// This is a temporary fix for InputDirectionMagnitude => FPVector2 conversion and will be removed in future version.
			ExposedInputDirectionMagnitude* leftThumb = (ExposedInputDirectionMagnitude*)(&input.ThumbSticks.HighRes->_leftThumb);
			if (leftThumb->EncodedAngle != default)
			{
				int angle     = ((int)leftThumb->EncodedAngle - 1) * 2;
				FP  magnitude = ((FP)leftThumb->EncodedMagnitude) / 255;

				playerInput.MoveDirection = FPVector2.Rotate(FPVector2.Up, angle * FP.Deg2Rad) * magnitude;
			}

			return playerInput;
		}

		public static FPVector2 GetPollLookRotationDelta(FPVector2 lookRotationDelta)
		{
			// This method "simulates" conversion/compression from full precision look rotation delta.
			// The resulting value represents value which is actually processed in simulation and should be subtracted from accumulated look rotation delta in input poller.

			InputPitchYaw inputPitchYaw = default;

			inputPitchYaw.Pitch = lookRotationDelta.X;
			inputPitchYaw.Yaw   = lookRotationDelta.Y;

			lookRotationDelta.X = inputPitchYaw.Pitch;
			lookRotationDelta.Y = inputPitchYaw.Yaw;

			return lookRotationDelta;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct ExposedInputDirectionMagnitude
		{
			[FieldOffset(0)]
			public byte EncodedAngle;
			[FieldOffset(1)]
			public byte EncodedMagnitude;
			[FieldOffset(0)]
			private int _padding;
		}
	}
}

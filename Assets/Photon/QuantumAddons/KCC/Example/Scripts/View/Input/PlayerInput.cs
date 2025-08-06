namespace Quantum
{
	using UnityEngine;
	using Photon.Deterministic;

	/// <summary>
	/// Handles player input.
	/// </summary>
	[DefaultExecutionOrder(-10)]
	public sealed class PlayerInput : MonoBehaviour
	{
		/// <summary>
		/// This is a special accumulator which accepts mouse/touch delta and returns smoothed,
		/// frame-aligned look rotation delta. It is essential to get super snappy butter-smooth experience.
		/// </summary>
		private Vector2Accumulator _lookRotationAccumulator = new Vector2Accumulator(0.02f, true);

		private BasePlayerInput _accumulatedInput;
		private bool            _resetAccumulatedInput;
		private int             _lastAccumulateFrame;
		private PolledInput[]   _polledInputs = new PolledInput[20];
		private InputTouches    _inputTouches = new InputTouches();
		private InputTouch      _moveTouch;
		private InputTouch      _lookTouch;
		private bool            _jumpTouch;
		private float           _jumpTime;

		/// <summary>
		/// This method returns any pending look rotation delta ahead of predicted frame. Takes into account Local Input Offset and render-aligned accumulations.
		/// </summary>
		public Vector2 GetPendingLookRotationDelta(QuantumGame game)
		{
			Vector2 pendingLookRotationDelta = default;

			for (int i = 0; i < game.Session.LocalInputOffset; ++i)
			{
				// To get responsive look rotation, we need to apply all pending inputs ahead of predicted tick => these can be delayed by local input offset.
				// For example if the LocalInputOffset == 2, PredictedFrame.Number == 100, inputs for ticks 101 and 102 are already polled and we should apply them as well.
				BasePlayerInput polledInput = GetInputForFrame(game.Frames.Predicted.Number + i + 1);
				pendingLookRotationDelta.x += polledInput.LookRotationDelta.X.AsFloat;
				pendingLookRotationDelta.y += polledInput.LookRotationDelta.Y.AsFloat;
			}

			// The simulation runs with a fixed tick rate which is not aligned with render rate.
			// For local player we also need to add look rotation accumulated since last fixed update to get super smooth look.
			// The _lookRotationAccumulator contains remaining delta since last polled input (which is always simulation tick aligned).
			pendingLookRotationDelta.x += _lookRotationAccumulator.AccumulatedValue.x;
			pendingLookRotationDelta.y += _lookRotationAccumulator.AccumulatedValue.y;

			return pendingLookRotationDelta;
		}

		private void OnEnable()
		{
			QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));

			_inputTouches.TouchStarted  = OnTouchStarted;
			_inputTouches.TouchFinished = OnTouchFinished;
		}

		private void OnDisable()
		{
			_inputTouches.TouchStarted  = null;
			_inputTouches.TouchFinished = null;
		}

		private void Update()
		{
			// Accumulate input every frame, even if Quantum doesn't simulate new frames (this happens when rendering rate is faster than Quantum simulation rate).
			AccumulateInput();
		}

		private void AccumulateInput()
		{
			// The accumulation must be processed once per Unity frame.
			// This method is triggered from Update() and PollInput() methods.
			if (_lastAccumulateFrame == Time.frameCount)
				return;

			_lastAccumulateFrame = Time.frameCount;

			if (_resetAccumulatedInput == true)
			{
				_resetAccumulatedInput = false;
				_accumulatedInput = default;
			}

			if (Application.isMobilePlatform == true && Application.isEditor == false)
			{
				_inputTouches.Update();

				ProcessMobileInput();
			}
			else
			{
				ProcessStandaloneInput();
			}
		}

		private void ProcessStandaloneInput()
		{
			// Enter key is used for locking/unlocking cursor in game view.
			if (UnityEngine.Input.GetKeyDown(KeyCode.Return) || UnityEngine.Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				if (Cursor.lockState == CursorLockMode.Locked)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
				else
				{
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
				}
			}

			// Accumulate input only if the cursor is locked.
			if (Cursor.lockState != CursorLockMode.Locked)
				return;

			Vector2 lookRotationDelta = new Vector2(-UnityEngine.Input.GetAxisRaw("Mouse Y"), UnityEngine.Input.GetAxisRaw("Mouse X"));
			_lookRotationAccumulator.Accumulate(lookRotationDelta);

			Vector2 moveDirection = Vector2.zero;

			if (UnityEngine.Input.GetKey(KeyCode.W)) { moveDirection += Vector2.up;    }
			if (UnityEngine.Input.GetKey(KeyCode.S)) { moveDirection += Vector2.down;  }
			if (UnityEngine.Input.GetKey(KeyCode.A)) { moveDirection += Vector2.left;  }
			if (UnityEngine.Input.GetKey(KeyCode.D)) { moveDirection += Vector2.right; }

			_accumulatedInput.MoveDirection = moveDirection.normalized.ToFPVector2();

			_accumulatedInput.Jump |= UnityEngine.Input.GetKey(KeyCode.Space);
		}

		private void ProcessMobileInput()
		{
			Vector2 moveDirection     = Vector2.zero;
			Vector2 lookRotationDelta = Vector2.zero;

			if (_lookTouch != null && _lookTouch.IsActive == true)
			{
				lookRotationDelta = new Vector2(-_lookTouch.Delta.Position.y, _lookTouch.Delta.Position.x);
				lookRotationDelta *= 0.25f; // Sensitivity
			}

			_lookRotationAccumulator.Accumulate(lookRotationDelta);

			if (_moveTouch != null && _moveTouch.IsActive == true && _moveTouch.GetDelta().Position.Equals(default) == false)
			{
				float screenSizeFactor = 8.0f / Mathf.Min(Screen.width, Screen.height);

				moveDirection = new Vector2(_moveTouch.GetDelta().Position.x, _moveTouch.GetDelta().Position.y) * screenSizeFactor;
				if (moveDirection.sqrMagnitude > 1.0f)
				{
					moveDirection.Normalize();
				}
			}

			_accumulatedInput.Jump         |= _jumpTouch;
			_accumulatedInput.MoveDirection = moveDirection.ToFPVector2();
		}

		private void OnTouchStarted(InputTouch touch)
		{
			if (_moveTouch == null && touch.Start.Position.x < Screen.width * 0.5f)
			{
				_moveTouch = touch;
			}

			if (_lookTouch == null && touch.Start.Position.x > Screen.width * 0.5f)
			{
				_lookTouch = touch;
				_jumpTouch = default;

				if (_jumpTime > Time.realtimeSinceStartup - 0.25f)
				{
					_jumpTouch = true;
				}

				_jumpTime = Time.realtimeSinceStartup;
			}
		}

		private void OnTouchFinished(InputTouch touch)
		{
			if (_moveTouch == touch) { _moveTouch = default; }
			if (_lookTouch == touch) { _lookTouch = default; _jumpTouch = default; }
		}

		private void PollInput(CallbackPollInput callback)
		{
			// PollInput() is called before Quantum simulation and before Update().
			// As a first step, we need to accumulate the input for this Unity frame.
			AccumulateInput();

			_resetAccumulatedInput = true;

			Vector2   consumeLookRotation = _lookRotationAccumulator.ConsumeFrameAligned(callback.Game);
			FPVector2 pollLookRotation    = BasePlayerInput.GetPollLookRotationDelta(consumeLookRotation.ToFPVector2());

			_lookRotationAccumulator.Add(consumeLookRotation - pollLookRotation.ToUnityVector2());

			_accumulatedInput.LookRotationDelta = pollLookRotation;

			_polledInputs[callback.Frame % _polledInputs.Length] = new PolledInput() { Frame = callback.Frame, Input = _accumulatedInput };
			callback.SetInput(_accumulatedInput, DeterministicInputFlags.Repeatable);
		}

		/// <summary>
		/// Local history of polled inputs. We need this to apply all pending look rotations in case there is an input offset active.
		/// </summary>
		private BasePlayerInput GetInputForFrame(int frame)
		{
			if (frame <= 0)
				return default;

			PolledInput polledInput = _polledInputs[frame % _polledInputs.Length];
			if (polledInput.Frame == frame)
				return polledInput.Input;

			return default;
		}

		private struct PolledInput
		{
			public int             Frame;
			public BasePlayerInput Input;
		}
	}
}

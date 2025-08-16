namespace Quantum
{
	using UnityEngine;
	using UnityEngine.InputSystem;
	using Photon.Deterministic;

	/// <summary>
	/// Handles player input with new Input System.
	/// </summary>
	[DefaultExecutionOrder(-10)]
	public sealed class PlayerInput : MonoBehaviour
	{
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

		// New Input System references
		private InputSystem_Actions _inputActions;
		private Vector2 _moveInput;
		private Vector2 _lookInput;
		private bool _jumpInput;
		private bool _sprintInput;
		private bool _isCursorLocked = true;

		public Vector2 GetPendingLookRotationDelta(QuantumGame game)
		{
			Vector2 pendingLookRotationDelta = default;

			for (int i = 0; i < game.Session.LocalInputOffset; ++i)
			{
				BasePlayerInput polledInput = GetInputForFrame(game.Frames.Predicted.Number + i + 1);
				pendingLookRotationDelta.x += polledInput.LookRotationDelta.X.AsFloat;
				pendingLookRotationDelta.y += polledInput.LookRotationDelta.Y.AsFloat;
			}

			pendingLookRotationDelta.x += _lookRotationAccumulator.AccumulatedValue.x;
			pendingLookRotationDelta.y += _lookRotationAccumulator.AccumulatedValue.y;

			return pendingLookRotationDelta;
		}

		private void Awake()
		{
			_inputActions = new InputSystem_Actions();
			
			// Subscribe to input events
			_inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
			_inputActions.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
			
			_inputActions.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
			_inputActions.Player.Look.canceled += ctx => _lookInput = Vector2.zero;
			
			_inputActions.Player.Jump.performed += ctx => _jumpInput = true;
			_inputActions.Player.Jump.canceled += ctx => _jumpInput = false;
			
			// Add sprint input handling
			_inputActions.Player.Sprint.performed += ctx => _sprintInput = true;
			_inputActions.Player.Sprint.canceled += ctx => _sprintInput = false;
			
			_inputActions.Player.ToggleCursor.performed += ctx => ToggleCursorLock();
		}

		private void OnEnable()
		{
			_inputActions?.Enable();
			
			QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));

			_inputTouches.TouchStarted  = OnTouchStarted;
			_inputTouches.TouchFinished = OnTouchFinished;
		}

		private void OnDisable()
		{
			_inputActions?.Disable();
			
			_inputTouches.TouchStarted  = null;
			_inputTouches.TouchFinished = null;
		}

		private void OnDestroy()
		{
			_inputActions?.Dispose();
		}

		private void Update()
		{
			AccumulateInput();
		}

		private void ToggleCursorLock()
		{
			_isCursorLocked = !_isCursorLocked;
			
			if (_isCursorLocked)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		private void AccumulateInput()
		{
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
			// Only accumulate input if cursor is locked
			if (!_isCursorLocked)
				return;

			// Look input - invert Y axis to match legacy behavior
			Vector2 lookDelta = new Vector2(-_lookInput.y, _lookInput.x);
			_lookRotationAccumulator.Accumulate(lookDelta);

			// Move input normalized
			_accumulatedInput.MoveDirection = _moveInput.normalized.ToFPVector2();

			// Jump input
			_accumulatedInput.Jump |= _jumpInput;

			// Sprint input
			_accumulatedInput.Sprint |= _sprintInput;
		}

		private void ProcessMobileInput()
		{
			Vector2 moveDirection     = Vector2.zero;
			Vector2 lookRotationDelta = Vector2.zero;

			if (_lookTouch != null && _lookTouch.IsActive == true)
			{
				lookRotationDelta = new Vector2(-_lookTouch.Delta.Position.y, _lookTouch.Delta.Position.x);
				lookRotationDelta *= 0.1f; // Sensitivity
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
			// Note: Sprint on mobile could be implemented with a double-tap or UI button
			// For now, no sprint on mobile unless you add a UI button
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
			AccumulateInput();

			_resetAccumulatedInput = true;

			Vector2 consumeLookRotation = _lookRotationAccumulator.ConsumeFrameAligned(callback.Game);
			FPVector2 pollLookRotation = BasePlayerInput.GetPollLookRotationDelta(consumeLookRotation.ToFPVector2());

			_lookRotationAccumulator.Add(consumeLookRotation - pollLookRotation.ToUnityVector2());

			_accumulatedInput.LookRotationDelta = pollLookRotation;

			_polledInputs[callback.Frame % _polledInputs.Length] = new PolledInput() { Frame = callback.Frame, Input = _accumulatedInput };
			callback.SetInput(_accumulatedInput, DeterministicInputFlags.Repeatable);
		}

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
namespace Quantum {
  using Photon.Deterministic;
  unsafe partial struct QuantumDemoInputShooter3D {
    public static implicit operator Input(QuantumDemoInputShooter3D sInput) {
      Input input = default;
      input._a = sInput.Jump;
      input._b = sInput.Dash;
      input._c = sInput.Fire;
      input._d = sInput.AltFire;
      input._r1 = sInput.Use;

      input.ThumbSticks.HighRes->_leftThumb = sInput.MoveDirection;

      // higher res right thumbstick (pitch yaw deltas)
      input.ThumbSticks.HighRes->_rightThumb.Pitch = sInput.Pitch;
      input.ThumbSticks.HighRes->_rightThumb.Yaw = sInput.Yaw;
      
      return input;
    }

    public static implicit operator QuantumDemoInputShooter3D(Input input) {
      QuantumDemoInputShooter3D sInput = default;
      sInput.Jump = input._a;
      sInput.Dash = input._b;
      sInput.Fire = input._c;
      sInput.AltFire = input._d;
      sInput.Use = input._r1;

      sInput.MoveDirection = input.ThumbSticks.Regular->_leftThumb;

      sInput.Yaw = input.ThumbSticks.HighRes->_rightThumb.Yaw;
      sInput.Pitch = input.ThumbSticks.HighRes->_rightThumb.Pitch;
      return sInput;
    }
    // use this as projectile orientation upon firing
    public FPQuaternion LookRotation => FPQuaternion.Euler(Pitch, Yaw, FP._0);
  }
}
namespace Quantum {
  using Photon.Deterministic;
  unsafe partial struct QuantumDemoInputTopDown {

    public static implicit operator Input(QuantumDemoInputTopDown tInput) {
      Input input = default;
      input._left = tInput.Left;
      input._right = tInput.Right;
      input._up = tInput.Up;
      input._down = tInput.Down;
      input._a = tInput.Jump;
      input._b = tInput.Dash;
      input._c = tInput.Fire;
      input._d = tInput.AltFire;
      input._r1 = tInput.Use;

      input.ThumbSticks.Regular->_rightThumb = tInput.AimDirection;
      input.ThumbSticks.Regular->_leftThumb = tInput.MoveDirection;

      return input;
    }

    public static implicit operator QuantumDemoInputTopDown(Input input) {
      QuantumDemoInputTopDown tInput = default;
      tInput.Left = input._left;
      tInput.Right = input._right;
      tInput.Up = input._up;
      tInput.Down = input._down;
      tInput.Jump = input._a;
      tInput.Dash = input._b;
      tInput.Fire = input._c;
      tInput.AltFire = input._d;
      tInput.Use = input._r1;

      tInput.AimDirection = input.ThumbSticks.Regular->_rightThumb;
      tInput.MoveDirection = input.ThumbSticks.Regular->_leftThumb;

      return tInput;
    }

    public FPVector2 DigitalDirection {
      get {
        FPVector2 value = default;
        if (Left) value.X -= FP._1;
        if (Right) value.X += FP._1;
        if (Down) value.Y -= FP._1;
        if (Up) value.Y += FP._1;
        if (value.SqrMagnitude > FP._1) value = value.Normalized;
        return value;
      }
    }
  }
}
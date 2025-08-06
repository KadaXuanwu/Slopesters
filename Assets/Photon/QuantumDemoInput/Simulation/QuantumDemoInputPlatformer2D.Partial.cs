namespace Quantum {
  using Photon.Deterministic;
  unsafe partial struct QuantumDemoInputPlatformer2D {
    public static implicit operator Input(QuantumDemoInputPlatformer2D pInput) {
      Input input = default;
      input._left = pInput.Left;
      input._right = pInput.Right;
      input._up = pInput.Up;
      input._down = pInput.Down;
      input._a = pInput.Jump;
      input._b = pInput.Dash;
      input._c = pInput.Fire;
      input._d = pInput.AltFire;
      input._r1 = pInput.Use;

      input.ThumbSticks.Regular->_leftThumb = pInput.AimDirection;
      return input;
    }

    public static implicit operator QuantumDemoInputPlatformer2D(Input input) {
      QuantumDemoInputPlatformer2D pInput = default;
      pInput.Left = input._left;
      pInput.Right = input._right;
      pInput.Up = input._up;
      pInput.Down = input._down;
      pInput.Jump = input._a;
      pInput.Dash = input._b;
      pInput.Fire = input._c;
      pInput.AltFire = input._d;
      pInput.Use = input._r1;

      pInput.AimDirection = input.ThumbSticks.Regular->_leftThumb;
      return pInput;
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
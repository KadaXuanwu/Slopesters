namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;

  /// <summary>
  /// A Unity script that creates empty input for any Quantum game.
  /// </summary>
  public class QuantumDemoInputTopDownPolling : MonoBehaviour {

    private void OnEnable() {
      QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    /// <summary>
    /// Set an empty input when polled by the simulation.
    /// </summary>
    /// <param name="callback"></param>
    public void PollInput(CallbackPollInput callback) {
      QuantumDemoInputTopDown tInput = default;

      var x = UnityEngine.Input.GetAxis("Horizontal");
      var y = UnityEngine.Input.GetAxis("Vertical");

      tInput.Left = x < 0;
      tInput.Right = x > 0;
      tInput.Down = y < 0;
      tInput.Up = y > 0;

      // no worries with clamping, normalization (all implicit)
      tInput.MoveDirection = new FPVector2(x.ToFP(), y.ToFP());

      // normally uses second thumb stick or mouse based input
      tInput.AimDirection = new FPVector2(x.ToFP(), y.ToFP());

      tInput.Jump = UnityEngine.Input.GetButton("Jump");
      tInput.Dash = UnityEngine.Input.GetButton("Fire2");
      tInput.Fire = UnityEngine.Input.GetButton("Fire1");
      tInput.Use = UnityEngine.Input.GetButton("Fire3");

      // implicitly casts to base input
      callback.SetInput(tInput, DeterministicInputFlags.Repeatable);
    }
  }
}
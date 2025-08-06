namespace Quantum
{
	using UnityEngine;
	using System.Threading.Tasks;
	using Quantum.Menu;

	/// <summary>
	/// This scripts manages cursor locking and shows/hides Camera and EventSystem in Menu scene.
	/// </summary>
	public class MenuUI : QuantumMenuConnectionBehaviourSDK
	{
		[SerializeField]
		private GameObject[] _menuObjects;

		private bool _isBusy;
		private bool _isConnected;

		public override async Task<ConnectResult> ConnectAsync(QuantumMenuConnectArgs connectionArgs)
		{
			_isBusy = true;

			ConnectResult result = await base.ConnectAsync(connectionArgs);

			_isBusy = false;

			return result;
		}

		public override async System.Threading.Tasks.Task DisconnectAsync(int reason)
		{
			_isBusy = true;

			await base.DisconnectAsync(reason);

			_isBusy = false;
		}

		private void Awake()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		private void Update()
		{
			if (_isBusy == true)
				return;
			if (_isConnected == IsConnected)
				return;

			_isConnected = IsConnected;

			if (_isConnected == false)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}

			foreach (GameObject go in _menuObjects)
			{
				go.SetActive(_isConnected == false);
			}
		}
	}
}

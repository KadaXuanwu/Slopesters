namespace Quantum
{
	using UnityEngine;

	/// <summary>
	/// Shows information related to gameplay.
	/// </summary>
	public sealed class GameplayUI : MonoBehaviour
	{
		[SerializeField]
		private GameObject _standaloneControls;
		[SerializeField]
		private GameObject _mobileControls;
		[SerializeField]
		private GameObject _gamepadControls;

		private void Awake()
		{
			RefreshControls(false);
		}

		private void Update()
		{
			if (QuantumRunner.Default != null)
			{
				RefreshControls(true);
			}
		}

		private void RefreshControls(bool hasPlayer)
		{
			if (hasPlayer == false)
			{
				if (_standaloneControls != null) { _standaloneControls.SetActive(false); }
				if (_mobileControls     != null) { _mobileControls.SetActive(false);     }
				if (_gamepadControls    != null) { _gamepadControls.SetActive(false);    }

				return;
			}

			if (Application.isMobilePlatform == true && Application.isEditor == false)
			{
				if (_standaloneControls != null) { _standaloneControls.SetActive(false); }
				if (_mobileControls     != null) { _mobileControls.SetActive(true);      }
			}
			else
			{
				if (_standaloneControls != null) { _standaloneControls.SetActive(true); }
				if (_mobileControls     != null) { _mobileControls.SetActive(false);    }
			}
		}
	}
}

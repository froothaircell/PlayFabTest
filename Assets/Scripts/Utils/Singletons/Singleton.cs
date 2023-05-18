using UnityEngine;
using Spades.Managers.GeneralUtils;

namespace PlayFabTests.Utils.Singleton
{
	public class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		#region Fields

		protected static T _instance;
		#endregion

		#region Properties
		public static T Instance
		{
			get
			{
				//if (GameStateManager.ApplicationQuitting) return null;
				if (_instance == null)
				{
					_instance = FindObjectOfType<T>();
					if (_instance == null)
					{
						GameObject obj = new GameObject();
						obj.name = typeof(T).Name;
						_instance = obj.AddComponent<T>();
					}
				}
				if (_instance == null)
				{
					Debug.LogError($"The instance for {typeof(T)} is still null");
				}
				return _instance;
			}
		}

		#endregion

		#region Methods

		protected virtual void InitSingleton()
		{
			//if (GameStateManager.ApplicationQuitting)
			//	return;

			if (Instance != null && GetInstanceID() != Instance.GetInstanceID())
			{
				Debug.LogError($"Destroying game object {gameObject}");
				Destroy(gameObject);
				return;
			}

			_instance = this as T;
			DontDestroyOnLoad(gameObject);
		}

		protected virtual void CleanSingleton()
		{ }

		protected virtual void Awake()
		{
			InitSingleton();
		}

		protected virtual void OnDestroy()
		{
			CleanSingleton();
		}

		#endregion
	}
}

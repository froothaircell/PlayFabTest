using UnityEngine;

namespace PlayFabTests.Utils.Singleton
{
	public class DestroyableSingleton<T> : MonoBehaviour where T : DestroyableSingleton<T>
	{
		#region Fields

		private static T _instance;

		#endregion

		#region Properties
		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        // Debug.LogWarning($"DestroyableSingleton of type {typeof(T)} does not exist in the current context");
						return null;
                    }
                }
				return _instance;
			}
		}

		#endregion

		#region Methods
		public static T CreateInstance(Transform parent = null)
		{
			if (_instance != null)
            {
				Debug.LogError($"Instance of {typeof(T)} already exists");
				return null;
            }

			var spawnedObj = new GameObject();
			spawnedObj.name = typeof(T).Name;

			if (parent != null)
            {
				spawnedObj.transform.SetParent(parent);
            }

			var typeObj = spawnedObj.AddComponent<T>();
			
			return typeObj;
		}

		protected virtual void InitSingleton()
		{
			if (Instance != null && GetInstanceID() != Instance.GetInstanceID())
			{
				Destroy(gameObject);
			}
			else
			{
				_instance = this as T;
			}
		}

		protected virtual void CleanSingleton()
		{
			_instance = null;
		}

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

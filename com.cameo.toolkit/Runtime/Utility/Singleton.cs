using UnityEngine;

namespace Cameo
{/// <summary>
///	自動搜尋場景中已存在的物件
///	如沒有將會自動建立
///
/// 在Resource資料夾中有與class name同名的物件，將會被自動加入場景中
/// </summary>
/// <typeparam name="T"></typeparam>
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		//public bool IsDontDestroyOnLoaded;

		protected static T _instance;

		protected static readonly object _synObject = new object();

		#region Property Message
		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_synObject)
					{
						if (_instance == null)
						{
							_instance = FindObjectOfType<T>();
						}
					}

					if (_instance == null)
					{
						Init();

						Debug.Log("An instance of " + typeof(T) +
							" is needed in the scene, but there is none. Created automatically");
					}
				}

				return _instance;
			}
		}

		public static  bool IsExistInstance	{get {return _instance != null;}}
		#endregion

	    void Awake()
		{
			RegisterInstance ();
		}

		public virtual void init()
        {

        }

		public static T Init()
		{
			string classname = typeof(T).Name.ToString();
			var prefab = Resources.Load(classname, typeof(GameObject)) as GameObject;
			var obj_init = Instantiate(prefab);
			_instance = obj_init.GetComponent<T>();
			if (_instance == null) _instance = obj_init.GetComponentInChildren<T>();
			if (_instance == null)
			{
				GameObject obj = new GameObject(typeof(T).ToString());
				_instance = obj.AddComponent<T>();
			}

			return _instance;
		}

		public void RegisterInstance () 
		{
			if (_instance == null)
			{
				_instance = GetComponent<T>();

				init();

				/*
				if ((_instance as Singleton<T>).IsDontDestroyOnLoaded) 
				{
					DontDestroyOnLoad (_instance.gameObject);
				}
				*/
			}
			else if(_instance != this)
			{
				DestroyImmediate(gameObject);
			}
		}
		/*
		//For Editor Use
		public void Destory()
		{
			if (_instance != null)
			{
				DestroyImmediate(_instance.gameObject);
				_instance = null;
			}
			T [] instances = FindObjectsOfType<T>();
			for(int i = 0; i < instances.Length; i++)
			{
				DestroyImmediate(instances[i].gameObject);
			}
		}
		*/
	}
}


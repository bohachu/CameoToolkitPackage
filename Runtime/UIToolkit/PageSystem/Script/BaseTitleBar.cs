using UnityEngine;
using UnityEngine.UI;

namespace Cameo.UI
{
	public class BaseTitleBar : MonoBehaviour 
	{
		public Text TextTitle;
        public float AnimationDuring = 0.25f;

        private BasePageAnimation[] pageAnimators;

		void Awake()
		{
			pageAnimators = GetComponents<BasePageAnimation> ();
		}

		public void Open(string title)
		{
			Open (title, AnimationDuring);
		}

		public virtual void Open(string title, float during)
		{
			if (TextTitle != null)
			{
				TextTitle.text = title;
			}
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive (true);
				for (int i = 0; i < pageAnimators.Length; ++i)
				{
					pageAnimators [i].Play (during, false);
				}
			}
			CancelInvoke ("onCloseFinished");
		}

		public void Close()
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive (true);
				for (int i = 0; i < pageAnimators.Length; ++i)
				{
					pageAnimators [i].Play (AnimationDuring, true);
				}
			}
			Invoke ("onCloseFinished", AnimationDuring);
		}

		protected virtual void onCloseFinished()
		{
			gameObject.SetActive (false);
		}
	}
}
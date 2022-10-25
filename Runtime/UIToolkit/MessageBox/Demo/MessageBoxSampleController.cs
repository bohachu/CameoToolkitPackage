using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cameo.UI;

public class MessageBoxSampleController : MonoBehaviour {

	public void OnBtnClicked()
	{
        Dictionary<string, object> paramMapping = new Dictionary<string, object>();
        paramMapping.Add("message", "Simple!");
		MessageBoxManager.Instance.ShowMessageBox ("Simple", paramMapping);
	}
}

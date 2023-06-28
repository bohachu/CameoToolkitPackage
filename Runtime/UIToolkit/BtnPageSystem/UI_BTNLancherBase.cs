using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Cameo
{
    [System.Serializable]
    public class ScoreResult
    {
        public string ID;
        public float Score;
        public float Duration;
        public bool IsPass=false; //是否通過，不一定會使用
        public ScoreResult()
        {
            ID = "";
            Score = 0;
            Duration = 0;
            IsPass = false;
        }
        public ScoreResult(string id, float score, float duration, bool isPass = false)
        {
            ID = id;
            Score = score;
            Duration = duration;
            IsPass = isPass;
        }
    }
}
namespace Cameo.UI
{
    public class UI_BTNLancherBase : MonoBehaviour
    {
        public UnityAction<ScoreResult> OnMissionDone;
        protected bool IsFirstPlay=true;
        public UnityAction OnMissionCancel;
        /// <summary>
        /// override to lanch any by BTNPage
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator LanchProcess(BTNData BTNData, UnityAction<ScoreResult> _OnMissionDone,string DataSheetID, bool IsFirstPlay, UnityAction _OnMissionCancel = null)
        {
            this.IsFirstPlay=IsFirstPlay;
            OnMissionDone = _OnMissionDone;
            OnMissionCancel = _OnMissionCancel;
            yield return null;
        }
    }
}

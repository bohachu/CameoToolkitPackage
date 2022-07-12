using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Cameo
{
    public class AudioControlCenter : Singleton<AudioControlCenter>
    {
        private class ValueToData
        {
            public string paramName;
            public float TargetValue;
            public float CurValue;
            public float Delta;
        }

        public AudioMixer Mixer;

        private Dictionary<string, ValueToData> dicValueToData = new Dictionary<string, ValueToData>();

        void LateUpdate()
        {
            foreach (string key in dicValueToData.Keys)
            {
                ValueToData valueToData = dicValueToData[key];

                if (valueToData.CurValue != valueToData.TargetValue)
                {
                    valueToData.CurValue += valueToData.Delta * Time.deltaTime;
                    if ((valueToData.Delta > 0 && valueToData.CurValue >= valueToData.TargetValue) ||
                       (valueToData.Delta < 0 && valueToData.CurValue < valueToData.TargetValue))
                    {
                        valueToData.CurValue = valueToData.TargetValue;
                    }
                    Mixer.SetFloat(valueToData.paramName, valueToData.CurValue);
                }

            }
        }

        public void FadeVolume(string paramName, float fadeFrom, float fadeTo, float time)
        {
            if (time <= 0)
            {
                Mixer.SetFloat(paramName, fadeTo);
            }
            else
            {
                ValueToData data = (dicValueToData.ContainsKey(paramName)) ? dicValueToData[paramName] : new ValueToData();

                data.paramName = paramName;
                data.TargetValue = fadeTo;
                data.CurValue = fadeFrom;
                data.Delta = (fadeTo - fadeFrom) / time;

                if (!dicValueToData.ContainsKey(paramName))
                {
                    dicValueToData.Add(data.paramName, data);
                }

                Mixer.SetFloat(paramName, fadeFrom);
            }
        }

        public void FadeVolume(string paramName, float fadeTo, float time)
        {
            float fadeFrom = -1;
            Mixer.GetFloat(paramName, out fadeFrom);

            FadeVolume(paramName, fadeFrom, fadeTo, time);
        }
    }
}

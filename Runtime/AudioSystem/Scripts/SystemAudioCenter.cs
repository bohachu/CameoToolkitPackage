using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cameo
{
    public class SystemAudioCenter : Singleton<SystemAudioCenter>
    {
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private List<AudioDef> audioDefs;
        [SerializeField]
        private List<AudioTextDef> audioTextDefs;
        [SerializeField] AudioClip FallBackAudio;
        private Dictionary<AudioClipType, AudioClip> AudioTypeMap
        {
            get
            {
                if (audiotypeMap == null)
                {
                    audiotypeMap = new Dictionary<AudioClipType, AudioClip>();

                    foreach (AudioDef audioDef in audioDefs)
                    {
                        audiotypeMap[audioDef.Def] = audioDef.Clip;
                    }
                }
                return audiotypeMap;
            }
        }
        private Dictionary<AudioClipType, AudioClip> audiotypeMap;
        private float lastShotTimming = 0;

        public AudioClip GetClip(AudioClipType clipType)
        {
            return AudioTypeMap[clipType];
        }
        public void PlayClip(AudioClip audio)
        {
            audioSource.PlayOneShot(audio);
        }
        public void PlayLoop(AudioClipType clipType)
        {
            if (!AudioTypeMap.ContainsKey(clipType))
            {
                Debug.Log("AudioClipType clip not exsit:" + clipType.ToString());
                return;
            }
            StartCoroutine(LoopPlay(clipType));
        }
        
        bool IsLoopPlay = false;
        IEnumerator LoopPlay(AudioClipType clipType)
        {
            IsLoopPlay = true;
            while (IsLoopPlay)
            {
                audioSource.PlayOneShot(AudioTypeMap[clipType]);
                yield return new WaitForSeconds(2f);
            }
            yield return null;
        }
        public void StopLoop()
        {
            IsLoopPlay = false;
        }

        public void PlayOneShot(string clipName)
        {
            foreach (AudioTextDef audioTextDef in audioTextDefs)
            {
                if (audioTextDef.Def == clipName)
                {
                    audioSource.PlayOneShot(audioTextDef.Clip);
                    return;
                }
            }
           
        }
        int errorCount = 0;
        public void PlayOneShot(AudioClipType clipType)
        {
            audioSource.loop = false;
            if (!AudioTypeMap.ContainsKey(clipType))
            {
                Debug.Log("AudioClipType clip not exsit");
                audioSource.PlayOneShot(FallBackAudio);
                
                return;
            }
            float escape = Time.time - lastShotTimming;

            if (escape < 0.1f)
            {
                errorCount++;
                if (errorCount > 9)
                    Debug.Log("連續十次播放小於0.1秒，跳過此音效 ：" + clipType.ToString());
                return;
            }
            errorCount = 0;
            lastShotTimming = Time.time;
            
            audioSource.PlayOneShot(AudioTypeMap[clipType]);

        }
    }

    [Serializable]
    public class AudioDef
    {
        public AudioClipType Def;
        public AudioClip Clip;
    }
    [Serializable]
    public class AudioTextDef
    {
        public string Def;
        public AudioClip Clip;
    }
    public enum AudioClipType
    {
        DefaultClick,
        CommonUIButton,
        MissionStart,
        MissionSuccess,
        MissionFail,
        ActionSuccess,
        ActionFail,
        TakePhoto,
        Scanning,
        NewPlayerButton,//和action一樣可以用CommunUIButton

        Purchase,

      //  PurchaseItemCountChange,//MissionStart 一樣 

      //  PlantAction,//和action一樣可以用CommunUIButton

        CastSkill,

        BodyImpact,
        ShowDialog,

        PlayerReact,

        GetGift,

      //  ItemCollect,//MissionStart 一樣
        PageScroll,

        Portal,

        ElfReact
    }

}

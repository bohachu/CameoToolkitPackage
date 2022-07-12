using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Cameo
{
    public class MusicSettingButtonController : MonoBehaviour
    {
        [SerializeField]
        private Image button;

        [SerializeField]
        private Sprite offSprite;

        [SerializeField]
        private Sprite onSprite;

        [SerializeField]
        private AudioMixer mixer;

        private static bool isMute = false;

        const string AudioMuteKey = "IS_AUDIO_MUTE";

        const string GlocalAudioKey = "GlobalAudio";

        private void Start()
        {
            if(PlayerPrefs.HasKey(AudioMuteKey))
            {
                isMute = PlayerPrefs.GetInt(AudioMuteKey) > 0;
            }
            else
            {
                isMute = true;
            }

            setAudio();
        }

        public void OnClicked()
        {
            isMute = !isMute;

            setAudio();
        }

        private void setAudio()
        {
            PlayerPrefs.SetInt(AudioMuteKey, isMute ? 1 : 0);

            button.sprite = isMute ? offSprite : onSprite;

            mixer.SetFloat(GlocalAudioKey, isMute ? -80 : 0);

            PlayerPrefs.Save();
        }
    }

}

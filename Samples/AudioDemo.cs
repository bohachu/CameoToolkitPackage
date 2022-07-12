using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cameo;
namespace Cameo.test
{
    public class AudioDemo : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SystemAudioCenter.Instance.PlayOneShot(AudioClipType.DefaultClick);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}

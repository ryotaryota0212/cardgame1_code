using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    //シングルトン化
    public static SoundManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public AudioSource audioSourceBGM;
    public AudioClip[] audioclipsBGM;


    public AudioSource audioSourceSE;
    public AudioClip[] audioclipsSE;

    
    public void PlayBGM(string SceneName)
    {

        audioSourceBGM.Stop();

        switch (SceneName)
        {
            default:

            case "Title":
            audioSourceBGM.clip =audioclipsBGM[0];
            break;

            case "Game":
            audioSourceBGM.clip =audioclipsBGM[1];
            break;

        }
        audioSourceBGM.Play();

    }

    public void PlaySE(int index)
    {
        audioSourceSE.PlayOneShot(audioclipsSE[index]);
    }
}

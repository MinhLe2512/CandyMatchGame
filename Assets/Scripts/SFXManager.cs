using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Clip { Select, Swap, Clear };
public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;
    public AudioSource[] sfx;

    // Start is called before the first frame update
    void Start()
    {
        instance = GetComponent<SFXManager>();
        sfx = GetComponents<AudioSource>();
    }
    public void PlaySFX(Clip audioClip)
    {
        sfx[(int)(audioClip)].Play();
    }
}

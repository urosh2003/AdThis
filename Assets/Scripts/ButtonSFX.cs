using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSFX : MonoBehaviour
{
    [SerializeField] private List<AudioClip> buttonClips;
    // Start is called before the first frame update

    void Awake()
    {
        //DontDestroyOnLoad(gameObject);
    }
    
    public void PlayRandomButtonSFX()
    {
        var clip = buttonClips[Random.Range(0, buttonClips.Count)];
        GetComponent<AudioSource>().PlayOneShot(clip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

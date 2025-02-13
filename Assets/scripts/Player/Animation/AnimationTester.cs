using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationTester : MonoBehaviour
{
    public Animator arm_anim;
    public string animation;

    private void Start()
    {
            arm_anim.Play(animation);
    }
    // Start is called before the first frame update

    

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopCharacterAnimation : MonoBehaviour
{
    public Image m_Image;

    public Sprite[] m_SpriteArray;
    public float m_Speed = .02f;

    public Sprite[] blue, red, yellow, green;

    private int m_IndexSprite;
    Coroutine m_CorotineAnim;
    bool IsDone;

    private void Awake()
    {
        if (blue != null)
        {
            m_SpriteArray = blue;
        }
        StartCoroutine(Func_PlayAnimUI());
    }

    public void SetBodySprite(Sprite[] anim)
    {
        m_SpriteArray  = anim;
    }
    IEnumerator Func_PlayAnimUI()
    {
        yield return new WaitForSeconds(m_Speed);
        if (m_IndexSprite >= m_SpriteArray.Length -1)
        {
            m_IndexSprite = 0;
        }
        m_Image.sprite = m_SpriteArray[m_IndexSprite];
        m_IndexSprite += 1;
        if (IsDone == false)
            m_CorotineAnim = StartCoroutine(Func_PlayAnimUI());
    }
}

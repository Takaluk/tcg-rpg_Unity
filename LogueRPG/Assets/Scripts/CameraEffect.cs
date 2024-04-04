using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffectManager : MonoBehaviour
{
    #region Instance
    private static CameraEffectManager m_instance;
    public static CameraEffectManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<CameraEffectManager>();
            }

            return m_instance;
        }
    }
    #endregion

    public void ShakeCam(float dur, float pow, int fre)
    {
        transform.DOShakePosition(dur, pow, fre);
    }
}

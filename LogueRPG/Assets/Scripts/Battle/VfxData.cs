using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VfxData : MonoBehaviour
{
    public float effectTiming;
    // Start is called before the first frame update
    public void DestroyAnimation()
    {
        Destroy(gameObject);
    }
}

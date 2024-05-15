using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buff : MonoBehaviour
{
    EntityStat stat;
    public SkillType buffType;
    int pow;
    float duration;
    public float currentTime;
    Image icon;
    Entity target;

    Coroutine buffCoroutine;
    WaitForSeconds delay01 = new WaitForSeconds(0.1f);
    private void Awake()
    {
        icon = GetComponent<Image>();
    }

    public void InIt(SkillEffect buffEffect, Entity target)
    {
        this.target = target;
        buffType = buffEffect.skillType;
        stat = buffEffect.skillStatType;
        pow = buffEffect.pow;
        duration = buffEffect.buffDur;
        icon.sprite = buffEffect.buffIcon;

        currentTime = duration;

        if (buffType == SkillType.Buff)
        {
            target.IncreaseStat(stat, pow);
            target.AddBuff(this);
        }
        else if (buffType == SkillType.Debuff)
        {
            target.DecreaseStat(stat, pow);
            target.AddDebuff(this);
        }
    }

    public void ActivateBuff()
    {
        buffCoroutine = StartCoroutine(Activation());
    }

    public void PauseBuffTimer()
    {
        if (buffCoroutine != null)
        {
            StopCoroutine(buffCoroutine);
        }
    }

    IEnumerator Activation()
    {
        while (currentTime > 0)
        {
            currentTime -= 0.1f;
            yield return delay01;
        }

        currentTime = 0;
        Deactivation();
    }

    public void Deactivation()
    {
        if (buffType == SkillType.Buff)
        {
            target.DecreaseStat(stat, pow);
            target.RemoveBuff(this);
        }
        else if (buffType == SkillType.Debuff)
        {
            target.IncreaseStat(stat, pow);
            target.RemoveDebuff(this);
        }

        Destroy(gameObject);
    }
}

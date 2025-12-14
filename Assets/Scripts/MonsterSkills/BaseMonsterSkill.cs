using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseMonsterSkill : MonoBehaviour
{
    public string skillName;
    public string skillDescription;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public abstract IEnumerator Execute(CardInstance target);
    public IEnumerator PostExecute()
    {
        yield return SpiritLinkManager.Instance.ResolveAll();
    }

    protected CardInstance cardInstance;

}

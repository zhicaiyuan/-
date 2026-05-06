using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneSkill : MonoBehaviour
{

    [Header("复制信息")]
    [SerializeField] private GameObject clonePerfab;
    [SerializeField] private float cloneDuration;
    [Space]
    [SerializeField] private bool canAttack;

    public void CreateClone(Transform clonePosition,Vector3 offset)
    {
        GameObject newClone = Instantiate(clonePerfab);

        newClone.GetComponent<CloneSkillController>().SetUpClone(clonePosition,cloneDuration,canAttack,offset);
    }
}

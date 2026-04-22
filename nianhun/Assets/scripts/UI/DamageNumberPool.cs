using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageNumberPool : MonoBehaviour
{
    public static DamageNumberPool instance;
    public DamageNumber damageNumberprefab;
    public Transform canvasTransform;
    public int poolSize = 20;
    private Queue<DamageNumber> pool = new Queue<DamageNumber>();
    void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
        for (int i = 0; i < poolSize; i++)
        {
            DamageNumber dn = Instantiate(damageNumberprefab, canvasTransform);
            dn.gameObject.SetActive(false);
            pool.Enqueue(dn);
        }
    }
    public DamageNumber GetFromPool()
    {
        if(pool.Count > 0)
        {
            DamageNumber dn = pool.Dequeue();
            dn.gameObject.SetActive(true);
            return dn;
        }
        else
        {
            return Instantiate(damageNumberprefab, canvasTransform);
        }
    }

    public void RenturnToPool(DamageNumber dn)
    {
        if(dn == null )
        {
            return;
        }
        dn.gameObject.SetActive(false);
        pool.Enqueue(dn);
    }
    public void SpawnDamageNumber(Vector3 pos,int damage,bool isCrit)
    {
        if(damage == 0)
        {
            return;
        }
        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        DamageNumber newNumber = GetFromPool();
        newNumber.transform.position = screenPos;
        newNumber.Initialize(damage, isCrit);
        StartCoroutine(RecycleAfterTime(newNumber));
    }
        IEnumerator RecycleAfterTime(DamageNumber dn)
    {
        yield return new WaitForSeconds(dn.fadeduration);
        RenturnToPool(dn);
    }
}

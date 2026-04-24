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
    }//从对象池里获取

    public void RenturnToPool(DamageNumber dn)
    {
        if(dn == null )
        {
            return;
        }
        dn.gameObject.SetActive(false);
        pool.Enqueue(dn);
    }//返回对象池
    public void SpawnDamageNumber(Vector2 pos,int damage,bool isCrit,bool isavoid)
    {
        if(damage == 0)
        {
            return;
        }
        
        DamageNumber newNumber = GetFromPool();
        Debug.Log(pos);
        newNumber.transform.position = new Vector3((float)pos.x, (float)pos.y, newNumber.transform.position.z);
        Debug.Log(newNumber.transform.position);//设置位置
        newNumber.Initialize(damage, isCrit,isavoid);
        if (newNumber != null)
        {
        StartCoroutine(RecycleAfterTime(newNumber));

        }

    }
        IEnumerator RecycleAfterTime(DamageNumber dn)
    {
        yield return new WaitForSeconds(dn.fadeduration);
        RenturnToPool(dn);//等待几秒返回对象池
    }
}

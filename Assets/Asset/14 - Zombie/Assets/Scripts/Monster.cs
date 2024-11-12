using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public float damage = 100;

    public virtual void Attack()
    {
        Debug.Log("공격!");
    }
}

public class Orc : Monster
{
    public override void Attack()
    {
        base.Attack();
        Debug.Log("우리는 노예가 되지 않는다!");
    }
    public void WarCry()
    {
        Debug.Log("전투함성!");

        Monster[] monsters = FindObjectsOfType<Monster>();
        for (int i = 0; i < monsters.Length; i++) {
            {
                monsters[i].damage += 10;
            }
    }
}

public class Dragon : Monster
{
        public override void Attack()
        {
            base.Attack();
            Debug.Log("모든 것이 불타오를 것이다!");
        }
        public void Fly()
    {
        Debug.Log("날기");
    }
}
}

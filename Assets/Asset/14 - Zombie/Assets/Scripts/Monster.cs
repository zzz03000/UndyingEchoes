using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public float damage = 100;

    public virtual void Attack()
    {
        Debug.Log("����!");
    }
}

public class Orc : Monster
{
    public override void Attack()
    {
        base.Attack();
        Debug.Log("�츮�� �뿹�� ���� �ʴ´�!");
    }
    public void WarCry()
    {
        Debug.Log("�����Լ�!");

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
            Debug.Log("��� ���� ��Ÿ���� ���̴�!");
        }
        public void Fly()
    {
        Debug.Log("����");
    }
}
}

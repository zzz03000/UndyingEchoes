using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; //AI, ������̼� �ý��� ���� �ڵ� ��������
using Photon.Pun;

public class Enemy : LivingEntity
{
    public LayerMask whatIsTarget;//���� ��� ���̾�

    private LivingEntity targetEntity; //���� ���
    private NavMeshAgent pathFinder; //��� ��� AI ������Ʈ

    public ParticleSystem hitEffect; //�ǰݽ� ����� ��ƼŬ ȿ��
    public AudioClip deathSound; //��� �� ����� �Ҹ�
    public AudioClip hitSound; //�ǰ� �� ����� �Ҹ�

    private Animator enemyAnimator; //�ִϸ����� ������Ʈ
    private AudioSource enemyAudioPlayer; //����� �ҽ� ������Ʈ
    private Renderer enemyRenderer; //������ ������Ʈ

    public float damage = 20f; //���ݷ�
    public float timeBetAttack = 0.5f; //���� ����
    private float lastAttackTime; //������ ���� ����

    private PhotonView photonView;

    //������ ����� �����Ѵ��� �˷��ִ� ������Ƽ
    private bool hasTarget
    {
        get
        {
            //������ ����� �����ϰ�, ����� ������� �ʾҴٸ� true
            if (targetEntity != null && !targetEntity.dead)
            {
                return true;
            }

            //�׷��� �ʴٸ� false
            return false;
        }
    }
    private void Awake()
    {
        pathFinder = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
        enemyAudioPlayer = GetComponent<AudioSource>();

        enemyRenderer = GetComponentInChildren<Renderer>();
        photonView = GetComponent<PhotonView>(); // PhotonView �ʱ�ȭ
    }

    //�� AI�� �ʱ� ������ �����ϴ� �¾� �޼���
    [PunRPC]
    public void Setup(float newHealth, float newDamage, float newSpeed, Color skinColor)
    {
        //ü�� ����
        startingHealth = newHealth;
        health = newHealth;
        //���ݷ� ����
        damage = newDamage;
        //�׺�޽� ������Ʈ�� �̵� �ӵ� ����
        pathFinder.speed = newSpeed;
        enemyRenderer.material.color = skinColor;

    }

    private void Start()
    {
        //ȣ��Ʈ�� �ƴ϶�� AI�� ���� ��ƾ�� �������� ����
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        //���� ������Ʈ Ȱ��ȭ�� ���ÿ� AI ���� ��ƾ ����
        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        //ȣ��Ʈ�� �ƴ϶�� �ִϸ��̼� �Ķ���͸� ���� �������� ����
        //ȣ��Ʈ�� �Ķ���͸� �����ϸ� Ŭ���̾�Ʈ�� �ڵ����� ���޵Ǳ� ����
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        //���� ����� ���� ���ο� ���� �ٸ� �ִϸ��̼� ���
        enemyAnimator.SetBool("HasTarget", hasTarget);
    }

    //�ֱ������� ������ ����� ��ġ�� ã�� ��� ����
    private IEnumerator UpdatePath()
    {
        //��� �ִ� ���� ���� ����
        while (!dead)
        {
            if(hasTarget)
            {
                //���� ��� ���� : ��θ� �����ϰ� AI �̵��� ��� ����
                pathFinder.isStopped = false;
                pathFinder.SetDestination(targetEntity.transform.position);
            }
            else
            {
                //���� ��� ���� : AI �̵� ����
                pathFinder.isStopped = true;

                //20������ �������� ���� ������ ���� �׷��� �� ���� ��ġ�� ��� �ݶ��̴��� ������
                //��,whatIsTarget ���̾ ���� �ݶ��̴��� ���������� ���͸�
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

                for(int i = 0; i < colliders.Length; i++)
                {
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    if(livingEntity != null && !livingEntity.dead)
                    {
                        targetEntity = livingEntity;
                        // ���� ��� ���� RPC ȣ��
                        photonView.RPC("SetTarget", RpcTarget.Others, targetEntity.photonView.ViewID);

                        break;
                    }
                }
            }
            //0.25�� �ֱ�� ó�� �ݺ�
            yield return new WaitForSeconds(0.25f);
        }
    }
    [PunRPC]
    private void SetTarget(int targetViewID)
    {
        PhotonView targetPhotonView = PhotonView.Find(targetViewID);
        if (targetPhotonView != null)
        {
            targetEntity = targetPhotonView.GetComponent<LivingEntity>();
        }
    }

    //�������� �Ծ��� �� ������ ó��
    [PunRPC]
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        //���� ������� ���� ��쿡�� �ǰ� ȿ�� ���
        if(!dead)
        {
            //���ݹ��� ������ �������� ��ƼŬ ȿ�� ���
            hitEffect.transform.position = hitPoint;
            hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitEffect.Play();

            //�ǰ� ȿ���� ���
            enemyAudioPlayer.PlayOneShot(hitSound);
        }
        //LivingEntity�� OnDamage()�� �����Ͽ� ������ ����
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    //��� ó��
    public override void Die()
    {
        //LivingEntity�� Die()�� �����Ͽ� �⺻ ��� ó�� ����
        base.Die();

        //�ٸ� AI�� �������� �ʵ��� �ڽ��� ��� �ݶ��̴��� ��Ȱ��ȭ
        Collider[] enemyColliders = GetComponents<Collider>();
        for(int i = 0; i < enemyColliders.Length;i++)
        {
            enemyColliders[i].enabled = false;
        }

        //AI ������ �����ϰ� �׺�Ž� ������Ʈ ��Ȱ��ȭ
        pathFinder.isStopped = true;
        pathFinder.enabled = true;

        //��� �ִϸ��̼� ���
        enemyAnimator.SetTrigger("Die");
        //��� ȿ���� ���
        enemyAudioPlayer.PlayOneShot(deathSound);

        // ��� �̺�Ʈ�� Ŭ���̾�Ʈ�� �˸�
        photonView.RPC("OnDie", RpcTarget.Others);

    }

    [PunRPC]
    private void OnDie()
    {
        // ��� �ִϸ��̼� ó��
        enemyAnimator.SetTrigger("Die");
    }

    private void OnTriggerStay(Collider other)
    {
        //ȣ��Ʈ�� �ƴ϶�� ���� ���� �Ұ�
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        //�ڽ��� ������� �ʾ�����,
        //�ֱ� ���� �������� timebetAttack �̻� �ð��� �����ٸ� ���� ����
        if(!dead && Time.time >= lastAttackTime + timeBetAttack)
        {
            //������ LivingEntity Ÿ�� �������� �õ�
            LivingEntity attackTarget = other.GetComponent<LivingEntity>();

            //������ LivingEntity�� �ڽ��� ���� ����̶�� ���� ����
            if(attackTarget != null && attackTarget == targetEntity)
            {
                lastAttackTime = Time.time;

                //������ �ǰ� ��ġ�� �ǰ� ������ �ٻ����� ���
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - other.transform.position;

                //���� ����
                attackTarget.OnDamage(damage, hitPoint, hitNormal);
            }
        }
        //Ʈ���� �浹�� ���� ���� ������Ʈ�� ���� ����̶�� ���� ����
    }
}

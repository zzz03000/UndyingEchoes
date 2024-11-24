using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; //AI, 내비게이션 시스템 관련 코드 가져오기
using Photon.Pun;

public class Enemy : LivingEntity
{
    public LayerMask whatIsTarget;//추적 대상 레이어

    private LivingEntity targetEntity; //추적 대상
    private NavMeshAgent pathFinder; //경로 계산 AI 에이전트

    public ParticleSystem hitEffect; //피격시 재생할 파티클 효과
    public AudioClip deathSound; //사망 시 재생할 소리
    public AudioClip hitSound; //피격 시 재생할 소리

    private Animator enemyAnimator; //애니메이터 컴포넌트
    private AudioSource enemyAudioPlayer; //오디오 소스 컴포넌트
    private Renderer enemyRenderer; //랜더러 컴포넌트

    public float damage = 20f; //공격력
    public float timeBetAttack = 0.5f; //공격 간격
    private float lastAttackTime; //마지막 공격 시점

    private PhotonView photonView;

    //추적할 대상이 존재한느지 알려주는 프로퍼티
    private bool hasTarget
    {
        get
        {
            //추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (targetEntity != null && !targetEntity.dead)
            {
                return true;
            }

            //그렇지 않다면 false
            return false;
        }
    }
    private void Awake()
    {
        pathFinder = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
        enemyAudioPlayer = GetComponent<AudioSource>();

        enemyRenderer = GetComponentInChildren<Renderer>();
        photonView = GetComponent<PhotonView>(); // PhotonView 초기화
    }

    //적 AI의 초기 스펙을 결정하는 셋업 메서드
    [PunRPC]
    public void Setup(float newHealth, float newDamage, float newSpeed, Color skinColor)
    {
        //체력 설정
        startingHealth = newHealth;
        health = newHealth;
        //공격력 설정
        damage = newDamage;
        //네비메시 에이전트의 이동 속도 설정
        pathFinder.speed = newSpeed;
        enemyRenderer.material.color = skinColor;

    }

    private void Start()
    {
        //호스트가 아니라면 AI의 추적 루틴을 실행하지 않음
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        //게임 오브젝트 활성화와 동시에 AI 추적 루틴 시작
        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        //호스트가 아니라면 애니메이션 파라미터를 직접 갱신하지 않음
        //호스트가 파라미터를 갱신하면 클라이언트에 자동으로 전달되기 때문
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        //추적 대상의 존재 여부에 따라 다른 애니메이션 재생
        enemyAnimator.SetBool("HasTarget", hasTarget);
    }

    //주기적으로 추적할 대상의 위치를 찾아 경로 갱신
    private IEnumerator UpdatePath()
    {
        //살아 있는 동안 무한 루프
        while (!dead)
        {
            if(hasTarget)
            {
                //추적 대상 존재 : 경로를 갱신하고 AI 이동을 계속 진행
                pathFinder.isStopped = false;
                pathFinder.SetDestination(targetEntity.transform.position);
            }
            else
            {
                //추적 대상 없음 : AI 이동 중지
                pathFinder.isStopped = true;

                //20유닛의 반지름을 가진 가상의 구를 그렸을 때 구와 겹치는 모든 콜라이더를 가져옴
                //단,whatIsTarget 레이어를 가진 콜라이더만 가져오도록 필터링
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

                for(int i = 0; i < colliders.Length; i++)
                {
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    if(livingEntity != null && !livingEntity.dead)
                    {
                        targetEntity = livingEntity;
                        // 추적 대상 변경 RPC 호출
                        photonView.RPC("SetTarget", RpcTarget.Others, targetEntity.photonView.ViewID);

                        break;
                    }
                }
            }
            //0.25초 주기로 처리 반복
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

    //데미지를 입었을 때 실행할 처리
    [PunRPC]
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        //아직 사망하지 않은 경우에만 피격 효과 재생
        if(!dead)
        {
            //공격받은 지점과 방향으로 파티클 효과 재생
            hitEffect.transform.position = hitPoint;
            hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitEffect.Play();

            //피격 효과음 재생
            enemyAudioPlayer.PlayOneShot(hitSound);
        }
        //LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    //사망 처리
    public override void Die()
    {
        //LivingEntity의 Die()를 실행하여 기본 사망 처리 실행
        base.Die();

        //다른 AI를 방해하지 않도록 자신의 모든 콜라이더를 비활성화
        Collider[] enemyColliders = GetComponents<Collider>();
        for(int i = 0; i < enemyColliders.Length;i++)
        {
            enemyColliders[i].enabled = false;
        }

        //AI 추적을 중지하고 네비매쉬 컴포넌트 비활성화
        pathFinder.isStopped = true;
        pathFinder.enabled = true;

        //사망 애니메이션 재생
        enemyAnimator.SetTrigger("Die");
        //사망 효과음 재생
        enemyAudioPlayer.PlayOneShot(deathSound);

        // 사망 이벤트를 클라이언트에 알림
        photonView.RPC("OnDie", RpcTarget.Others);

    }

    [PunRPC]
    private void OnDie()
    {
        // 사망 애니메이션 처리
        enemyAnimator.SetTrigger("Die");
    }

    private void OnTriggerStay(Collider other)
    {
        //호스트가 아니라면 공격 실행 불가
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        //자신이 사망하지 않았으며,
        //최근 공격 시점에서 timebetAttack 이상 시간이 지났다면 공격 가능
        if(!dead && Time.time >= lastAttackTime + timeBetAttack)
        {
            //상대방의 LivingEntity 타입 가져오기 시도
            LivingEntity attackTarget = other.GetComponent<LivingEntity>();

            //상대방의 LivingEntity가 자신의 추적 대상이라면 공격 실행
            if(attackTarget != null && attackTarget == targetEntity)
            {
                lastAttackTime = Time.time;

                //상대방의 피격 위치와 피격 방향을 근삿값으로 계산
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - other.transform.position;

                //공격 실행
                attackTarget.OnDamage(damage, hitPoint, hitNormal);
            }
        }
        //트리거 충돌한 상대방 게임 오브젝트가 추적 대상이라면 공격 실행
    }
}

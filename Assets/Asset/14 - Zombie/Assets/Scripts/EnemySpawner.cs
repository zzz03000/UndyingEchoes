using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviourPun, IPunObservable
{
    public Enemy enemyPrefab; //생성할 적 AI

    public Transform[] spawnPoints; //적 AI를 소환할 위치

    public float damageMax = 40f; //최대 공격력
    public float damageMin = 20f; //최소 공격력

    public float healthMax = 200f; //최대 체력
    public float healthMin = 100f; //최소 체력

    public float speedMax = 3f; //최대 속도
    public float speedMin = 1f; //최소 속도

    public Color strongEnemyColor = Color.red; //강한 적 AI가 가지게 될 피부색

    private List<Enemy> enemies = new List<Enemy>(); //생성될 적을 담는 리스트

    private int enemyCount = 0; //현재 남은 적 수
    private int wave; //현재 웨이브


    //주기적으로 자동 실행되는 동기화 메서드
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //로컬 오브젝트라면 쓰기 부분이 실행됨
        if (stream.IsWriting)
        {
            //남은 적 수를 네트워크를 통해 보내기
            stream.SendNext(enemies.Count);
            //현재 웨이브를 네트워크를 통해 보내기
            stream.SendNext(wave);
        }
        else
        {
            //리모트 오브젝트라면 읽기 부분이 실행됨
            //남은 적 수를 네트워크를 통해 받기
            enemyCount = (int)stream.ReceiveNext();
            //현재 웨이브를 네트워크를 통해 받기
            wave = (int)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        PhotonPeer.RegisterType(typeof(Color), 128, ColorSerialization.SerializeColor, ColorSerialization.DeserializeColor);
    }
    private void Update()
    {
        //호스트만 적을 직접 생성할 수 있음
        //다른 클라이언트는 호스트가 생성한 적을 동기화를 통해 받아옴
        if(PhotonNetwork.IsMasterClient)
        {
            //게임오버 상태일 때는 생성하지 않음
            if (GameManager.instance != null && GameManager.instance.isGameover)
            {
                return;
            }

            //적을 모두 물리친 경우 다음 스폰 실행
            if (enemies.Count <= 0)
            {
                SpawnWave();
            }
        }

        //UI 갱신
        UpdateUI();
    }

    //웨이브 정보를 UI로 표시
    private void UpdateUI()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            // 호스트는 직접 생성한 적 리스트를 이용해 남은 적 수 표시
            UIManager.instance.UpdateWaveText(wave, enemies.Count);
        }
        else
        {
            //클라이언트가 적 리스트를 갱신할 수 없으므로
            //호스트가 보내준 enemyCount를 이용해 적 수 표시
            UIManager.instance.UpdateWaveText(wave, enemyCount);
        }
    }

    //현재 웨이브에 맞춰 적 생성
    private void SpawnWave()
    {
        //웨이브 1 증가
        wave++;

        //현재 웨이브 *1.5를 반올림한 만큼 적 생성
        int spawnCount = Mathf.RoundToInt(wave * 1.5f);

        //spawnCount만큼 적 생성
        for (int i = 0; i < spawnCount; i++)
        {
            //적 세기를 0% ~ 100% 사이에서 랜덤 결정
            float enemyIntensity = Random.Range(0f, 1f);
            // 적 생성 처리 실행
            CreateEnemy(enemyIntensity);
        }
    }

    //적을 생성하고 추적할 대상을 할당
    private void CreateEnemy(float intensity)
    {
        //intensity를 기반으로 적의 능력치 결정
        float health = Mathf.Lerp(healthMin, healthMax, intensity);
        float damage = Mathf.Lerp(damageMax, damageMin, intensity);
        float speed = Mathf.Lerp(speedMin, speedMax, intensity);

        //intensity를 기반으로 하얀색과 enemyStrength 사이에서 적의 피부색 결정
        Color skinColor = Color.Lerp(Color.white, strongEnemyColor, intensity);

        //생성할 위치를 랜덤으로 결정
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        //적 프리팹으로부터 적 생성. 네트워크상의 모든 클라이언트에 생성됨
        GameObject createdEnemy = PhotonNetwork.Instantiate(enemyPrefab.gameObject.name,spawnPoint.position,spawnPoint.rotation);

        //생성한 적을 셋업하기 위해 Enemy 컴포넌트를 가져옴
        Enemy enemy = createdEnemy.GetComponent<Enemy>();

        //생성한 적의 능력치와 추적 대상 설정
        enemy.photonView.RPC("Setup",RpcTarget.All,health,damage,speed,skinColor);

        //생성된 적을 리스트에 추가
        enemies.Add(enemy);

        //적의 onDeath 이벤트에 익명 메서드 등록
        //사망한 적을 리스트에서 제거
        enemy.onDeath += () => enemies.Remove(enemy);
        //사망한 적을 10초 뒤에 파괴
        enemy.onDeath += () => StartCoroutine(DestroyAfter(enemy.gameObject, 10f));
        //적 사망 시 점수 상승
        enemy.onDeath += () => GameManager.instance.AddScore(100);
    }

    //포톤의 PhotonNetwork.Destroy()는 지연 파괴를 지원하지 않으므로 지연 파괴를 직접 구현함
    IEnumerator DestroyAfter(GameObject target,float delay)
    {
        yield return new WaitForSeconds(delay);
        //target이 아직 파괴되지 않았다면
        if(target != null)
        {
            //target을 모든 네트워크상에서 파괴
            PhotonNetwork.Destroy(target);
        }
    }
}

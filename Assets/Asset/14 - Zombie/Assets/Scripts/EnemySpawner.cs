using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviourPun, IPunObservable
{
    public Enemy enemyPrefab; //������ �� AI

    public Transform[] spawnPoints; //�� AI�� ��ȯ�� ��ġ

    public float damageMax = 40f; //�ִ� ���ݷ�
    public float damageMin = 20f; //�ּ� ���ݷ�

    public float healthMax = 200f; //�ִ� ü��
    public float healthMin = 100f; //�ּ� ü��

    public float speedMax = 3f; //�ִ� �ӵ�
    public float speedMin = 1f; //�ּ� �ӵ�

    public Color strongEnemyColor = Color.red; //���� �� AI�� ������ �� �Ǻλ�

    private List<Enemy> enemies = new List<Enemy>(); //������ ���� ��� ����Ʈ

    private int enemyCount = 0; //���� ���� �� ��
    private int wave; //���� ���̺�


    //�ֱ������� �ڵ� ����Ǵ� ����ȭ �޼���
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //���� ������Ʈ��� ���� �κ��� �����
        if (stream.IsWriting)
        {
            //���� �� ���� ��Ʈ��ũ�� ���� ������
            stream.SendNext(enemies.Count);
            //���� ���̺긦 ��Ʈ��ũ�� ���� ������
            stream.SendNext(wave);
        }
        else
        {
            //����Ʈ ������Ʈ��� �б� �κ��� �����
            //���� �� ���� ��Ʈ��ũ�� ���� �ޱ�
            enemyCount = (int)stream.ReceiveNext();
            //���� ���̺긦 ��Ʈ��ũ�� ���� �ޱ�
            wave = (int)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        PhotonPeer.RegisterType(typeof(Color), 128, ColorSerialization.SerializeColor, ColorSerialization.DeserializeColor);
    }
    private void Update()
    {
        //ȣ��Ʈ�� ���� ���� ������ �� ����
        //�ٸ� Ŭ���̾�Ʈ�� ȣ��Ʈ�� ������ ���� ����ȭ�� ���� �޾ƿ�
        if(PhotonNetwork.IsMasterClient)
        {
            //���ӿ��� ������ ���� �������� ����
            if (GameManager.instance != null && GameManager.instance.isGameover)
            {
                return;
            }

            //���� ��� ����ģ ��� ���� ���� ����
            if (enemies.Count <= 0)
            {
                SpawnWave();
            }
        }

        //UI ����
        UpdateUI();
    }

    //���̺� ������ UI�� ǥ��
    private void UpdateUI()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            // ȣ��Ʈ�� ���� ������ �� ����Ʈ�� �̿��� ���� �� �� ǥ��
            UIManager.instance.UpdateWaveText(wave, enemies.Count);
        }
        else
        {
            //Ŭ���̾�Ʈ�� �� ����Ʈ�� ������ �� �����Ƿ�
            //ȣ��Ʈ�� ������ enemyCount�� �̿��� �� �� ǥ��
            UIManager.instance.UpdateWaveText(wave, enemyCount);
        }
    }

    //���� ���̺꿡 ���� �� ����
    private void SpawnWave()
    {
        //���̺� 1 ����
        wave++;

        //���� ���̺� *1.5�� �ݿø��� ��ŭ �� ����
        int spawnCount = Mathf.RoundToInt(wave * 1.5f);

        //spawnCount��ŭ �� ����
        for (int i = 0; i < spawnCount; i++)
        {
            //�� ���⸦ 0% ~ 100% ���̿��� ���� ����
            float enemyIntensity = Random.Range(0f, 1f);
            // �� ���� ó�� ����
            CreateEnemy(enemyIntensity);
        }
    }

    //���� �����ϰ� ������ ����� �Ҵ�
    private void CreateEnemy(float intensity)
    {
        //intensity�� ������� ���� �ɷ�ġ ����
        float health = Mathf.Lerp(healthMin, healthMax, intensity);
        float damage = Mathf.Lerp(damageMax, damageMin, intensity);
        float speed = Mathf.Lerp(speedMin, speedMax, intensity);

        //intensity�� ������� �Ͼ���� enemyStrength ���̿��� ���� �Ǻλ� ����
        Color skinColor = Color.Lerp(Color.white, strongEnemyColor, intensity);

        //������ ��ġ�� �������� ����
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        //�� ���������κ��� �� ����. ��Ʈ��ũ���� ��� Ŭ���̾�Ʈ�� ������
        GameObject createdEnemy = PhotonNetwork.Instantiate(enemyPrefab.gameObject.name,spawnPoint.position,spawnPoint.rotation);

        //������ ���� �¾��ϱ� ���� Enemy ������Ʈ�� ������
        Enemy enemy = createdEnemy.GetComponent<Enemy>();

        //������ ���� �ɷ�ġ�� ���� ��� ����
        enemy.photonView.RPC("Setup",RpcTarget.All,health,damage,speed,skinColor);

        //������ ���� ����Ʈ�� �߰�
        enemies.Add(enemy);

        //���� onDeath �̺�Ʈ�� �͸� �޼��� ���
        //����� ���� ����Ʈ���� ����
        enemy.onDeath += () => enemies.Remove(enemy);
        //����� ���� 10�� �ڿ� �ı�
        enemy.onDeath += () => StartCoroutine(DestroyAfter(enemy.gameObject, 10f));
        //�� ��� �� ���� ���
        enemy.onDeath += () => GameManager.instance.AddScore(100);
    }

    //������ PhotonNetwork.Destroy()�� ���� �ı��� �������� �����Ƿ� ���� �ı��� ���� ������
    IEnumerator DestroyAfter(GameObject target,float delay)
    {
        yield return new WaitForSeconds(delay);
        //target�� ���� �ı����� �ʾҴٸ�
        if(target != null)
        {
            //target�� ��� ��Ʈ��ũ�󿡼� �ı�
            PhotonNetwork.Destroy(target);
        }
    }
}

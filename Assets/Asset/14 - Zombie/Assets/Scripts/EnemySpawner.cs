using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
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
    private int wave; //���� ���̺�

    private void Update()
    {
        //���ӿ��� ������ ���� �������� ����
        if(GameManager.instance != null && GameManager.instance.isGameover)
        {
            return;
        }

        //���� ��� ����ģ ��� ���� ���� ����
        if(enemies.Count <= 0 )
        {
            SpawnWave();
        }

        //UI ����
        UpdateUI();
    }

    //���̺� ������ UI�� ǥ��
    private void UpdateUI()
    {
        //���� ���̺�� ���� �� �� ǥ��
        UIManager.instance.UpdateWaveText(wave, enemies.Count);
    }

    //���� ���̺꿡 ���� �� ����
    private void SpawnWave()
    {
        //���̺� 1 ����
        wave++;

        //���� ���̺� *1.5�� �ݿø��� ��ŭ �� ����
        int spawnCount = Mathf.RoundToInt(wave * 1.5f);

        //spawnCount��ŭ �� ����
        for(int i = 0;  i < spawnCount; i++)
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
        Transform spawnPoint = spawnPoints[Random.Range(0,spawnPoints.Length)];

        //�� ���������κ��� �� ����
        Enemy enemy = Instantiate(enemyPrefab,spawnPoint.position, spawnPoint.rotation);

        //������ ���� �ɷ�ġ�� ���� ��� ����
        enemy.Setup(health, damage, speed, skinColor);

        //������ ���� ����Ʈ�� �߰�
        enemies.Add(enemy);

        //���� onDeath �̺�Ʈ�� �͸� �޼��� ���
        //����� ���� ����Ʈ���� ����
        enemy.onDeath += () => enemies.Remove(enemy);
        //����� ���� 10�� �ڿ� �ı�
        enemy.onDeath += () => Destroy(enemy.gameObject, 10f);
        //�� ��� �� ���� ���
        enemy.onDeath += () => GameManager.instance.AddScore(100);
    }
}

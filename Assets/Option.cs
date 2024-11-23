using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Option : MonoBehaviourPun
{
    [SerializeField]
    private GameObject optionPanel; // �ɼ� â
    [SerializeField]
    private GameObject countdownPanel; //ī��Ʈ�ٿ� �ǳ�
    [SerializeField]
    private TextMeshProUGUI countdownText; // ī��Ʈ�ٿ� UI �ؽ�Ʈ

    // ȯ�漳�� ���� ��û
    public void RequestOpenOption()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncOptionState", RpcTarget.All, true); // ������ Ŭ���̾�Ʈ�� ���� ����ȭ
        }
        else
        {
            photonView.RPC("SendOpenOptionRequestToMaster", RpcTarget.MasterClient);
        }
    }

    // ȯ�漳�� �ݱ� ��û
    public void RequestCloseOption()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartCloseOptionCountdown", RpcTarget.All); // ������ Ŭ���̾�Ʈ�� ���� ����ȭ
        }
        else
        {
            photonView.RPC("SendCloseOptionRequestToMaster", RpcTarget.MasterClient);
        }
    }

    // ������ Ŭ���̾�Ʈ�� ��û�� �޾��� �� ó��
    [PunRPC]
    private void SendOpenOptionRequestToMaster()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncOptionState", RpcTarget.All, true);
        }
    }

    [PunRPC]
    private void SendCloseOptionRequestToMaster()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartCloseOptionCountdown", RpcTarget.All);
        }
    }

    // ��� Ŭ���̾�Ʈ�� ���� ����ȭ
    [PunRPC]
    private void SyncOptionState(bool isOpen)
    {
        optionPanel.SetActive(isOpen);
        Time.timeScale = isOpen ? 0f : 1f; // ������ ���� ����, ������ ���� �����
    }

    // �ݱ� ī��Ʈ�ٿ� ó�� (RPC ȣ�� �� �� Ŭ���̾�Ʈ���� ����)
    [PunRPC]
    private void StartCloseOptionCountdown()
    {
        // ī��Ʈ�ٿ��� �����ϴ� ��ȣ�� ������
        StartCoroutine(CloseOptionWithCountdown());
    }

    // ī��Ʈ�ٿ� ó��
    private IEnumerator CloseOptionWithCountdown()
    {
        optionPanel.SetActive(false); // �ɼ� â ��Ȱ��ȭ
        countdownPanel.gameObject.SetActive(true); // ī��Ʈ�ٿ� �ǳ� Ȱ��ȭ

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString(); // ī��Ʈ�ٿ� ������Ʈ
            yield return new WaitForSecondsRealtime(1f); // ���� �ð� �������� 1�� ���
        }

        countdownPanel.gameObject.SetActive(false); // ī��Ʈ�ٿ� �ǳ� ��Ȱ��ȭ
        Time.timeScale = 1f; // ���� �����
    }

    public void GameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

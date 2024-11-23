using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Option : MonoBehaviourPun
{
    [SerializeField]
    private GameObject optionPanel; // 옵션 창
    [SerializeField]
    private GameObject countdownPanel; //카운트다운 판넬
    [SerializeField]
    private TextMeshProUGUI countdownText; // 카운트다운 UI 텍스트

    // 환경설정 열기 요청
    public void RequestOpenOption()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncOptionState", RpcTarget.All, true); // 마스터 클라이언트가 상태 동기화
        }
        else
        {
            photonView.RPC("SendOpenOptionRequestToMaster", RpcTarget.MasterClient);
        }
    }

    // 환경설정 닫기 요청
    public void RequestCloseOption()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartCloseOptionCountdown", RpcTarget.All); // 마스터 클라이언트가 상태 동기화
        }
        else
        {
            photonView.RPC("SendCloseOptionRequestToMaster", RpcTarget.MasterClient);
        }
    }

    // 마스터 클라이언트가 요청을 받았을 때 처리
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

    // 모든 클라이언트에 상태 동기화
    [PunRPC]
    private void SyncOptionState(bool isOpen)
    {
        optionPanel.SetActive(isOpen);
        Time.timeScale = isOpen ? 0f : 1f; // 열리면 게임 멈춤, 닫히면 게임 재시작
    }

    // 닫기 카운트다운 처리 (RPC 호출 후 각 클라이언트에서 실행)
    [PunRPC]
    private void StartCloseOptionCountdown()
    {
        // 카운트다운을 시작하는 신호를 보낸다
        StartCoroutine(CloseOptionWithCountdown());
    }

    // 카운트다운 처리
    private IEnumerator CloseOptionWithCountdown()
    {
        optionPanel.SetActive(false); // 옵션 창 비활성화
        countdownPanel.gameObject.SetActive(true); // 카운트다운 판넬 활성화

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString(); // 카운트다운 업데이트
            yield return new WaitForSecondsRealtime(1f); // 실제 시간 기준으로 1초 대기
        }

        countdownPanel.gameObject.SetActive(false); // 카운트다운 판넬 비활성화
        Time.timeScale = 1f; // 게임 재시작
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;
using Photon.Pun;

public class ChatManager : MonoBehaviourPun
{
    /*bool isGameStart = false;
    string playerName = "";*/

    public string chatMessage;
    Text chatText;
    ScrollRect scroll_rect = null;

    public GameObject chatPanel;      // Scroll View 또는 채팅창 패널
    public InputField inputField;     // 채팅 입력창

    PhotonView pv;

    private bool isChatActive = false;  // 채팅창 활성화 여부
    void Awake()
    {
        chatText = GameObject.Find("ChatText").GetComponent<Text>();
        scroll_rect = GameObject.Find("Scroll View").GetComponent <ScrollRect>();
    }

    void Start()
    {
        chatPanel.SetActive(false);
        inputField.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Enter 키로 채팅창 활성화/비활성화 전환
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isChatActive)  // 채팅창이 비활성화된 상태
            {
                ActivateChat();
            }
            else  // 채팅창이 활성화된 상태, 메시지 보내기
            {
                SendMessage();
            }
        }
    }

    void ActivateChat()
    {
        isChatActive = true;
        chatPanel.SetActive(true);
        inputField.gameObject.SetActive(true);
        inputField.ActivateInputField();
    }

    void SendMessage()
    {
        string message = inputField.text.Trim();

        if (!string.IsNullOrEmpty(message))  // 빈 메시지는 무시
        {
            
            // 채팅창에 메시지 추가
            chatText.text += "\n" + PhotonNetwork.NickName +" : " + message;

            // 입력 필드 초기화
            inputField.text = "";
        }

        // 다시 입력 대기
        inputField.ActivateInputField();
    }

    /*public void ShowChat(string chat)
    {
        chatText.text += chat + "\n";
        scroll_rect.verticalNormalizedPosition = 0.0f;
    }*/
}

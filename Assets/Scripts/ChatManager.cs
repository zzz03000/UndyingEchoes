using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    /*bool isGameStart = false;
    string playerName = "";*/

    public string chatMessage;
    Text chatText;
    ScrollRect scroll_rect = null;

    void Awake()
    {
        chatText = GameObject.Find("ChatText").GetComponent<Text>();
        scroll_rect = GameObject.Find("Scroll View").GetComponent <ScrollRect>();
    }

    public void ShowChat(string chat)
    {
        chatText.text += chat + "\n";
        scroll_rect.verticalNormalizedPosition = 0.0f;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

using Photon.Pun;
using UnityEngine;

public class PlayerNicknameDisplay : MonoBehaviourPun
{
    public TextMesh nicknameText; // 닉네임을 표시할 TextMesh
    public Transform characterTransform; // 캐릭터의 Transform
    private Transform cameraTransform;

    private void Start()
    {
        // 자신의 PhotonView인지 확인 후 닉네임 설정
        if (photonView.IsMine)
        {
            nicknameText.text = PhotonNetwork.NickName;
        }
        else
        {
            nicknameText.text = photonView.Owner.NickName; // 다른 플레이어의 닉네임
        }

        // 카메라 Transform 가져오기
        cameraTransform = Camera.main.transform;

        // 캐릭터 Transform 설정 (필요시 수동 연결 가능)
        if (characterTransform == null)
        {
            characterTransform = transform;
        }
    }

    private void Update()
    {
        if (cameraTransform != null)
        {
            // 닉네임의 위치는 캐릭터 머리 위에 고정
            Vector3 nicknamePosition = characterTransform.position + Vector3.up * 2.5f; // 머리 위로 올리기
            nicknameText.transform.position = nicknamePosition;

            // 닉네임이 항상 카메라를 바라보게 설정
            nicknameText.transform.LookAt(cameraTransform);
            nicknameText.transform.Rotate(0, 180, 0); // 텍스트가 반대로 보이지 않도록 회전
        }
    }
}


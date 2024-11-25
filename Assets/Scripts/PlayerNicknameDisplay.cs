using Photon.Pun;
using UnityEngine;

public class PlayerNicknameDisplay : MonoBehaviourPun
{
    public TextMesh nicknameText; // �г����� ǥ���� TextMesh
    public Transform characterTransform; // ĳ������ Transform
    private Transform cameraTransform;

    private void Start()
    {
        // �ڽ��� PhotonView���� Ȯ�� �� �г��� ����
        if (photonView.IsMine)
        {
            nicknameText.text = PhotonNetwork.NickName;
        }
        else
        {
            nicknameText.text = photonView.Owner.NickName; // �ٸ� �÷��̾��� �г���
        }

        // ī�޶� Transform ��������
        cameraTransform = Camera.main.transform;

        // ĳ���� Transform ���� (�ʿ�� ���� ���� ����)
        if (characterTransform == null)
        {
            characterTransform = transform;
        }
    }

    private void Update()
    {
        if (cameraTransform != null)
        {
            // �г����� ��ġ�� ĳ���� �Ӹ� ���� ����
            Vector3 nicknamePosition = characterTransform.position + Vector3.up * 2.5f; // �Ӹ� ���� �ø���
            nicknameText.transform.position = nicknamePosition;

            // �г����� �׻� ī�޶� �ٶ󺸰� ����
            nicknameText.transform.LookAt(cameraTransform);
            nicknameText.transform.Rotate(0, 180, 0); // �ؽ�Ʈ�� �ݴ�� ������ �ʵ��� ȸ��
        }
    }
}


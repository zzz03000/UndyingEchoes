using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine; //�ó׸ӽ� ���� �ڵ�
using Photon.Pun; //Pun ���� �ڵ�

public class CameraSetup : MonoBehaviourPun
{
    void Start()
    {
        //���� �ڽ��� ���� �÷��̾���
        if(photonView.IsMine)
        {
            //���� �ִ� �ó׸ӽ� ���� ī�޶� ã��
            CinemachineVirtualCamera followCam = FindObjectOfType<CinemachineVirtualCamera>();
            //���� ī�޶��� ���� ����� �ڽ��� Ʈ���������� ����
            followCam.Follow = transform;
            followCam.LookAt = transform;
        }
    }
}

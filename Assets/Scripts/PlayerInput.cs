﻿using UnityEngine;
using Photon.Pun;

public class PlayerInput : MonoBehaviourPun
{
    public string moveAxisName = "Vertical"; // 앞뒤 움직임을 위한 입력축 이름
    public string rotateAxisName = "Horizontal"; // 좌우 회전을 위한 입력축 이름
    public string fireButtonName = "Fire1"; // 발사를 위한 입력 버튼 이름
    public string reloadButtonName = "Reload"; // 재장전을 위한 입력 버튼 이름
    public string strafeAxisName = "Horizontal"; // 좌우 이동을 위한 입력축 이름 (A/D)

    // 값 할당은 내부에서만 가능
    public float move { get; private set; } // 감지된 앞뒤 이동 입력값
    public float rotate { get; private set; } // 감지된 회전 입력값
    public bool fire { get; private set; } // 감지된 발사 입력값
    public bool reload { get; private set; } // 감지된 재장전 입력값
    public float strafe { get; private set; } // 감지된 좌우 이동 입력값

    // 매프레임 사용자 입력을 감지
    private void Update()
    {
        // 로컬 플레이어가 아닌 경우 입력을 받지 않음
        if (!photonView.IsMine)
        {
            return;
        }

        // 게임오버 상태에서는 사용자 입력을 감지하지 않는다
        if (GameManager.instance != null
            && GameManager.instance.isGameover)
        {
            move = 0;
            rotate = 0;
            fire = false;
            reload = false;
            strafe = 0;
            return;
        }

        // move에 관한 입력 감지
        move = Input.GetAxis(moveAxisName);
        // rotate에 관한 입력 감지
        rotate = Input.GetAxis(rotateAxisName);
        // fire에 관한 입력 감지
        fire = Input.GetButton(fireButtonName);
        // reload에 관한 입력 감지
        reload = Input.GetButtonDown(reloadButtonName);
        // 좌우 이동 입력 감지
        strafe = Input.GetAxis(strafeAxisName); // A/D 키로 이동 감지
    }
}

using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPun
{
    public float moveSpeed = 5f; // 앞뒤 이동 속도
    public float strafeSpeed = 5f; // 좌우 이동 속도
    public float mouseSensitivity = 2f; // 마우스 감도

    private Animator playerAnimator; // 애니메이터
    private PlayerInput playerInput; // 입력 관리
    private Rigidbody playerRigidbody; // 리지드바디
    [SerializeField]
    private Transform playerCamera; // 카메라

    private float cameraPitch = 0f; // 카메라 상하 회전 각도 제한

    [SerializeField]
    private GameObject cameraPrefab; // 카메라 프리팹

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();

        if (photonView.IsMine)
        {
            // 로컬 플레이어에만 카메라 추가
            GameObject cameraObject = Instantiate(cameraPrefab, transform);
            playerCamera = cameraObject.transform;

            // 카메라 위치 조정
            playerCamera.localPosition = new Vector3(-0.15f, 1.4f, 0.25f);
            playerCamera.localRotation = Quaternion.identity;

            Cursor.lockState = CursorLockMode.Locked; // 마우스 잠금
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // 이동 처리
        Move();

        // 애니메이터 갱신
        playerAnimator.SetFloat("Move", playerInput.move);
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // 회전 처리
        Rotate();
    }

    private void Move()
    {
        // 앞뒤 이동
        Vector3 moveDistance = playerInput.move * transform.forward * moveSpeed * Time.deltaTime;
        // 좌우 이동 (A/D)
        Vector3 strafeDistance = playerInput.strafe * transform.right * strafeSpeed * Time.deltaTime;

        // 리지드바디를 통한 이동 처리
        playerRigidbody.MovePosition(playerRigidbody.position + moveDistance + strafeDistance);
    }

    private void Rotate()
    {
        // 마우스 입력 감지
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 캐릭터 좌우 회전
        transform.Rotate(0, mouseX, 0);

        // 카메라 상하 회전 (각도 제한 적용)
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f); // 각도 제한
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }
}

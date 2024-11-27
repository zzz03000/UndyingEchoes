using Photon.Pun;
using UnityEngine;

// 플레이어 캐릭터를 사용자 입력에 따라 움직이는 스크립트
public class PlayerMovement : MonoBehaviourPun
{
    public float moveSpeed = 5f; // 앞뒤 움직임의 속도
    public float mouseSensitivity = 2f; // 마우스 민감도

    private Animator playerAnimator; // 플레이어 캐릭터의 애니메이터
    private PlayerInput playerInput; // 플레이어 입력을 알려주는 컴포넌트
    private Rigidbody playerRigidbody; // 플레이어 캐릭터의 리지드바디
    private float cameraPitch = 0f; // 카메라의 상하 회전값
    [SerializeField]
    private Transform playerCamera; // 플레이어 카메라

    [SerializeField]
    Camera chatactorCamera;

    [SerializeField]
    private GameObject cameraPrefab; // 카메라 프리팹

    private void Start()
    {
        // 사용할 컴포넌트들의 참조를 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        chatactorCamera = Camera.main;

        if (photonView.IsMine)
        {
            // 로컬 플레이어에만 카메라 추가
            GameObject cameraObject = Instantiate(cameraPrefab, transform);
            playerCamera = cameraObject.transform;

            // 카메라 위치 조정
            playerCamera.localPosition = new Vector3(-0.15f, 1.4f, 0.25f); // 머리 위치
            playerCamera.localRotation = Quaternion.identity;

            Cursor.lockState = CursorLockMode.Locked; // 마우스 잠금
        }

        /*//카메라를 찾지 못하면 에러코드 출력
        if (chatactorCamera == null)
        {
            Debug.LogError("카메라가 없음");
        }*/
    }
    private void Update()
    {
        // 로컬 플레이어만 카메라와 회전 조작 가능
        if (!photonView.IsMine) return;

        // 마우스 입력으로 회전
        Rotate();
    }
    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨
    private void FixedUpdate()
    {
        // 로컬 플레이어만 직접 위치와 회전을 변경 가능
        if (!photonView.IsMine)
        {
            return;
        }

        /*//마우스 방향으로 움직임 !!수정!!
        LookMouseCursor();*/
        // 움직임 실행
        Move();

        // 입력값에 따라 애니메이터의 Move 파라미터 값을 변경
        playerAnimator.SetFloat("Move", playerInput.moveVertical);
    }

    // 입력값에 따라 캐릭터를 앞뒤양옆으로 움직임
    private void Move()
    {
        // WASD 입력에 따른 이동 방향 계산
        Vector3 moveDirection =
            transform.forward * playerInput.moveVertical +
            transform.right * playerInput.moveHorizontal;

        // 리지드바디를 통해 이동 처리
        Vector3 moveVelocity = moveDirection.normalized * moveSpeed;
        playerRigidbody.velocity = new Vector3(moveVelocity.x, playerRigidbody.velocity.y, moveVelocity.z);
    }

    private void Rotate()
    {
        // 마우스 입력값
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 플레이어 좌우 회전 (Yaw)
        transform.Rotate(Vector3.up * mouseX);

        // 카메라 상하 회전 (Pitch)
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f); // 각도 제한
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
    }
    /*public void LookMouseCursor()
    {
        Ray ray = chatactorCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitResult;
        if (Physics.Raycast(ray, out hitResult))
        {
            Vector3 mouseDir = new Vector3(hitResult.point.x, transform.position.y, hitResult.point.z) - transform.position;
            playerAnimator.transform.forward = mouseDir;
        }
    }*/
}
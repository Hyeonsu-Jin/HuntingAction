using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float playerHp = 100;              // 체력
    private float playerEnergy = 100;          // 기력
    private float playerSpeed = 2f;      // 속도
    private float playerRSpeed = 4f;     // 달리기 속도
    private float invTime = 0.25f;       // 무적 시간

    private Animator pAnimator;                 // 플레이어 애니메이터 컴포넌트


    [SerializeField]
    private Transform pBody;                 // 플레이어 애니메이터 컴포넌트

    [SerializeField]
    private Transform camArm;           // 카메라 부모 오브젝트
    private float mouseX;               // 마우스 X 좌표
    private float mouseY;               // 마우스 Y 좌표
    private float mouseSpeed = 3f;      // 마우스 감도


    private bool isRun;

    // Start is called before the first frame update
    void Start()
    {
        // 마우스 중앙 고정
        Cursor.lockState = CursorLockMode.Locked;
        // 플레이어 시점 카메라 저장
        pAnimator = pBody.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        LookAround();
        PlayerMove();
    }

    // 카메라 시점 전환
    private void LookAround()
    {
        // 현재 마우스 위치
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        // 카메라 암 회전 값
        Vector3 camAngle = camArm.rotation.eulerAngles;
        // 카메라 시점 구하기
        float x = camAngle.x - mouseDelta.y;

        // 카메라 시점 상하 제한
        if (x < 180f)
            x = Mathf.Clamp(x, -1f, 70f);   // 카메라 현재 시점이 -1 ~ 70을 넘지 않도록 조정
        else
            x = Mathf.Clamp(x, 335f, 361f); // 카메라 현재 시점이 335 ~ 361을 넘지 않도록 조정

        // 카메라 시점 적용
        camArm.rotation = Quaternion.Euler(x, camAngle.y + mouseDelta.x, camAngle.z);
    }

    private void PlayerMove()
    {
        // 키보드 조작
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // 키보드 조작 시 isMove = true
        bool isMove = moveInput.magnitude != 0;
        // 키보드 조작에 따른 처리
        if (isMove)
        {
            // 카메라 시점 앞뒤 좌표 저장
            Vector3 lookForward = new Vector3(camArm.forward.x, 0f, camArm.forward.z).normalized;
            // 카메라 시점 좌우 좌표 저장
            Vector3 lookRight = new Vector3(camArm.right.x, 0f, camArm.right.z).normalized;

            // 카메라 시점과 키보드 입력에 따른 플레이어 좌표의 변위 값 저장
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;
            // 대각선 이동 시 빨라지는 문제 해결
            moveDir = moveDir.normalized;

            // 카메라 시점(보고있는) 방향으로 플레이어 회전
            pBody.forward = moveDir;
            // 키보드 입력 및 보고있는 방향에 따라 플레이어의 좌표 변경
            transform.position += moveDir * Time.deltaTime * playerSpeed;
        }
    }

    private void Attack()
    {

    }
}

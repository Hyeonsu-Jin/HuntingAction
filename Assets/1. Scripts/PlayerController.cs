using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 캐릭터, 애니메이터, 카메라 오브젝트 설정
    [SerializeField]
    private Transform pBody;                 // 플레이어 컴포넌트
    private Animator pAnimator;              // 플레이어 애니메이터 컴포넌트
    [SerializeField]
    private Transform camArm;                // 카메라 부모 오브젝트

    // 캐릭터 정보
    private float mouseSpeed = 1f;             // 마우스 감도
    private float playerHp = 100;              // 체력
    private float playerEnergy = 100;          // 기력
    private float playerSpeed = 2f;            // 속도
    private float invTime = 0.25f;             // 무적 시간

    // 회피 (구르기)
    private bool isRoll = false;                 // 회피 가능 여부
    private float rollDistance = 3f;             // 회피 거리
    private float rollCoolTime = 0.5f;             // 회피 쿨타임


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
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSpeed;
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
        camArm.position = pBody.position;
    }

    // 플레이어 조작 함수
    private void PlayerMove()
    {
        // 키보드 조작
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // 키보드 조작에 따른 처리
        // 이동 시 이동 애니메이션 재생
        if (moveInput.magnitude != 0)
            pAnimator.SetBool("Move", true);
        else
        {   
            // 미조작 시 대기 애니메이션 재생 후 리턴
            pAnimator.SetBool("Move", false);
            return;
        }

        // 구르는 중이면 리턴
        if (isRoll)
            return;

        
        // 아래는 플레이어 조작시에만 실행됨. ================================================================
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

        Vector3 finalDir = Time.deltaTime * playerSpeed * moveDir;
        // 키보드 입력 및 보고있는 방향에 따라 플레이어의 좌표 변경
        transform.position += finalDir;

        // 회피 함수
        Roll(moveDir);
    }

    /// <summary>
    /// 구르기 함수 (회피 함수)
    /// </summary>
    /// <param name="moveDir">움직일 방향</param>
    private void Roll(Vector3 moveDir)
    {
        // 스페이스바를 누르면 보고 있는 방향으로 구름.
        float rollSpeed = Input.GetAxisRaw("Roll") * rollDistance;
        if (rollSpeed == 0 || isRoll)
            return;

        // 구르기 시작
        // 구르기 애니메이션 재생
        pAnimator.SetTrigger("Roll");

        // 구르기를 연속으로 쓰지 못 하도록 설정
        isRoll = true;

        // 목적지 = 현재 위치 + (바라보는 방향 * 구르기 속도)
        Vector3 finalPos = transform.position + (moveDir * rollSpeed);
        StartCoroutine(RollingTime(finalPos));
    }

    /// <summary>
    /// 굴러서 목적지까지 이동하는 함수
    /// </summary>
    /// <param name="destination">목적지</param>
    /// <returns></returns>
    private IEnumerator RollingTime(Vector3 destination)
    {
        // 구른 시간
        float rollEndTime = 0f;
        // 1초동안 구르기 (구르기 쿨타임 동안)
        while (rollEndTime < rollCoolTime)
        {
            // 구른 시간 증가
            rollEndTime += Time.deltaTime;
            // 현재 위치를 구른 뒤의 위치로 부드럽게 이동
            // Vector3.Lerp(현재 위치, 목적지, *한 프레임에 목적지까지 얼마나 이동할지);   * 0 : 이동 X  ~  1 : 목적지
            transform.position = Vector3.Lerp(transform.position, destination, rollEndTime);
            yield return null;
        }
        // 선형 보간 마침
        transform.position = destination;
        // 구르기 종료
        isRoll = false;
    }

    

    private void Attack()
    {

    }
}

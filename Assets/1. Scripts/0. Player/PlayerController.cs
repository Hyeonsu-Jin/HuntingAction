using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public struct PlayerInfo
{
    // 플레이어 정보
    public float playerHp;                // 체력
    public float playerMaxHp;             // 최대 체력
    public float playerEnergy;            // 기력
    public float playerSpeed;             // 속도
    public float invTime;                 // 무적 시간

    public bool isHit;

    // 회피 (구르기)
    public bool isRoll;                    // 회피 상태 여부
    public float rollDistance;             // 회피 거리
    public float rollCoolTime;             // 회피 쿨타임

    // 가드
    public bool isDef;                    // 가드 유무
    public bool firstDef;                 // 가포 유무
    public bool isDefInv;                 // 가포 무적시간 유무
    public float dInvTime;                 // 가포 무적시간
}

public class PlayerController : MonoBehaviour
{
    // 캐릭터, 애니메이터, 카메라 오브젝트 설정
    [SerializeField]
    private Transform pBody;                 // 플레이어 컴포넌트
    private Animator pAnimator;              // 플레이어 애니메이터 컴포넌트
    [SerializeField]
    private Transform camArm;                // 카메라 부모 오브젝트
    private float mouseSpeed = 1f;             // 마우스 감도

    // 캐릭터 정보
    private PlayerInfo pInfo;
    // 캐릭터 정보 프로퍼티
    public PlayerInfo PlayerInfoProperty
    {
        get { return pInfo; }
        set { pInfo = value; }
    }

    private void PlayerInfoInitialize()
    {
        pInfo.playerHp = 100f;
        pInfo.playerMaxHp = 100f;
        pInfo.playerEnergy = 100f;
        pInfo.playerSpeed = 2f;
        pInfo.invTime = 0.25f;

        pInfo.isHit = false;

        pInfo.isRoll = false;
        pInfo.rollDistance = 3f;
        pInfo.rollCoolTime = 0.5f;

        pInfo.isDef = false;
        pInfo.isDefInv = false;
        pInfo.dInvTime = 0.025f;
        pInfo.firstDef = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerInfoInitialize();
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
        if (pInfo.isRoll)
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

        Vector3 finalDir = Time.deltaTime * pInfo.playerSpeed * moveDir;
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
        float rollSpeed = Input.GetAxisRaw("Roll") * pInfo.rollDistance;
        if (rollSpeed == 0 || pInfo.isRoll)
            return;

        // 구르기 시작
        // 구르기 애니메이션 재생
        pAnimator.SetTrigger("Roll");

        // 구르기를 연속으로 쓰지 못 하도록 설정
        pInfo.isRoll = true;

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
        Vector3 startPos = transform.position;
        // 1초동안 구르기 (구르기 쿨타임 동안)
        while (rollEndTime < pInfo.rollCoolTime)
        {
            // 구른 시간 증가
            rollEndTime += Time.deltaTime;
            // 현재 위치를 구른 뒤의 위치로 부드럽게 이동
            // Vector3.Lerp(현재 위치, 목적지, *한 프레임에 목적지까지 얼마나 이동할지);   * 0 : 이동 X  ~  1 : 목적지
            transform.position = Vector3.Lerp(startPos, destination, rollEndTime / pInfo.rollCoolTime);
            yield return null;
        }
        // 선형 보간 마침
        transform.position = destination;
        // 구르기 종료
        pInfo.isRoll = false;
    }

    

    private void Attack()
    {

    }
}

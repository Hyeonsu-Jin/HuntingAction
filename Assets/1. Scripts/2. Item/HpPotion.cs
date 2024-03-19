using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpPotion : Item
{
    // 체력 정보를 받기 위함
    private PlayerInfo pInfo;
    // 대형 물약 유무
    public bool isHuge = false;

    private void Start()
    {
        pInfo = GetComponent<PlayerController>().PlayerInfoProperty;
    }

    // 아이템 개수 세팅
    public override void ItemDataSetting()
    {
        iData.maxCount = 5;     // 총 소지 수 = 5;
        iData.currCount = 5;    // 현재 소지 수 = 5;
        iData.isUsing = false;    // 사용 중 상태 = false;
        iData.useTime = 3f;     // 사용 시 걸리는 시간 = 3f;
    }

    public override void Use()
    {
        // 소지 수가 0이거나 풀피면 리턴
        if (iData.isUsing || iData.currCount == 0 || pInfo.playerHp == 100)
            return;

        // 물약 사용 중
        iData.isUsing = true;
        // 소지 수 감소
        iData.currCount--;
        // @ 이속 감소 필요 

        // 물약 마시기
        StartCoroutine(useDelay());
    }

    private IEnumerator useDelay()
    {
        // 회복량
        float heallingHp;
        if (isHuge)
            heallingHp = 70f;
        else
            heallingHp = 35f;

        // 현재 HP
        float currHp = pInfo.playerHp;
        // 힐된 후의 HP
        float afterHp = currHp + heallingHp;
        // 힐링 시간
        float currDelTime = 0f;


        // HP를 서서히 채움.
        // 피가 100이 아니고, 현재 체력이 회복 후의 체력보다 적을 때.
        while (pInfo.playerHp != 100 && pInfo.playerHp < afterHp /* && !isRolling && !isHit*/)
        {
            // 구르거나 피격 시 물약 사용 중단
            if (pInfo.isRoll || pInfo.isHit)
                StopCoroutine(useDelay());

            // 체력을 서서히 회복
            pInfo.playerHp = Mathf.Lerp(currHp, afterHp, currDelTime / iData.useTime);
            // 체력 회복 걸린 시간 증가
            currDelTime += Time.deltaTime;
            yield return null;
        }

        /*
                // 구르지 않을 때만 피 채우기
                if (!isRolling || !isHit)
                    // HP 선형 보간 완료
                    td.hpProperty = afterHp;
        */

        // HP 선형 보간 완료
        pInfo.playerHp = afterHp;
        // 물약 사용 끝
        iData.isUsing = false;

        // @이속 증가 필요 
    }
}
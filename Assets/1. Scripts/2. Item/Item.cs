using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ItemData
{
    public int maxCount;    // 최대 소지 수
    public int currCount;   // 현재 소지 수
    public bool isUsing;      // 사용 중 상태
    public float useTime;   // 사용 시 걸리는 시간
}

public abstract class Item : MonoBehaviour
{
    // 아이템 정보
    protected ItemData iData;
    // 아이템 데이터 세팅
    public abstract void ItemDataSetting();
    // 아이템 사용
    public abstract void Use();
}
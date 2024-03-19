using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    private PlayerInfo pInfo;
    private void Start()
    {
        pInfo = GetComponent<PlayerController>().PlayerInfoProperty;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.red);
        Defence();
    }

    private void Defence()
    {
        float deff = Input.GetAxisRaw("Defence");
        if (deff != 0)
        {
            pInfo.isDef = true;

            if (pInfo.firstDef)
            {
                pInfo.isDefInv = true;
                StartCoroutine(DeffInvinsibleTime());
            }
            pInfo.firstDef = false;
        }
        else
        {
            pInfo.isDef = false;
            pInfo.firstDef = true;
        }
    }
    private IEnumerator DeffInvinsibleTime()
    {
        yield return new WaitForSecondsRealtime(pInfo.dInvTime);
        pInfo.isDefInv = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Monster")
        {
            if (pInfo.isDefInv)
            {
                Debug.Log($"가포 성공\nHP: {pInfo.playerHp}");
            }
            else if (pInfo.isDef)
            {
                pInfo.playerHp -= 5;
                Debug.Log($"일반 방어\nHP: {pInfo.playerHp}");
                // 연산 순서 때문에 성능 차이 발생 (아래의 주석 코드는 벡터 연산을 두 번 함)
                // transform.forward * Time.deltaTime
                // transform.forward * 5f
                // transform.position -= transform.forward * Time.deltaTime * 5f;       
                transform.position -= Time.deltaTime * 5f * transform.forward;
            }
            else
            {
                pInfo.playerHp -= 30;
                Debug.Log($"피격\nHP: {pInfo.playerHp}");
                transform.position -= Time.deltaTime * 10f * transform.forward;
            }
        }
    }
}

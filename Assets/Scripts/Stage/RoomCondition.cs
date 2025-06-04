using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCondition : MonoBehaviour
{
    List<GameObject> MonsterListInRoom = new List<GameObject> ( );
    public bool playerInThisRoom = false;
    public bool isClearRoom = false;
    // Start is called before the first frame update
    void Start ( )
    {

    }

    // Update is called once per frame
    void Update ( )
    {
        if ( playerInThisRoom )
        {
            if ( MonsterListInRoom.Count <= 0 && !isClearRoom )
            {
                isClearRoom = true;
                Debug.Log ( " Clear " );
            }
        }
    }

    private void OnTriggerEnter ( Collider other )
    {
        if ( other.CompareTag ( "Player" ) )
        {
            //플레이어가 방에 들어오면 이방의 몹리스트를 링크(복사)시킨다.
            playerInThisRoom = true;
            PlayerTargeting.Instance.MonsterList = new List<GameObject> ( MonsterListInRoom );
            Debug.Log ( "Enter New Room! Mob Count : " + PlayerTargeting.Instance.MonsterList.Count );
            Debug.Log ( "Player Enter New Room!" );
        }
        if ( other.CompareTag ( "Monster" ) )
        {
            MonsterListInRoom.Add ( other.transform.parent.gameObject ); // 변경
            Debug.Log ( " Mob name : " + other.transform.parent.gameObject ); // 변경
        }
    }

    private void OnTriggerExit ( Collider other )
    {
        if ( other.CompareTag ( "Player" ) )
        {
            playerInThisRoom = false;
            PlayerTargeting.Instance.MonsterList.Clear ( );
            Debug.Log ( "Player Exit!" );
        }
        if ( other.CompareTag ( "Monster" ) )
        {
            MonsterListInRoom.Remove ( other.transform.parent.gameObject );  // 변경
        }
    }
}
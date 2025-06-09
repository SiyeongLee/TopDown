using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Cell : MonoBehaviour
{
    public bool isChecked;
    public bool isVisited;
    public bool isBossRoom;
    public int id;
    public int difficulty;
    public float cellSize;
    public Vector2Int pos;
    public Vector2 posWorld;
    public Vector3Int tilemapLocalPos;
    private SpriteRenderer minimapSprite;
    public UnityEngine.Color activeColor;
    public UnityEngine.Color deactiveColor;
    public Dictionary<int, GameObject> walls;
    public List<Door> doors;
    public List<Vector3> spawnPosList;
    
    public Cell( Vector2Int pos, Vector2 posWorld, int size )
    {
        this.pos = pos;
        this.posWorld = posWorld;
    }

    private void Awake()
    {
        minimapSprite = transform.Find( "minimapSprite" ).GetComponent<SpriteRenderer>();
        walls = new Dictionary<int, GameObject>
        {
            { 0, null },
            { 1, null },
            { 2, null },
            { 3, null }
        };

        doors = new List<Door>();
        spawnPosList = new List<Vector3>();
    }

    public void InitCell( Vector2Int pos )
    {
        this.pos = pos;
    }

    public void InitMinimapSprite( Sprite _sprite, Quaternion _rotate )
    {
        minimapSprite.sprite = _sprite;
        minimapSprite.transform.localRotation = _rotate;
    }

    public void SetCameraPos( int id )
    {
        if (GameTestManager.GetInstance().DoNotMoveCameraMode)
            return;

        DungeonManager.GetInstance().SetPlayerRoomID( id );
        DungeonManager.GetInstance().SetMainCameraPos();
    }

    public void SetDoorsEnable( bool isEnable )
    {
        foreach (Door curDoor in doors)
        {
            curDoor.enabled = isEnable;
        }
    }

    public void SetSpawnPosList(Vector3 pos)
    {
        spawnPosList.Add( pos + new Vector3( cellSize / 4, cellSize / 4, 0 ) );
        spawnPosList.Add( pos + new Vector3( -cellSize / 4, cellSize / 4, 0 ) );
        spawnPosList.Add( pos + new Vector3( cellSize / 4, -cellSize / 4, 0 ) );
        spawnPosList.Add( pos + new Vector3( -cellSize / 4, -cellSize / 4, 0 ) );
    }
}

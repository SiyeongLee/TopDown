using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct FTileInfoByCellID
{
    public Tilemap tilemap;
    public Vector3Int pos;

    public FTileInfoByCellID(Tilemap _tilemap, Vector3Int _pos)
    {
        tilemap = _tilemap;
        pos = _pos;
    }
};

public class DungeonManager : MonoBehaviour
{
    private static DungeonManager instance;

    public int enemyCount;
    public int playerRoomID;
    public int difficulty;
    public int cellSize;
    public int tileNumPerCell;
    public Player player;
    public Camera mainCamera;
    public Camera minimapCamera;
    public Cell[,] cellList;
    public Tilemap tilemap;
    public Dictionary<int, List<FTileInfoByCellID>> tilemapDic;
    public Dictionary<int, List<Door>> doorDic;
    public Dictionary<int, HashSet<Cell>> sameRoomDic;  // id, 해당 id의 cell들
    public Dictionary<int, HashSet<Cell>> adjacentCellDic;  // id, 해당 id와 인접한 cell들
    public HashSet<int> isRoomVisited;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy( gameObject );
        }

        sameRoomDic = new Dictionary<int, HashSet<Cell>>();
        tilemapDic = new Dictionary<int, List<FTileInfoByCellID>>();
        doorDic = new Dictionary<int, List<Door>>();
        adjacentCellDic = new Dictionary<int, HashSet<Cell>>();
        isRoomVisited = new HashSet<int>();
    }

    public static DungeonManager GetInstance()
    {
        return instance;
    }

    public void AddToSameRoomDic(Cell cell)
    {
        if (sameRoomDic.ContainsKey(cell.id ))
        {
            sameRoomDic[cell.id].Add( cell );
        }
        else
        {
            sameRoomDic.Add( cell.id, new HashSet<Cell>() { cell } );
        }
    }

    public void SetPlayerRoomID(int id)
    {
        playerRoomID = id;
    }

    public void SetPlayerPos(Vector3Int pos)
    {
        player.transform.position = tilemap.CellToWorld(pos);
    }

    public void SetMainCameraPos()
    {
        Vector3 pos = new Vector3( 0, 0, 0 );

        if (!sameRoomDic.ContainsKey( playerRoomID ))
            return;

        foreach (Cell cell in sameRoomDic[playerRoomID])
        {
            pos += cell.transform.position;
        }

        pos /= sameRoomDic[playerRoomID].Count;
        mainCamera.transform.position = new Vector3( pos.x, pos.y, -10 );
        minimapCamera.transform.position = new Vector3( pos.x, pos.y, -10 );
    }

    public void SetPlayerTransform(Vector2 pos, float size)
    {
        player.transform.position = pos;
        player.transform.localScale = new Vector3( size, size, 0 );
    }

    public void AddToTilemapDic( int id, Tilemap tilemapType, Vector3Int pos )
    {
        FTileInfoByCellID tileInfo = new FTileInfoByCellID( tilemapType, pos );
        if (tilemapDic.ContainsKey( id ))
        {
            tilemapDic[id].Add( tileInfo );
        }
        else
        {
            tilemapDic.Add( id, new List<FTileInfoByCellID>() { tileInfo } );
        }
    }

    public void AddToDoorDic( int id, Door door )
    {
        if (doorDic.ContainsKey( id ))
        {
            doorDic[id].Add( door );
        }
        else
        {
            doorDic.Add( id, new List<Door>() { door } );
        }
    }

    public void SetVisibilityTiles(int id, bool isVisible)
    {
        if (tilemapDic.ContainsKey(id))
        { 
            foreach (FTileInfoByCellID tileInfo in tilemapDic[id])
            {
                Tilemap tilemapType = tileInfo.tilemap;
                Vector3Int pos = tileInfo.pos;

                Color color = tilemapType.GetColor( pos );
                if (isVisible)
                {
                    color.a = 1;
                }
                else
                {
                    color.a = 0;
                }
                tilemapType.SetColor( pos, color );
            }

            foreach (Door door in doorDic[id])
            {
                door.SetVisibility( isVisible );
                if (isVisible)
                {
                    door.GetComponent<BoxCollider2D>().isTrigger = true;
                }
                else
                {
                    door.GetComponent<BoxCollider2D>().isTrigger = false;
                }
            }
        }
    }

    public void ActivateMinimap( int id, bool isActivate )
    {
        foreach(Cell cell in sameRoomDic[id])
        {
            SpriteRenderer minimapRenderer = cell.transform.Find( "minimapSprite" ).GetComponent<SpriteRenderer>();
            cell.isVisited = true;
            if (isActivate)
            {
                minimapRenderer.color = cell.activeColor;
            }
            else
            {
                minimapRenderer.color = cell.deactiveColor;
            }
        }
    }

    public void SetVisibilityMinimap( int id, bool isActivate )
    {
        foreach (Cell cell in sameRoomDic[id])
        {
            SpriteRenderer minimapRenderer = cell.transform.Find( "minimapSprite" ).GetComponent<SpriteRenderer>();
            if (isActivate)
            {
                Color color = minimapRenderer.color;
                color.a = 1;
                minimapRenderer.color = color;
            }
            else
            {
                Color color = minimapRenderer.color;
                color.a = 0;
                minimapRenderer.color = color;
            }
        }
    }

    public bool IsCellAdjacent( Cell prevCell, Cell postCell )
    {
        return adjacentCellDic.ContainsKey( prevCell.id ) && adjacentCellDic[prevCell.id].Contains( postCell );
    }

    public void AddAdjacentID( Cell prevCell, Cell postCell )
    {
        if (adjacentCellDic.ContainsKey( prevCell.id ))
        {
            adjacentCellDic[prevCell.id].Add( postCell );
        }
        else
        {
            adjacentCellDic.Add( prevCell.id, new HashSet<Cell>() { postCell } );
        }
        if (adjacentCellDic.ContainsKey( postCell.id ))
        {
            adjacentCellDic[postCell.id].Add( prevCell );
        }
        else
        {
            adjacentCellDic.Add( postCell.id, new HashSet<Cell>() { prevCell } );
        }
    }

    public void EnterRoom(bool isBossRoom)
    {
        SetDoorEnabled( false );
        isRoomVisited.Add( playerRoomID );
        if (isBossRoom)
        {
            SetDifficulty();
            SetBoss();
        }
        else
        {
            SetDifficulty();
            SetEnemy();
        }
    }

    public void ClearRoom()
    {
        SetDoorEnabled( true );
    }

    public void SetDifficulty()
    {
        difficulty++;
    }

    public void SetEnemy()
    {
        Enemy enemy;
        enemyCount = 0;
        foreach (Cell cell in sameRoomDic[playerRoomID])
        {
            foreach (Vector3 spawnPos in cell.spawnPosList)
            {
                enemy = PoolingManager.GetInstance().enemyPool.Get().GetComponent<Enemy>();
                enemy.transform.position = spawnPos;
                enemy.SetStat(difficulty);
                enemy.transform.localScale = tilemap.cellSize * 0.5f;
                enemyCount++;
            }
        }
    }

    public void SetBoss()
    {
        Vector3Int spawnPos = Vector3Int.zero;
        foreach (Cell cell in sameRoomDic[playerRoomID])
        {
            spawnPos += new Vector3Int( cell.tilemapLocalPos.x, cell.tilemapLocalPos.y, 0 );
        }
        spawnPos = (spawnPos / sameRoomDic[playerRoomID].Count) + new Vector3Int( tileNumPerCell / 2, tileNumPerCell / 2, 0 );

        Boss boss = PoolingManager.GetInstance().enemyPool.Get().GetComponent<Boss>();
        boss.enabled = true;
        boss.GetComponent<Enemy>().enabled = false;

        boss.transform.position = tilemap.CellToWorld( spawnPos );
        boss.SetStat( difficulty );
        boss.transform.localScale = tilemap.cellSize;
    }

    public void AddEnemy(int count)
    {
        enemyCount += count;
        if (enemyCount <= 0)
        {
            ClearRoom();
        }
    }

    public void SetDoorEnabled(bool isEnabled)
    {
        if (!doorDic.ContainsKey( playerRoomID )) return;
        foreach (Door door in doorDic[playerRoomID])
        {
            door.SetDoorEnabled( isEnabled );
        }
    }
}

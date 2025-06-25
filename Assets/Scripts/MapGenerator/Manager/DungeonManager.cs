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
    public Dictionary<int, HashSet<Cell>> adjacentCellDic;  // id, 해당 id의 인접한 cell들
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
        Debug.Log($"[DungeonManager] EnterRoom 호출됨 - 보스방: {isBossRoom}");
        
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
        Debug.Log($"[DungeonManager] ClearRoom 호출됨 - 방 ID: {playerRoomID} (도어는 이미 열려있음)");
        // SetDoorEnabled( true ); // 제거 - 도어가 이미 열려있으므로 불필요
    }

    public void SetDifficulty()
    {
        difficulty++;
    }

    public void SetEnemy()
    {
        Debug.Log($"[DungeonManager] SetEnemy 호출됨 - 현재 방 ID: {playerRoomID}");
        
        if (!sameRoomDic.ContainsKey(playerRoomID))
        {
            Debug.LogError($"[DungeonManager] 방 ID {playerRoomID}가 sameRoomDic에 없습니다!");
            return;
        }
        
        Debug.Log($"[DungeonManager] 방의 Cell 수: {sameRoomDic[playerRoomID].Count}");
        
        Enemy enemy;
        enemyCount = 0;
        int maxEnemies = 5; // 최대 5마리로 제한
        
        foreach (Cell cell in sameRoomDic[playerRoomID])
        {
            Debug.Log($"[DungeonManager] Cell {cell.name} 처리 중 - spawnPosList 크기: {cell.spawnPosList.Count}");
            
            foreach (Vector3 spawnPos in cell.spawnPosList)
            {
                // 최대 5마리까지만 스폰
                if (enemyCount >= maxEnemies)
                {
                    Debug.Log($"[DungeonManager] 최대 적 수({maxEnemies}마리)에 도달하여 스폰 중단");
                    break;
                }
                
                Debug.Log($"[DungeonManager] Enemy 스폰 시도 - 위치: {spawnPos}");
                
                enemy = PoolingManager.GetInstance().enemyPool.Get().GetComponent<Enemy>();
                if (enemy == null)
                {
                    Debug.LogError("[DungeonManager] Enemy 컴포넌트를 가져올 수 없습니다!");
                    continue;
                }
                
                enemy.transform.position = spawnPos;
                enemy.SetStat(difficulty);
                enemy.transform.localScale = tilemap.cellSize * 0.5f;
                enemyCount++;
                
                Debug.Log($"[DungeonManager] 적 스폰 성공 - 번호: {enemyCount}, 위치: {spawnPos}, Enemy 스크립트 활성화: {enemy.enabled}");
            }
            
            // 최대 5마리까지만 스폰
            if (enemyCount >= maxEnemies)
            {
                break;
            }
        }
        
        Debug.Log($"[DungeonManager] 적 스폰 완료 - 총 {enemyCount}마리");
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
        int oldEnemyCount = enemyCount;
        enemyCount += count;
        
        Debug.Log($"[DungeonManager] AddEnemy 호출됨 - 변경량: {count}, 이전 적 수: {oldEnemyCount}, 현재 적 수: {enemyCount}");
        
        // 더 이상 적 카운트가 0이 되어도 도어를 열지 않음 (이미 열려있음)
        // if (enemyCount <= 0)
        // {
        //     Debug.Log("[DungeonManager] 모든 적이 제거됨 - 방 클리어!");
        //     ClearRoom();
        // }
    }

    public void SetDoorEnabled(bool isEnabled)
    {
        Debug.Log($"[DungeonManager] SetDoorEnabled 호출됨 - 방 ID: {playerRoomID}, 활성화: {isEnabled}");
        
        if (!doorDic.ContainsKey( playerRoomID ))
        {
            Debug.LogError($"[DungeonManager] 방 ID {playerRoomID}에 도어가 없습니다!");
            return;
        }
        
        Debug.Log($"[DungeonManager] 도어 수: {doorDic[playerRoomID].Count}");
        
        foreach (Door door in doorDic[playerRoomID])
        {
            door.SetDoorEnabled( isEnabled );
            Debug.Log($"[DungeonManager] 도어 {door.name} 활성화: {isEnabled}");
        }
    }
}

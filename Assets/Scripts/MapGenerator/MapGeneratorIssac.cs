using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum ETileType
{
    eGround,
    eWall,
    eDoor,
}

public enum EDir
{
    eRight,
    eLeft,
    eDown,
    eUp,
    eLeftUp,
    eRightUp,
    eLeftDown,
    eRightDown,
}

public class MapGeneratorIssac : MonoBehaviour
{
    bool DrawnBossCell = false;
    bool isBossCell = false;
    private float tileSizePerCell;
    private int roomID = 1;
    private int bossRoomID = -1;
    [SerializeField] private int roomDepth = 10;
    [SerializeField] private int cellSize = 30;
    [SerializeField, Tooltip("cellSize % tileNumPerCell == 0")] private int tileNumPerCell = 30;
    private Vector2Int mapSize;
    [SerializeField] private GameObject cellObj;
    [SerializeField] private GameObject groundObj;
    [SerializeField] private GameObject wallObj;
    [SerializeField] private GameObject doorObj;
    [SerializeField] private GameObject tileBaseObj;
    [SerializeField] private GameObject player;
    [SerializeField] private Sprite groundSprite;
    [SerializeField] private Sprite wallSprite;
    [SerializeField] private Sprite doorSprite;
    [SerializeField] private Sprite minimap4Walls;
    [SerializeField] private Sprite minimap3Walls;
    [SerializeField] private Sprite minimap2Walls;
    [SerializeField] private GameObject cellParent;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase doorTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase bossGroundTile;
    [SerializeField] private Sprite bossDoorSprite;
    [SerializeField] private GameObject grid;
    [SerializeField] private Cell[,] cellList;


    // ������, ����, �Ʒ�, ��, ���ʾƷ�, �����ʾƷ�, ������, ��������
    private int[] xdir = new int[] { 0, 0, 1, -1, 1, 1, -1, -1 };
    private int[] ydir = new int[] { 1, -1, 0, 0, -1, 1, -1, 1 };

    public Vector2Int[][][] RoomTypes = new Vector2Int[][][]
    {
        new Vector2Int[][]
        {
            new Vector2Int[] { new Vector2Int( 0, 0 ) }
        },
        new Vector2Int[][]
        {
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 0, 1 ) },
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 1, 0 ) },
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( -1, 0 ) },
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 0, -1 ) },
        },
        new Vector2Int[][]
        {
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 0, 1 ), new Vector2Int(-1, 0) },              // ������ ��
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 0, 1 ), new Vector2Int(1, 0) },              // ������ �Ʒ�
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 1, 0 ), new Vector2Int(0, 1) },              // �Ʒ� ������
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 1, 0 ), new Vector2Int(0, -1) },              // �Ʒ� ����
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 0, -1 ), new Vector2Int(-1, 0) },              // ���� ��
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 0, -1 ), new Vector2Int(1, 0) },              // ���� �Ʒ�
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( -1, 0 ), new Vector2Int(0, 1) },              // �� ������
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( -1, 0 ), new Vector2Int(0, -1) },              // �� ����          
        },
        new Vector2Int[][]
        {
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 1, 0 ), new Vector2Int(1, 1), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( 1, 0 ), new Vector2Int(1,- 1), new Vector2Int(0, -1) },
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( -1, 0 ), new Vector2Int(-1, 1), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int( 0, 0 ), new Vector2Int( -1, 0 ), new Vector2Int(-1, -1), new Vector2Int(0, -1) }
        }
    };

    void Start()
    {
        InitMap();
        GenerateRoom(cellList[cellList.GetLength(1) / 2, cellList.GetLength(0) / 2]);

        // 플레이어 할당 확인
        if (player == null)
        {
            Debug.LogError("Player is not assigned in MapGeneratorIssac!");
        }
        else
        {
            Debug.Log($"Player assigned: {player.name}");
        }

        InitPlayer();
        InitMapInvisible();
    }

    void InitMap()
    {
        int cellNum = roomDepth * 4;
        mapSize = new Vector2Int(cellNum * cellSize, cellNum * cellSize);
        cellList = new Cell[cellNum, cellNum];
        tileSizePerCell = cellSize / tileNumPerCell;
        DungeonManager.GetInstance().tileNumPerCell = tileNumPerCell;
        DungeonManager.GetInstance().cellSize = cellSize;
        for (int i = 0; i < cellNum; i++)
        {
            for (int j = 0; j < cellNum; j++)
            {
                cellList[i, j] = Instantiate(cellObj).GetComponent<Cell>();
                cellList[i, j].InitCell(new Vector2Int(i, j));
                cellList[i, j].transform.localScale = new Vector3(cellSize, cellSize, 0);
                cellList[i, j].transform.position = new Vector3(-mapSize.x / 2 + i * cellSize + cellSize / 2, -mapSize.y / 2 + j * cellSize + cellSize / 2, 10);
                cellList[i, j].transform.parent = cellParent.transform;
                cellList[i, j].tilemapLocalPos = new Vector3Int(-mapSize.x / 2 + tileNumPerCell * i, -mapSize.x / 2 + tileNumPerCell * j, 0);
                cellList[i, j].cellSize = cellSize;
            }
        }
    }

    void InitMapInvisible()
    {
        if (!GameTestManager.GetInstance().allMapVisibleMode)
        {
            for (int id = 2; id < roomID; id++)
            {
                DungeonManager.GetInstance().SetVisibilityTiles(id, false);
                DungeonManager.GetInstance().SetVisibilityMinimap(id, false);
            }
        }
    }

    void InitPlayer()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player 오브젝트가 씬에 없습니다! Inspector에서 player 필드를 할당하거나, Player 오브젝트에 'Player' 태그를 지정하세요.");
                return;
            }
        }

        // DungeonManager null 체크 추가
        if (DungeonManager.GetInstance() == null)
        {
            Debug.LogError("DungeonManager instance is null!");
            return;
        }

        // DungeonManager의 player 필드 설정
        DungeonManager.GetInstance().player = player.GetComponent<Player>();

        // 플레이어 위치 설정 - 중앙 셀의 중앙에 위치
        Vector3Int centerCellPos = new Vector3Int(cellList.GetLength(1) / 2, cellList.GetLength(0) / 2, 0);
        Vector3Int playerTilePos = new Vector3Int(
            cellList[centerCellPos.x, centerCellPos.y].tilemapLocalPos.x + tileNumPerCell / 2,
            cellList[centerCellPos.x, centerCellPos.y].tilemapLocalPos.y + tileNumPerCell / 2,
            0
        );
        
        Vector3 playerWorldPos = groundTilemap.CellToWorld(playerTilePos);
        playerWorldPos.z = 0; // Z 위치를 0으로 설정하여 카메라 앞에 위치
        
        // 플레이어 위치와 스케일 설정
        player.transform.position = playerWorldPos;
        
        // DungeonManager에 플레이어 위치 정보 전달
        DungeonManager.GetInstance().SetPlayerTransform(playerWorldPos, 1f);

        // 플레이어 속도 조정
        player.GetComponent<Player>().moveSpeed *= tileSizePerCell * 0.5f;
        
        // 플레이어가 보이도록 SpriteRenderer 활성화 확인
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            playerSprite.enabled = true;
            // 스프라이트는 Player 스크립트에서 자체적으로 관리하므로 여기서 설정하지 않음
        }
        
        Debug.Log($"Player initialized at position: {playerWorldPos}, scale: {player.transform.localScale}");
    }

    /*
     * pos: room's left down pos
     * isSpecialRoom: start or boss room
     */
    void GenerateRoom(Cell stdCell)
    {
        int nx, ny;

        if (roomID > roomDepth)
            return;

        (bool, List<Cell>) checkRoomResult = CheckValidRoom(stdCell);

        if (checkRoomResult.Item1 == false)
            return;

        DrawCell(checkRoomResult.Item2);
        roomID++;

        // �ֺ� ������� ���� cell Ž��
        HashSet<Cell> nearCells = GetNearCells(checkRoomResult.Item2);
        foreach (Cell cell in nearCells)
        {
            GenerateRoom(cell);
            if (roomID > roomDepth)
            {
                isBossCell = true;
                if (!DrawnBossCell)
                {
                    DrawBossRoom(cell);
                }
                return;
            }
        }
    }

    void DrawBossRoom(Cell cell)
    {
        (bool, List<Cell>) checkBossRoomResult = CheckBossRoom(cell);
        if (checkBossRoomResult.Item1)
        {
            DrawCell(checkBossRoomResult.Item2);
            DrawnBossCell = true;
        }
    }

    void DrawCell(List<Cell> cells)
    {
        int nx, ny;
        foreach (Cell cell in cells)
        {
            cell.isChecked = true;
            cell.id = roomID;
            if (isBossCell)
            {
                cell.isBossRoom = true;
            }
        }

        SetTileMap(cells);
        foreach (Cell cell in cells)
        {
            for (int i = 0; i < 4; i++)
            {
                nx = cell.pos.x + xdir[i];
                ny = cell.pos.y + ydir[i];
                if (0 < nx && nx <= cellList.GetLength(1) && 0 < ny && ny <= cellList.GetLength(0) && cellList[nx, ny].id != cell.id && cellList[nx, ny].id != 0)
                {
                    if (DungeonManager.GetInstance().IsCellAdjacent(cell, cellList[nx, ny]))
                        continue;

                    DungeonManager.GetInstance().AddAdjacentID(cell, cellList[nx, ny]);

                    if (i == 0 || i == 2) GenerateDoor(cell, cellList[nx, ny], (EDir)i);
                    else if (i == 1 || i == 3) GenerateDoor(cellList[nx, ny], cell, (EDir)i);
                    if (isBossCell) return;
                }
            }
        }
    }

    void SetTileMap(List<Cell> cells)
    {
        bool rOk, lOk, uOk, dOk, ldOk, rdOk, luOk, ruOk;
        int rx, ry, lx, ly, ux, uy, dx, dy, ldx, ldy, rdx, rdy, lux, luy, rux, ruy;
        foreach (Cell cell in cells)
        {
            rx = cell.pos.x + xdir[0];
            ry = cell.pos.y + ydir[0];
            lx = cell.pos.x + xdir[1];
            ly = cell.pos.y + ydir[1];
            dx = cell.pos.x + xdir[2];
            dy = cell.pos.y + ydir[2];
            ux = cell.pos.x + xdir[3];
            uy = cell.pos.y + ydir[3];
            ldx = cell.pos.x + xdir[4];
            ldy = cell.pos.y + ydir[4];
            rdx = cell.pos.x + xdir[5];
            rdy = cell.pos.y + ydir[5];
            lux = cell.pos.x + xdir[6];
            luy = cell.pos.y + ydir[6];
            rux = cell.pos.x + xdir[7];
            ruy = cell.pos.y + ydir[7];

            rOk = 0 <= rx && rx < cellList.GetLength(1) && 0 <= ry && ry < cellList.GetLength(0) && cellList[rx, ry].id == cell.id;
            lOk = 0 <= lx && lx < cellList.GetLength(1) && 0 <= ly && ly < cellList.GetLength(0) && cellList[lx, ly].id == cell.id;
            uOk = 0 <= ux && ux < cellList.GetLength(1) && 0 <= uy && uy < cellList.GetLength(0) && cellList[ux, uy].id == cell.id;
            dOk = 0 <= dx && dx < cellList.GetLength(1) && 0 <= dy && dy < cellList.GetLength(0) && cellList[dx, dy].id == cell.id;
            ldOk = 0 <= ldx && ldx < cellList.GetLength(1) && 0 <= ldy && ldy < cellList.GetLength(0) && cellList[ldx, ldy].id == cell.id;
            rdOk = 0 <= rdx && rdx < cellList.GetLength(1) && 0 <= rdy && rdy < cellList.GetLength(0) && cellList[rdx, rdy].id == cell.id;
            luOk = 0 <= lux && lux < cellList.GetLength(1) && 0 <= luy && luy < cellList.GetLength(0) && cellList[lux, luy].id == cell.id;
            ruOk = 0 <= rux && rux < cellList.GetLength(1) && 0 <= ruy && ruy < cellList.GetLength(0) && cellList[rux, ruy].id == cell.id;

            DrawTilesInCell(cell);
            if (!uOk) DrawWall(cell, EDir.eUp);
            if (!dOk) DrawWall(cell, EDir.eDown);
            if (!rOk) DrawWall(cell, EDir.eRight);
            if (!lOk) DrawWall(cell, EDir.eLeft);
            if (!luOk) DrawWall(cell, EDir.eLeftUp);
            if (!ruOk) DrawWall(cell, EDir.eRightUp);
            if (!ldOk) DrawWall(cell, EDir.eLeftDown);
            if (!rdOk) DrawWall(cell, EDir.eRightDown);
            DrawMinimap(cell, dOk, uOk, lOk, rOk);
        }
    }

    private void DrawMinimap(Cell cell, bool dOk, bool uOk, bool lOk, bool rOk)
    {
        if (!dOk && !uOk && !lOk && !rOk)
        {
            //posWorld
            cell.InitMinimapSprite(minimap4Walls, Quaternion.Euler(0, 0, 0));
        }
        else if (!dOk && !uOk && !lOk)
        {
            cell.InitMinimapSprite(minimap3Walls, Quaternion.Euler(0, 0, 180));
        }
        else if (!dOk && !rOk && !lOk)
        {
            cell.InitMinimapSprite(minimap3Walls, Quaternion.Euler(0, 0, 270));
        }
        else if (!dOk && !rOk && !uOk)
        {
            cell.InitMinimapSprite(minimap3Walls, Quaternion.Euler(0, 0, 0));
        }
        else if (!lOk && !uOk && !rOk)
        {
            cell.InitMinimapSprite(minimap3Walls, Quaternion.Euler(0, 0, 90));
        }
        else if (!lOk && !uOk)
        {
            cell.InitMinimapSprite(minimap2Walls, Quaternion.Euler(0, 0, 90));
        }
        else if (!lOk && !dOk)
        {
            cell.InitMinimapSprite(minimap2Walls, Quaternion.Euler(0, 0, 180));
        }
        else if (!rOk && !dOk)
        {
            cell.InitMinimapSprite(minimap2Walls, Quaternion.Euler(0, 0, -90));
        }
        else if (!uOk && !rOk)
        {
            cell.InitMinimapSprite(minimap2Walls, Quaternion.Euler(0, 0, 0));
        }
    }

    void DrawTilesInCell(Cell cell)
    {
        Vector3Int curPos;
        for (int i = 0; i < tileNumPerCell; i++)
        {
            for (int j = 0; j < tileNumPerCell; j++)
            {
                curPos = new Vector3Int(cell.tilemapLocalPos.x + i, cell.tilemapLocalPos.y + j, 0);

                if (!isBossCell)
                {
                    groundTilemap.SetTile(curPos, groundTile);
                }
                else
                {
                    groundTilemap.SetTile(curPos, bossGroundTile);
                }

                DungeonManager.GetInstance().AddToTilemapDic(roomID, groundTilemap, curPos);
            }
        }

        //  ߾Ӱ ϱ
        //cell.posWorld = groundTilemap.CellToWorld( new Vector3Int( cell.tilemapLocalPos.x + tileNumPerCell / 2, cell.tilemapLocalPos.y + tileNumPerCell / 2, 0 ) );
        Vector2 pos = new Vector2(cell.tilemapLocalPos.x + tileNumPerCell / 2, cell.tilemapLocalPos.y + tileNumPerCell / 2);
        DungeonManager.GetInstance().AddToSameRoomDic(cell);

        // set enemy spawn pos - 월드 좌표로 변환하여 전달
        Vector3 worldPos = groundTilemap.CellToWorld(new Vector3Int(cell.tilemapLocalPos.x + tileNumPerCell / 2, cell.tilemapLocalPos.y + tileNumPerCell / 2, 0));
        cell.SetSpawnPosList(worldPos);
    }

    void DrawWall(Cell cell, EDir dir)
    {
        Vector3Int pos = new Vector3Int(0, 0, 0);
        switch (dir)
        {
            case EDir.eDown:
                for (int i = 0; i < tileNumPerCell; i++)
                {
                    pos = new Vector3Int(cell.tilemapLocalPos.x + tileNumPerCell - 1, cell.tilemapLocalPos.y + i);
                    SetWallTile(cell, pos);
                }
                break;
            case EDir.eUp:
                for (int i = 0; i < tileNumPerCell; i++)
                {
                    pos = new Vector3Int(cell.tilemapLocalPos.x, cell.tilemapLocalPos.y + i);
                    SetWallTile(cell, pos);
                }
                break;
            case EDir.eLeft:
                for (int i = 0; i < tileNumPerCell; i++)
                {
                    pos = new Vector3Int(cell.tilemapLocalPos.x + i, cell.tilemapLocalPos.y);
                    SetWallTile(cell, pos);
                }
                break;
            case EDir.eRight:
                for (int i = 0; i < tileNumPerCell; i++)
                {
                    pos = new Vector3Int(cell.tilemapLocalPos.x + i, cell.tilemapLocalPos.y + tileNumPerCell - 1);
                    SetWallTile(cell, pos);
                }
                break;
            case EDir.eLeftUp:
                pos = new Vector3Int(cell.tilemapLocalPos.x, cell.tilemapLocalPos.y);
                SetWallTile(cell, pos);
                break;
            case EDir.eRightUp:
                pos = new Vector3Int(cell.tilemapLocalPos.x, cell.tilemapLocalPos.y + tileNumPerCell - 1);
                SetWallTile(cell, pos);
                break;
            case EDir.eLeftDown:
                pos = new Vector3Int(cell.tilemapLocalPos.x + tileNumPerCell - 1, cell.tilemapLocalPos.y);
                SetWallTile(cell, pos);
                break;
            case EDir.eRightDown:
                pos = new Vector3Int(cell.tilemapLocalPos.x + tileNumPerCell - 1, cell.tilemapLocalPos.y + tileNumPerCell - 1);
                SetWallTile(cell, pos);
                break;
            default:
                break;
        }
    }

    void SetWallTile(Cell cell, Vector3Int pos)
    {
        wallTilemap.SetTile(pos, wallTile);
        DungeonManager.GetInstance().AddToTilemapDic(roomID, wallTilemap, pos);
    }

    public Door DrawDoor(Cell cell, EDir dir)
    {
        Vector3Int pos;
        Door doorInstance = Instantiate(doorObj).GetComponent<Door>();

        if (isBossCell) doorInstance.GetComponent<SpriteRenderer>().sprite = bossDoorSprite;

        doorInstance.OwnerCell = cell;
        switch (dir)
        {
            case EDir.eLeft:
                pos = new Vector3Int(cell.tilemapLocalPos.x + tileNumPerCell / 2, cell.tilemapLocalPos.y);
                break;
            case EDir.eRight:
                pos = new Vector3Int(cell.tilemapLocalPos.x + tileNumPerCell / 2, cell.tilemapLocalPos.y + tileNumPerCell - 1);
                break;
            case EDir.eUp:
                pos = new Vector3Int(cell.tilemapLocalPos.x, cell.tilemapLocalPos.y + tileNumPerCell / 2);
                break;
            case EDir.eDown:
                pos = new Vector3Int(cell.tilemapLocalPos.x + tileNumPerCell - 1, cell.tilemapLocalPos.y + tileNumPerCell / 2);
                break;
            default:
                pos = new Vector3Int(0, 0, 0);
                break;
        }

        wallTilemap.SetTile(pos, null);
        DungeonManager.GetInstance().AddToDoorDic(cell.id, doorInstance);
        doorInstance.transform.position = groundTilemap.CellToWorld(pos);
        return doorInstance;
    }

    void GenerateDoor(Cell prevCell, Cell postCell, EDir dir)
    {
        Door prevDoor, postDoor;
        // �� ����, ��ġ ��å
        if (dir == EDir.eRight || dir == EDir.eLeft)
        {
            prevDoor = DrawDoor(prevCell, EDir.eRight);
            postDoor = DrawDoor(postCell, EDir.eLeft);
        }
        else
        {
            prevDoor = DrawDoor(prevCell, EDir.eDown);
            postDoor = DrawDoor(postCell, EDir.eUp);
        }

        prevDoor.NextDoor = postDoor;
        postDoor.NextDoor = prevDoor;
        prevDoor.NextDoor.OwnerCell = postCell;
        postDoor.NextDoor.OwnerCell = prevCell;
    }

    (bool, List<Cell>) CheckValidRoom(Cell cell)
    {
        bool canGenerate;
        int nx, ny;
        int roomTypeNum = RoomTypes.Length;
        List<Cell> posList = new List<Cell>();
        // ����� ũ�⸦ ���ϱ� ���� ���� �迭
        int[] currentCellSizeArray = new int[roomTypeNum];
        // ũ�Ⱑ ������ ��� �� � ����� ���������� ���� ���� �迭
        int[] currentRoomTypeArray;
        // ��� ����� �� �� ��ǥ
        Vector2Int[] roomBlocks;

        for (int i = 0; i < roomTypeNum; i++)
            currentCellSizeArray[i] = i;

        currentCellSizeArray = ShuffleArray<int>(currentCellSizeArray);

        foreach (int currentcellSize in currentCellSizeArray)
        {
            currentRoomTypeArray = new int[RoomTypes[currentcellSize].Length];

            for (int i = 0; i < currentRoomTypeArray.Length; i++)
                currentRoomTypeArray[i] = i;

            currentRoomTypeArray = ShuffleArray<int>(currentRoomTypeArray);
            foreach (int currentRoomType in currentRoomTypeArray)
            {
                // ��� ����� �� �� ��ǥ
                roomBlocks = RoomTypes[currentcellSize][currentRoomType];
                canGenerate = true;
                posList.Clear();
                foreach (Vector2Int blockPos in roomBlocks)
                {
                    nx = cell.pos.x + blockPos.x;
                    ny = cell.pos.y + blockPos.y;
                    // ���� ����ų� �̹� ������ ���̸� �ٸ� �ڸ��� ã�ƺ��� �Ѵ�.
                    if (nx < 0 || nx >= cellList.GetLength(1) || ny < 0 || ny >= cellList.GetLength(0) || cellList[nx, ny].isChecked)
                    {
                        canGenerate = false;
                        break;
                    }
                    posList.Add(cellList[nx, ny]);
                }
                if (canGenerate)
                {
                    return (true, posList);
                }
            }
        }
        return (false, posList);
    }

    (bool, List<Cell>) CheckBossRoom(Cell cell)
    {
        bool canGenerate = false;
        int[] currentRoomTypeArray = new int[RoomTypes[3].Length];
        Vector2Int[] poses;
        int nx, ny;
        List<Cell> posList = new List<Cell>();

        for (int i = 0; i < currentRoomTypeArray.Length; i++)
            currentRoomTypeArray[i] = i;

        currentRoomTypeArray = ShuffleArray<int>(currentRoomTypeArray);
        foreach (int currentRoomType in currentRoomTypeArray)
        {
            poses = RoomTypes[3][currentRoomType];
            canGenerate = true;
            posList.Clear();
            foreach (Vector2Int pos in poses)
            {
                nx = cell.pos.x + pos.x;
                ny = cell.pos.y + pos.y;
                if (nx < 0 || nx >= cellList.GetLength(1) || ny < 0 || ny >= cellList.GetLength(0) || cellList[nx, ny].isChecked)
                {
                    canGenerate = false;
                    break;
                }
                posList.Add(cellList[nx, ny]);
            }
            if (canGenerate)
            {
                return (true, posList);
            }
        }
        Debug.Log("00000");
        return (false, posList);
    }

    // (����, ����)
    HashSet<Cell> GetNearCells(List<Cell> suburbCellList)
    {
        int nx, ny;
        HashSet<Cell> nearCells = new HashSet<Cell>();
        suburbCellList = ShuffleList<Cell>(suburbCellList);
        foreach (Cell curPos in suburbCellList)
        {
            for (int i = 0; i < 4; i++)
            {
                nx = curPos.pos.x + xdir[i];
                ny = curPos.pos.y + ydir[i];
                if (nx < 0 || nx >= mapSize.x || ny < 0 || ny >= mapSize.y || cellList[nx, ny].isChecked) continue;
                nearCells.Add(cellList[nx, ny]);
            }
        }
        return nearCells;
    }

    private T[] ShuffleArray<T>(T[] array)
    {
        int random1, random2;
        T temp;

        for (int i = 0; i < array.Length; ++i)
        {
            random1 = UnityEngine.Random.Range(0, array.Length);
            random2 = UnityEngine.Random.Range(0, array.Length);

            temp = array[random1];
            array[random1] = array[random2];
            array[random2] = temp;
        }

        return array;
    }

    private List<T> ShuffleList<T>(List<T> list)
    {
        int random1, random2;
        T temp;

        for (int i = 0; i < list.Count; ++i)
        {
            random1 = UnityEngine.Random.Range(0, list.Count);
            random2 = UnityEngine.Random.Range(0, list.Count);

            temp = list[random1];
            list[random1] = list[random2];
            list[random2] = temp;
        }

        return list;
    }
}

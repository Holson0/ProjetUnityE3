using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;


public class LevelsGenerator : MonoBehaviour
{
    [Header("Références")]
    public Transform player;
    public int roomSize = 16;
    public int viewDistance = 1;
    public float chance_room_souvenir = 0.1f;
    public NavMeshSurface navMeshSurface;

    [Header("Prefabs")]
    public GameObject room_HB;
    public GameObject room_GD;
    public GameObject room_GHD;
    public GameObject room_GBD;
    public GameObject room_HGBD;
    public GameObject room_Souvenir;

    private Dictionary<Door, GameObject> roomPrefabs;
    private Dictionary<Vector2Int, Door> generatedRooms = new();
    private Dictionary<Vector2Int, GameObject> roomInstances = new();

    [Header("Monstres")]
    public GameObject[] monsterPrefabs;
    public float monsterSpawnProbability = 0.3f;


    private Vector2Int[] directions = {
        new Vector2Int(0, 1),   // top (Z+)
        new Vector2Int(0, -1),  // bottom (Z-)
        new Vector2Int(-1, 0),  // left (X-)
        new Vector2Int(1, 0)    // right (X+)
    };

    private Door[] doorFlags = {
        Door.Top,
        Door.Bottom,
        Door.Left,
        Door.Right
    };

    void Start()
    {
        roomPrefabs = new Dictionary<Door, GameObject>
        {
            { Door.Top | Door.Bottom, room_HB },
            { Door.Left | Door.Right, room_GD },
            { Door.Left | Door.Top | Door.Right, room_GHD },
            { Door.Left | Door.Bottom | Door.Right, room_GBD },
            { Door.Top | Door.Bottom | Door.Left | Door.Right, room_HGBD }
        };

        Vector2Int start = Vector2Int.zero;
        GenerateRoomAt(start, Door.Top | Door.Bottom | Door.Left | Door.Right); // start room
    }

    void Update()
    {
        Vector2Int playerPos = new Vector2Int(
            Mathf.RoundToInt(player.position.x / roomSize),
            Mathf.RoundToInt(player.position.z / roomSize)
        );

        GenerateAround(playerPos);
        UnloadFarRooms(playerPos);

    }

    void UpdateNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
    }

    void GenerateAround(Vector2Int center)
    {
        for (int dx = -viewDistance; dx <= viewDistance; dx++)
        {
            for (int dz = -viewDistance; dz <= viewDistance; dz++)
            {
                Vector2Int pos = center + new Vector2Int(dx, dz);
                if (!generatedRooms.ContainsKey(pos))
                {
                    Door requiredDoors = GetRequiredDoors(pos);
                    GenerateRoomAt(pos, requiredDoors);
                }
            }
        }
    }

    void SpawnMonstersInRoom(Vector3 roomPosition)
    {
        if (Random.value < monsterSpawnProbability)
        {
            int monsterCount = Random.Range(1, 3);

            for (int i = 0; i < monsterCount; i++)
            {
                Vector3 monsterPosition = new Vector3(
                    roomPosition.x + Random.Range(-5f, 5f),
                    roomPosition.y,
                    roomPosition.z + Random.Range(-5f, 5f)
                );

                int randomMonsterIndex = Random.Range(0, monsterPrefabs.Length);
                Instantiate(monsterPrefabs[randomMonsterIndex], monsterPosition, Quaternion.identity);
            }
        }
    }


    Door GetRequiredDoors(Vector2Int pos)
    {
        Door doors = Door.None;

        for (int i = 0; i < 4; i++)
        {
            Vector2Int neighborPos = pos + directions[i];
            if (generatedRooms.TryGetValue(neighborPos, out Door neighborDoors))
            {
                if ((neighborDoors & GetOppositeDoor(doorFlags[i])) != 0)
                {
                    doors |= doorFlags[i];
                }
            }
        }

        if (doors == Door.None)
        {
            int rand = Random.Range(1, 16); // 0001 à 1111
            doors = (Door)rand;
        }

        return doors;
    }

    Door GetOppositeDoor(Door dir)
    {
        return dir switch
        {
            Door.Top => Door.Bottom,
            Door.Bottom => Door.Top,
            Door.Left => Door.Right,
            Door.Right => Door.Left,
            _ => Door.None
        };
    }

    void GenerateRoomAt(Vector2Int gridPos, Door desiredDoors)
    {
        Door rotatedDoors;
        Quaternion rotation = GetRotationForDoors(desiredDoors, out rotatedDoors);

        GameObject prefab = null;

        if (Random.value < chance_room_souvenir && room_Souvenir != null)
        {
            prefab = room_Souvenir;
            rotation = Quaternion.identity;
        }
        else
        {
            if (!roomPrefabs.TryGetValue(rotatedDoors, out prefab))
            {
                prefab = room_HGBD;
                rotation = Quaternion.identity;
            }
        }

        Vector3 worldPos = new Vector3(gridPos.x * roomSize, 0, gridPos.y * roomSize);
        GameObject roomInstance = Instantiate(prefab, worldPos, rotation, this.transform);

        UpdateNavMesh();
        SpawnMonstersInRoom(worldPos);

        UpdateNavMesh();

        generatedRooms[gridPos] = desiredDoors;
        roomInstances[gridPos] = roomInstance;
    }

    Quaternion GetRotationForDoors(Door doors, out Door rotatedDoors)
    {
        for (int angle = 0; angle < 360; angle += 90)
        {
            Door rotated = RotateDoors(doors, angle);
            if (roomPrefabs.ContainsKey(rotated))
            {
                rotatedDoors = rotated;
                return Quaternion.Euler(0, angle, 0);
            }
        }

        rotatedDoors = Door.Top | Door.Bottom | Door.Left | Door.Right;
        return Quaternion.identity;
    }

    Door RotateDoors(Door doors, int angle)
    {
        Door result = Door.None;

        for (int i = 0; i < 4; i++)
        {
            if ((doors & doorFlags[i]) != 0)
            {
                int rotatedIndex = (i + angle / 90) % 4;
                result |= doorFlags[rotatedIndex];
            }
        }

        return result;
    }

    void UnloadFarRooms(Vector2Int center)
    {
        foreach (var kvp in roomInstances)
        {
            Vector2Int pos = kvp.Key;
            GameObject room = kvp.Value;

            bool shouldBeActive =
                Mathf.Abs(pos.x - center.x) <= viewDistance &&
                Mathf.Abs(pos.y - center.y) <= viewDistance;

            if (room.activeSelf != shouldBeActive)
            {
                room.SetActive(shouldBeActive);
            }
        }
    }
}

//using System.Collections.Generic;
//using UnityEngine;
//using Unity.AI.Navigation;

//public class LevelsGenerator : MonoBehaviour
//{
//    [Header("Références")]
//    public Transform player;
//    public int roomSize = 16;
//    public int viewDistance = 1;
//    public float chance_room_souvenir = 0.1f;
//    public NavMeshSurface navMeshSurface;

//    [Header("Prefabs multi-portes")]
//    public GameObject room_HB;
//    public GameObject room_GD;
//    public GameObject room_GHD;
//    public GameObject room_GBD;
//    public GameObject room_HGBD;
//    public GameObject room_Souvenir;

//    [Header("Prefabs à une seule porte")]
//    public GameObject room_Top;
//    public GameObject room_Bottom;
//    public GameObject room_Left;
//    public GameObject room_Right;

//    private Dictionary<Door, GameObject> roomPrefabs;
//    private Dictionary<Vector2Int, Door> generatedRooms = new();
//    private Dictionary<Vector2Int, GameObject> roomInstances = new();

//    [Header("Monstres")]
//    public GameObject[] monsterPrefabs;
//    public float monsterSpawnProbability = 0.3f;

//    private Vector2Int[] directions = {
//        new Vector2Int(0, 1),   // top (Z+)
//        new Vector2Int(0, -1),  // bottom (Z-)
//        new Vector2Int(-1, 0),  // left (X-)
//        new Vector2Int(1, 0)    // right (X+)
//    };

//    private Door[] doorFlags = {
//        Door.Top,
//        Door.Bottom,
//        Door.Left,
//        Door.Right
//    };

//    void Start()
//    {
//        roomPrefabs = new Dictionary<Door, GameObject>
//        {
//            { Door.Top, room_Top },
//            { Door.Bottom, room_Bottom },
//            { Door.Left, room_Left },
//            { Door.Right, room_Right },
//            { Door.Top | Door.Bottom, room_HB },
//            { Door.Left | Door.Right, room_GD },
//            { Door.Left | Door.Top | Door.Right, room_GHD },
//            { Door.Left | Door.Bottom | Door.Right, room_GBD },
//            { Door.Top | Door.Bottom | Door.Left | Door.Right, room_HGBD }
//        };

//        Vector2Int start = Vector2Int.zero;
//        GenerateRoomAt(start, Door.Top | Door.Bottom | Door.Left | Door.Right); // Start room
//    }

//    void Update()
//    {
//        Vector2Int playerPos = new Vector2Int(
//            Mathf.RoundToInt(player.position.x / roomSize),
//            Mathf.RoundToInt(player.position.z / roomSize)
//        );

//        GenerateAround(playerPos);
//        UnloadFarRooms(playerPos);
//    }

//    void UpdateNavMesh()
//    {
//        if (navMeshSurface != null)
//        {
//            navMeshSurface.BuildNavMesh();
//        }
//    }

//    void GenerateAround(Vector2Int center)
//    {
//        for (int dx = -viewDistance; dx <= viewDistance; dx++)
//        {
//            for (int dz = -viewDistance; dz <= viewDistance; dz++)
//            {
//                Vector2Int pos = center + new Vector2Int(dx, dz);
//                if (!generatedRooms.ContainsKey(pos))
//                {
//                    Door requiredDoors = GetRequiredDoors(pos);
//                    GenerateRoomAt(pos, requiredDoors);
//                }
//            }
//        }
//    }

//    void SpawnMonstersInRoom(Vector3 roomPosition)
//    {
//        if (Random.value < monsterSpawnProbability)
//        {
//            int monsterCount = Random.Range(1, 3);
//            for (int i = 0; i < monsterCount; i++)
//            {
//                Vector3 monsterPosition = new Vector3(
//                    roomPosition.x + Random.Range(-5f, 5f),
//                    roomPosition.y,
//                    roomPosition.z + Random.Range(-5f, 5f)
//                );

//                int randomMonsterIndex = Random.Range(0, monsterPrefabs.Length);
//                Instantiate(monsterPrefabs[randomMonsterIndex], monsterPosition, Quaternion.identity);
//            }
//        }
//    }

//    Door GetRequiredDoors(Vector2Int pos)
//    {
//        Door doors = Door.None;
//        for (int i = 0; i < 4; i++)
//        {
//            Vector2Int neighborPos = pos + directions[i];
//            if (generatedRooms.TryGetValue(neighborPos, out Door neighborDoors))
//            {
//                if ((neighborDoors & GetOppositeDoor(doorFlags[i])) != 0)
//                {
//                    doors |= doorFlags[i];
//                }
//            }
//        }

//        if (doors == Door.None)
//        {
//            int rand = Random.Range(1, 16); // 0001 à 1111
//            doors = (Door)rand;
//        }

//        return doors;
//    }

//    Door GetOppositeDoor(Door dir)
//    {
//        return dir switch
//        {
//            Door.Top => Door.Bottom,
//            Door.Bottom => Door.Top,
//            Door.Left => Door.Right,
//            Door.Right => Door.Left,
//            _ => Door.None
//        };
//    }

//    void GenerateRoomAt(Vector2Int gridPos, Door desiredDoors)
//    {
//        GameObject prefab = null;
//        Quaternion rotation = Quaternion.identity;
//        Door rotatedDoors;

//        if (Random.value < chance_room_souvenir && room_Souvenir != null)
//        {
//            prefab = room_Souvenir;
//        }
//        else
//        {
//            (prefab, rotation, rotatedDoors) = GetBestMatchingRoom(desiredDoors);
//        }

//        Vector3 worldPos = new Vector3(gridPos.x * roomSize, 0, gridPos.y * roomSize);
//        GameObject roomInstance = Instantiate(prefab, worldPos, rotation, this.transform);

//        UpdateNavMesh();
//        SpawnMonstersInRoom(worldPos);

//        generatedRooms[gridPos] = desiredDoors;
//        roomInstances[gridPos] = roomInstance;
//    }

//    (GameObject, Quaternion, Door) GetBestMatchingRoom(Door desired)
//    {
//        foreach (var kvp in roomPrefabs)
//        {
//            Door original = kvp.Key;
//            GameObject prefab = kvp.Value;

//            for (int angle = 0; angle < 360; angle += 90)
//            {
//                Door rotated = RotateDoors(original, angle);
//                if (rotated == desired)
//                {
//                    return (prefab, Quaternion.Euler(0, angle, 0), rotated);
//                }
//            }
//        }

//        // fallback
//        return (room_HGBD, Quaternion.identity, Door.Top | Door.Bottom | Door.Left | Door.Right);
//    }

//    Door RotateDoors(Door doors, int angle)
//    {
//        Door result = Door.None;

//        for (int i = 0; i < 4; i++)
//        {
//            if ((doors & doorFlags[i]) != 0)
//            {
//                int rotatedIndex = (i + angle / 90) % 4;
//                result |= doorFlags[rotatedIndex];
//            }
//        }

//        return result;
//    }

//    void UnloadFarRooms(Vector2Int center)
//    {
//        foreach (var kvp in roomInstances)
//        {
//            Vector2Int pos = kvp.Key;
//            GameObject room = kvp.Value;

//            bool shouldBeActive =
//                Mathf.Abs(pos.x - center.x) <= viewDistance &&
//                Mathf.Abs(pos.y - center.y) <= viewDistance;

//            if (room.activeSelf != shouldBeActive)
//            {
//                room.SetActive(shouldBeActive);
//            }
//        }
//    }
//}
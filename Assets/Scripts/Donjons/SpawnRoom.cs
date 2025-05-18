//using UnityEngine;

//public class SpawnRoom : MonoBehaviour
//{
//    public LayerMask whatIsRoom;
//    public LevelsGenerator levelGen;

//    private void Update()
//    {
//        Collider2D roomDetect = Physics2D.OverlapCircle(transform.position, 1, whatIsRoom);
        
//        if (roomDetect == null && levelGen.stopGeneration)
//        {
//            int rand = Random.Range(0, levelGen.rooms.Length);
//            Instantiate(levelGen.rooms[rand], transform.position, Quaternion.identity);
//        }
//    }
//}
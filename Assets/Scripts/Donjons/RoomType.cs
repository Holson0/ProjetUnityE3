//using UnityEngine;

//public class RoomType : MonoBehaviour
//{
//    public int type;

//    public void RoomDestruction()
//    {
//        Destroy(gameObject);
//    }
//}
using System;

[System.Flags]
public enum Door
{
    None = 0,
    Top = 1,
    Bottom = 2,
    Left = 4,
    Right = 8
}

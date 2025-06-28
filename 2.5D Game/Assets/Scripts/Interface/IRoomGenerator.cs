using UnityEngine;

public interface IRoomGenerator
{
    RoomData GenerateRoom(Vector2Int position, Vector2Int size, RoomType roomType);
}

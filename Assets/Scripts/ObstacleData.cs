using static Assets.Scripts.MyTileData;

namespace Assets.Scripts
{
    public class ObstacleData
    {
        public ObstacleType obstacleType { get; private set; }
        public int maxHealth { get; private set; }
        public enum ObstacleType
        {
            Tree,
            Mountain,
            House,
        }
        public ObstacleData(ObstacleType obstacleType, TileType tileType)
        {
            switch (obstacleType)
            {
                case ObstacleType.Tree:
                    maxHealth = (int)Health.Light;
                    break;
                case ObstacleType.Mountain:
                    maxHealth = (int)Health.Heavy;
                    break;
                case ObstacleType.House:
                    maxHealth = (int)Health.Normal;
                    break;
                default:
                    break;
            }
            if (tileType == TileType.Sand)
            {
                maxHealth *= 2;
            }
            else if (tileType == TileType.Stone)
            {
                maxHealth *= 3;
            }
        }
    }
}

namespace Assets.Scripts
{
    public class MyTileData
    {
        public int MoveCost { get; private set; }
        public TileType tileType { get; private set; }

        public enum TileType
        {
            Plains,
            Mud,
            Sand,
            Stone,
            Ice,
            Empty,
        }

        public enum MoveCostType
        {
            Slide = 0,
            Passable = 2,
            Slower = 3,
            Stop = 9999,
        }

        public enum Health
        {
            None = 0,
            Light = 8,
            Normal = 16,
            Heavy = 64,
        }

        public MyTileData(TileType tileType)
        {
            this.tileType = tileType;

            switch (tileType)
            {
                case TileType.Plains:
                    MoveCost = (int)MoveCostType.Passable;
                    break;
                case TileType.Mud:
                    MoveCost = (int)MoveCostType.Slower;
                    break;
                case TileType.Sand:
                    MoveCost = (int)MoveCostType.Passable;
                    break;
                case TileType.Stone:
                    MoveCost = (int)MoveCostType.Passable;
                    break;
                case TileType.Ice:
                    MoveCost = (int)MoveCostType.Slide;
                    break;
                case TileType.Empty:
                    MoveCost = (int)MoveCostType.Stop;
                    break;
            }
        }
    }
}
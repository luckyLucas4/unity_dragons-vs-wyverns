using System.Collections.Generic;

namespace Assets.Scripts
{
    public class DragonData
    {
        public string Name { get; private set; }
        public int MaxHealth { get; private set; }
        public int AttackDamage { get; private set; }
        public int BreathDamage { get; private set; } = 0;
        public int Speed { get; private set; }
        public int SpecialCooldown { get; private set; }
        public int BreathRange { get; private set; } = 0;
        public DragonSize Size { get; private set; }
        public DragonType Type { get; private set; }

        public enum DragonType
        {
            Plain, Fire, Ice,
        }

        public enum DragonSize
        {
            Tiny, Small, Average, Huge,
        }

        public enum SpeedScale
        {
            None, Slow, Normal, Fast,
        }

        public enum DamageScale
        {
            None, Light, Normal, Heavy,
        }
        public enum HealthScale
        {
            None, Light, Normal, Heavy,
        }

        static readonly Dictionary<DragonSize, int> sizeDict = new()
        {
            [DragonSize.Tiny] = 4,
            [DragonSize.Small] = 6,
            [DragonSize.Average] = 10,
            [DragonSize.Huge] = 14,
        };
        static readonly Dictionary<SpeedScale, int> speedDict = new()
        {
            [SpeedScale.None] = 0,
            [SpeedScale.Slow] = 4,
            [SpeedScale.Normal] = 6,
            [SpeedScale.Fast] = 8,
        };
        static readonly Dictionary<DamageScale, int> damageDict = new()
        {
            [DamageScale.None] = 0,
            [DamageScale.Light] = 4,
            [DamageScale.Normal] = 8,
            [DamageScale.Heavy] = 12,
        };
        static readonly Dictionary<HealthScale, int> healthDict = new()
        {
            [HealthScale.None] = 0,
            [HealthScale.Light] = 30,
            [HealthScale.Normal] = 50,
            [HealthScale.Heavy] = 70,
        };

        public DragonData(string name, DragonType dragonType, DragonSize dragonSize)
        {
            Name = name;
            Type = dragonType;
            Size = dragonSize;
            switch (dragonType)
            {
                case DragonType.Plain:
                    SetData(SpeedScale.Normal, DamageScale.Heavy, HealthScale.Normal);
                    SpecialCooldown = 2;
                    break;
                case DragonType.Fire:
                    SetData(SpeedScale.Slow, DamageScale.Normal, HealthScale.Heavy);
                    SpecialCooldown = 4;
                    BreathRange = 3;
                    BreathDamage = DamageValue(DamageScale.Heavy) * SizeValue(dragonSize) * 2;
                    break;
                case DragonType.Ice:
                    SetData(SpeedScale.Fast, DamageScale.Normal, HealthScale.Normal);
                    SpecialCooldown = 1;
                    break;
            }
        }

        private void SetData(
            SpeedScale speedScale, 
            DamageScale damageScale, 
            HealthScale healthScale
            )
        {
            Speed = SpeedValue(speedScale);
            AttackDamage = DamageValue(damageScale) * SizeValue(Size);
            MaxHealth = HealthValue(healthScale) * SizeValue(Size);
        }

        public static int SizeValue (DragonSize dragonSize)
            => sizeDict[dragonSize];

        public static int SpeedValue(SpeedScale speedScale)
            => speedDict[speedScale];

        public static int DamageValue(DamageScale damageScale)
            => damageDict[damageScale];

        public static int HealthValue (HealthScale healthScale) 
            => healthDict[healthScale];
    }
}
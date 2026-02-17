using UnityEngine;

public enum HitVFXType
{
    None = 0,
    Spark = 1,
    BlockHit = 2,
    FleshHit = 3,
    MetalHit = 4
}

public interface IHitVFXGetter
{
    HitVFXType HitVFXType { get; }
}


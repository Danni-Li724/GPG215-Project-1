using UnityEngine;

public enum HitVFXType
{
    None = 0,
    Default = 1,
    Googey = 2,
    Bloody = 3,
    Metalic = 4
}

public interface IHitVFXGetter
{
    HitVFXType HitVFXType { get; }
}


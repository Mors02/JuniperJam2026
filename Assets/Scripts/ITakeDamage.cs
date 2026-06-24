using System;
using UnityEngine;

public enum DamageType
{
    None,
    Normal,
    Stasis,
    Knockback
}

public readonly struct DamageInfo
{
    public readonly int damage;
    public readonly DamageType damageType;
    public readonly float duration;
    public readonly Vector2 force;

    public readonly bool preserveVelocity;

    public DamageInfo(int damage = 0, DamageType damageType = DamageType.None, float duration = 0, Vector2? force = null, bool preserveVelocity = true)
    {
        this.damage = damage;
        this.damageType = damageType;
        this.duration = duration;
        this.force = force ?? Vector2.zero;
        this.preserveVelocity = preserveVelocity;
    }
}

public interface ITakeDamage
{
    public void TakeDamage(DamageInfo damageInfo);
}
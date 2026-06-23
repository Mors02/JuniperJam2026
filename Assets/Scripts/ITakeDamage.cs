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

    public DamageInfo(int damage = 0, DamageType damageType = DamageType.None, float duration = 0, Vector2? force = null)
    {
        this.damage = damage;
        this.damageType = damageType;
        this.duration = duration;
        this.force = force ?? Vector2.zero;
    }
}

public interface ITakeDamage
{
    public void TakeDamage(DamageInfo damageInfo);
}
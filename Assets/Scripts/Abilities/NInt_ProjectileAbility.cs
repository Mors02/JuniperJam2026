using UnityEngine;

[System.Serializable]
public class NInt_ProjectileAbility : Ability
{
    public GameObject projectilePrefab;
    
    private Transform transform;

    public override void Initialize(GameObject go = null)
    {
        base.Initialize(go);

        if (gameObject) transform = gameObject.transform;
    }

    public override bool CanTrigger()
    {
        return base.CanTrigger() && projectilePrefab;
    }

    public override void Trigger()
    {
        base.Trigger();

        var projectileObject = UnityEngine.Object.Instantiate(projectilePrefab, transform.position, transform.rotation);

        if (projectileObject.TryGetComponent<Projectile>(out var projectile))
        {
            projectile.SetOwner(gameObject);
        }
    }
}

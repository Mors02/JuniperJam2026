using System.Collections.Generic;
using UnityEngine;

public class DeathExplosionEffect : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    
    public void PlayEffect()
    {
        foreach (ParticleSystem effect in particleSystems)
        {
            effect.Play();
        }
    }
}

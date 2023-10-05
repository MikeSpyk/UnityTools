using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools.Effects
{
    [RequireComponent(typeof(ParticleSystem))]
    public class Effect : MonoBehaviour
    {
        private ParticleSystem m_particleSystem;
        private List<ParticleSystem> m_subParticleSystems = new List<ParticleSystem>();

        public ParticleSystem effectParticleSystem { get { return m_particleSystem; } }
        public List<ParticleSystem> subParticleSystems { get { return m_subParticleSystems; } }

        private void Awake()
        {
            m_particleSystem = GetComponent<ParticleSystem>();

            for (int i = 0; i < transform.childCount; i++)
            {
                ParticleSystem particleSystem = transform.GetChild(i).GetComponent<ParticleSystem>();

                if (particleSystem != null)
                {
                    m_subParticleSystems.Add(particleSystem);
                }
            }
        }
    }
}
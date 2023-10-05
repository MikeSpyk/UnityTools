using System;
using System.Collections.Generic;
using UnityEngine;
using BasicTools.GameObjects;

namespace BasicTools.Effects
{
    public class EffectsManager : MonoBehaviour
    {
        public static EffectsManager Singleton { get { return singleton; } }
        private static EffectsManager singleton;

        [SerializeField] private GameObject[] m_effectPrefabs;

        private GameObjectCache<Effect>[] m_effectsCache;
        private List<Effect>[] m_activeEffects;
        private List<Tuple<float, Effect>>[] m_effectsFading;

        private void Awake()
        {
            singleton = this;
            m_effectsCache = new GameObjectCache<Effect>[m_effectPrefabs.Length];
            m_activeEffects = new List<Effect>[m_effectPrefabs.Length];
            m_effectsFading = new List<Tuple<float, Effect>>[m_effectPrefabs.Length];

            for (int i = 0; i < m_effectsCache.Length; i++)
            {
                m_effectsCache[i] = new GameObjectCache<Effect>(m_effectPrefabs[i]);
                m_activeEffects[i] = new List<Effect>();
                m_effectsFading[i] = new List<Tuple<float, Effect>>();
            }
        }

        private void Update()
        {
            for (int i = 0; i < m_activeEffects.Length; i++)
            {
                for (int j = 0; j < m_activeEffects[i].Count; j++)
                {
                    if (!m_activeEffects[i][j].effectParticleSystem.isPlaying)
                    {
                        recyleEffect(m_activeEffects[i][j], i);
                        m_activeEffects[i].RemoveAt(j);
                        j--;
                    }
                }

                for (int j = 0; j < m_effectsFading[i].Count; j++)
                {
                    if (m_effectsFading[i][j].Item1 < Time.time)
                    {
                        m_effectsCache[i].recycle(m_effectsFading[i][j].Item2);
                        m_effectsFading[i].RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        public void recyleExternalEffect(Effect effect, int prefabIndex, float fadeOutTime = 0f)
        {
            int listIndex = m_activeEffects[prefabIndex].IndexOf(effect);

            if (listIndex > -1)
            {
                m_activeEffects[prefabIndex].RemoveAt(listIndex);
            }

            if (fadeOutTime <= 0)
            {
                recyleEffect(effect, prefabIndex);
            }
            else
            {
                effect.effectParticleSystem.Stop();
                m_effectsFading[prefabIndex].Add(new Tuple<float, Effect>(Time.time + fadeOutTime, effect));
            }
        }

        private void recyleEffect(Effect effect, int prefabIndex)
        {
            effect.effectParticleSystem.Stop();
            m_effectsCache[prefabIndex].recycle(effect);
        }

        public Effect playEffect(int prefabIndex, Vector3 position)
        {
            Effect effect = m_effectsCache[prefabIndex].get();
            effect.gameObject.transform.position = position;
            effect.effectParticleSystem.Play();
            m_activeEffects[prefabIndex].Add(effect);

            return effect;
        }
    }
}
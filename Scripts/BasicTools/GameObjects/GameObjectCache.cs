using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools.GameObjects
{
    public class GameObjectCache<T> where T : UnityEngine.Component
    {
        private GameObject m_prefab;
        private Queue<T> m_cache = new Queue<T>();

        public GameObjectCache(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new System.NotSupportedException("prefab is null");
            }

            m_prefab = prefab;
        }

        public T get()
        {
            T result;

            if (m_cache.Count > 0)
            {
                result = m_cache.Dequeue();
                result.gameObject.SetActive(true);
            }
            else
            {
                GameObject newInstance = GameObject.Instantiate(m_prefab);
                result = newInstance.GetComponent<T>();

                if (result == null)
                {
                    throw new System.NotSupportedException("the provided prefab-gameobject does not have the provided component(T:" + typeof(T).Name + ") attached. Make sure the prefab has a component of type T");
                }
            }

            return result;
        }

        public void recycle(T instance)
        {
            if (instance == null)
            {
                throw new System.ArgumentNullException();
            }
            else
            {
                instance.gameObject.SetActive(false);
                instance.transform.position = Vector3.zero;
                m_cache.Enqueue(instance);
            }
        }
    }
}
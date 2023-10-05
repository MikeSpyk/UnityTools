using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BasicTools
{
    namespace GameObjects
    {
        public class ObjectPlacer : MonoBehaviour
        {
            private enum MaterialState { Good, Bad }

            [SerializeField] private Material m_bluprintMaterialGood;
            [SerializeField] private Material m_bluprintMaterialBad;
            [SerializeField] private GameObject[] m_objectsPrefabs;
            [SerializeField] private Vector3[] m_prefabsOffset;
            /// <summary>
            /// Multiple can be placed in a row. Otherwise placing will stopped after one instance
            /// </summary>
            [SerializeField] public bool m_placeMultiple = false;
            [SerializeField] private float m_holdInputPlacingTime = 1f;

            public event EventHandler<int> placeModeStopped;
            public event EventHandler<int> placeModeStarted;
            /// <summary>
            /// gets fired if placing with holding a key is in progress. provides normalized progress value [0-1]
            /// </summary>
            public event EventHandler<float> holdInputPlacingProgress;
            public GameObject[] objectsPrefabs { get { return m_objectsPrefabs; } }
            public bool placingMode { get { return m_placingMode; } }
            public int currentPrefabIndex { get { return m_currentPrefabIndex; } }

            private MaterialState[] m_materialsBluprints = null;
            private GameObject[] m_objectsBluprints = null;
            private float[] m_objectsCircleSize = null;
            private GameObject m_currentBluprint = null;
            private GameObject m_currentRangeCircle = null;
            private Vector3 m_currentBluprintOffset = Vector3.zero;
            private int m_currentPrefabIndex = -1;
            private bool m_placingMode = false;
            private int m_rayCastLayerMask;
            private System.Func<GameObject, int, bool> m_currentBluprintPlaceCondition = null;
            private System.Func<GameObject, int, bool> m_currentBluprintPlacePreviewCondition = null;
            private System.Action<GameObject> m_currentBluprintBuiltCallback = null;
            private System.Func<Vector3, Vector3> m_currentBluprintSnapFunction = null;
            private int m_stopPlacingModeHash;
            private Vector2? m_lastTouchPosition = null;
            private float m_holdInputPlacingStartTime = 0;
            private Vector3 m_holdInputPlacingStartPosition = Vector3.zero;
            private int m_holdInputPlacingLastFrame = 0;

            private void Awake()
            {
                cacheBluprints();
                m_rayCastLayerMask = (int.MaxValue - (int)Mathf.Pow(2, 10)); // everything except layer 10 (triggers)
                m_stopPlacingModeHash = "m_stopPlacingModeHash".GetHashCode();
            }

            private void Update()
            {
                if (m_placingMode)
                {
                    if (Input.touchCount > 0)
                    {
                        m_lastTouchPosition = Input.GetTouch(0).position;
                    }

                    Vector3 screenCursorPosition = Input.mousePosition; // this is last touch position on mobile if no touch active and mouse position if mouse is available

#if UNITY_ANDROID && UNITY_EDITOR // editor has both: mouse and touch. in this case use touch and ingnore mouse
                    if (m_lastTouchPosition != null)
                    {
                        screenCursorPosition = new Vector3(m_lastTouchPosition.Value.x, m_lastTouchPosition.Value.y, 0);
                    }
#endif
                    RaycastHit hit = new RaycastHit();
                    Ray ray = Camera.main.ScreenPointToRay(screenCursorPosition);

                    if (Physics.Raycast(ray, out hit, float.MaxValue, m_rayCastLayerMask))
                    {
                        //Debug.Log("hit: "+hit.collider.gameObject.name);
                        //Debug.DrawRay(hit.point + m_currentBluprintOffset, Vector3.up);
                        m_currentBluprint.transform.position = hit.point + Quaternion.LookRotation(m_currentBluprint.transform.forward) * m_currentBluprintOffset;
                        m_currentBluprint.transform.position = m_currentBluprintSnapFunction(m_currentBluprint.transform.position);

                        if (m_currentRangeCircle != null)
                        {
                            m_currentRangeCircle.transform.position = m_currentBluprint.transform.position + Vector3.up * 0.001f;
                        }

                        if (Input.GetKey(KeyCode.R))
                        {
                            m_currentBluprint.transform.Rotate(0f, 1f, 0f);
                        }

                        if (m_currentBluprintPlacePreviewCondition(m_currentBluprint, m_currentPrefabIndex))
                        {
                            if (m_materialsBluprints[m_currentPrefabIndex] != MaterialState.Good)
                            {
                                setMaterialChildren(m_currentBluprint.transform, m_bluprintMaterialGood);
                                m_materialsBluprints[m_currentPrefabIndex] = MaterialState.Good;
                            }
                        }
                        else
                        {
                            if (m_materialsBluprints[m_currentPrefabIndex] != MaterialState.Bad)
                            {
                                setMaterialChildren(m_currentBluprint.transform, m_bluprintMaterialBad);
                                m_materialsBluprints[m_currentPrefabIndex] = MaterialState.Bad;
                            }
                        }

                        if (Input.touchCount > 0)
                        {
                            holdInputPlacing();
                        }
                        else if (Input.GetKeyDown(KeyCode.Mouse0) && m_currentBluprintPlaceCondition(m_currentBluprint, m_currentPrefabIndex))
                        {
                            GameObject child = Instantiate(m_objectsPrefabs[m_currentPrefabIndex], m_currentBluprint.transform.position, m_currentBluprint.transform.rotation);
                            m_currentBluprintBuiltCallback(child);

                            if (!m_placeMultiple)
                            {
                                stopPlacingMode();
                            }
                        }
                    }
                }
            }

            private void holdInputPlacing()
            {
                if (Time.frameCount > m_holdInputPlacingLastFrame + 1 || m_holdInputPlacingStartPosition != m_currentBluprint.transform.position) // holding interrupted
                {
                    startHoldInputPlacing();
                }
                else
                {
                    if (Time.time > m_holdInputPlacingStartTime + m_holdInputPlacingTime)
                    {
                        if (m_currentBluprintPlaceCondition(m_currentBluprint, m_currentPrefabIndex))
                        {
                            GameObject child = Instantiate(m_objectsPrefabs[m_currentPrefabIndex], m_currentBluprint.transform.position, m_currentBluprint.transform.rotation);
                            m_currentBluprintBuiltCallback(child);

                            if (!m_placeMultiple)
                            {
                                stopPlacingMode();
                            }
                        }

                        m_holdInputPlacingStartTime = Time.time;
                    }
                    else
                    {
                        fireHoldInputPlacingProgress((Time.time - m_holdInputPlacingStartTime) / m_holdInputPlacingTime);
                    }
                }

                m_holdInputPlacingLastFrame = Time.frameCount;
            }

            private void startHoldInputPlacing()
            {
                m_holdInputPlacingStartTime = Time.time;
                m_holdInputPlacingStartPosition = m_currentBluprint.transform.position;
            }

            private void firePlaceModeStopped(int prefabIndex)
            {
                placeModeStopped?.Invoke(this, prefabIndex);
            }

            private void firePlaceModeStarted(int prefabIndex)
            {
                placeModeStarted?.Invoke(this, prefabIndex);
            }

            private void fireHoldInputPlacingProgress(float progress)
            {
                holdInputPlacingProgress?.Invoke(this, progress);
            }

            private void cacheBluprints()
            {
                m_objectsBluprints = new GameObject[m_objectsPrefabs.Length];
                m_materialsBluprints = new MaterialState[m_objectsPrefabs.Length];
                m_objectsCircleSize = new float[m_objectsPrefabs.Length];

                for (int i = 0; i < m_objectsPrefabs.Length; i++)
                {
                    Tower tower = m_objectsPrefabs[i].GetComponent<Tower>();
                    if (tower != null)
                    {
                        m_objectsCircleSize[i] = tower.attackRadius / 10;
                    }

                    m_objectsBluprints[i] = Instantiate(m_objectsPrefabs[i]);
                    m_objectsBluprints[i].transform.SetParent(this.transform);
                    setMaterialChildren(m_objectsBluprints[i].transform, m_bluprintMaterialGood);
                    removeComponentChildren<MonoBehaviour>(m_objectsBluprints[i].transform);
                    removeComponentChildren<Collider>(m_objectsBluprints[i].transform);
                    m_objectsBluprints[i].SetActive(false);
                }
            }

            private void setMaterialChildren(Transform source, Material material)
            {
                Renderer renderer = source.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material[] materials = renderer.materials;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = material;
                    }

                    renderer.materials = materials;
                }

                for (int i = 0; i < source.childCount; i++)
                {
                    setMaterialChildren(source.GetChild(i), material);
                }
            }

            private static void removeComponentChildren<T>(Transform source)
            {
                T[] components = source.GetComponents<T>();
                if (components != null)
                {
                    for (int i = 0; i < components.Length; i++)
                    {
                        Destroy(components[i] as UnityEngine.Object);
                    }
                }

                for (int i = 0; i < source.childCount; i++)
                {
                    removeComponentChildren<T>(source.GetChild(i));
                }
            }

            public void stopPlacingMode()
            {
                if (m_placingMode)
                {
                    m_placingMode = false;

                    if (m_currentBluprint != null)
                    {
                        m_currentBluprint.SetActive(false);
                    }

                    if (m_currentRangeCircle != null)
                    {
                        WorldUiManager.singleton.destroyCircle(m_currentRangeCircle);
                    }

                    firePlaceModeStopped(m_currentPrefabIndex);
                    TowerDefenceUIManager.Singleton.removeEscapeableFunction(m_stopPlacingModeHash);
                }
            }

            public void startPlacingMode(int prefabIndex, System.Func<GameObject, int, bool> placeCondition, System.Func<GameObject, int, bool> placePreviewCondition, System.Action<GameObject> builtCallback, System.Func<Vector3, Vector3> snapFunction)
            {
                m_currentBluprintPlaceCondition = placeCondition;
                m_currentBluprintPlacePreviewCondition = placePreviewCondition;
                m_currentBluprintBuiltCallback = builtCallback;
                m_currentBluprintSnapFunction = snapFunction;

                m_currentPrefabIndex = prefabIndex;
                m_currentBluprint = m_objectsBluprints[prefabIndex];
                m_currentBluprint.SetActive(true);
                m_currentBluprintOffset = m_prefabsOffset[prefabIndex];
                m_placingMode = true;

                TowerDefenceUIManager.Singleton.addEscapeableFunction(stopPlacingMode, m_stopPlacingModeHash);

                m_currentRangeCircle = WorldUiManager.singleton.showCircle(Vector3.zero, m_objectsCircleSize[m_currentPrefabIndex]);

                firePlaceModeStarted(m_currentPrefabIndex);
            }

            public GameObject spawnInstantly(Vector3 position, int prefabIndex)
            {
                return Instantiate(m_objectsPrefabs[prefabIndex], position, m_objectsPrefabs[prefabIndex].transform.rotation);
            }
        }
    }
}
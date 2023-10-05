using System.Collections.Generic;
using UnityEngine;
using System;

namespace BasicTools.Effects
{
    /// <summary>
    /// creats a visable line on the ground that followes a provided path of vector2s. needs the shader "MovingTexture"
    /// </summary>
    public class VisualPathManager : MonoBehaviour
    {
        private enum Direction { up, down, right, left }

        [SerializeField] private GameObject m_rectanglePathPrefab;
        [SerializeField] private GameObject m_trianglePathPrefab;
        [SerializeField] private float m_pathAnimationSpeed = 1f;
        [SerializeField] private Color m_pathColor;
        private float m_defaultRectanlgeScale;
        private List<Queue<MeshRenderer>> m_prefabCache = new List<Queue<MeshRenderer>>();

        private void Awake()
        {
            m_defaultRectanlgeScale = m_rectanglePathPrefab.transform.localScale.x;
        }

        public void recylePath(List<Tuple<int, MeshRenderer>> path)
        {
            for (int i = 0; i < path.Count; i++)
            {
                recyclePrefab(path[i].Item2, path[i].Item1);
            }
        }

        public List<Tuple<int, MeshRenderer>> createVisualPath(List<Vector3> corners)
        {
            if (corners == null || corners.Count < 2)
            {
                throw new System.ArgumentOutOfRangeException("there must be at least 2 corners");
            }

            List<Tuple<int, MeshRenderer>> allMeshes_prefabIndex_renderer = new List<Tuple<int, MeshRenderer>>();
            List<Vector3> simplifiedPath = mergeCornersInAStraightLine(corners);

            Vector3 directionVec;
            Direction direction1;
            Direction direction2;

            for (int i = 1; i < simplifiedPath.Count - 1; i++)
            {
                directionVec = simplifiedPath[i] - simplifiedPath[i - 1];
                direction1 = directionFromVector(directionVec);

                if (directionVec.magnitude > 1) // if 2 corners are next to each other, there is no need for a rectanlge
                {
                    MeshRenderer rectangle = getPrefabCache(0);
                    //Debug.DrawRay(simplifiedPath[i - 1], directionVec);
                    rectangle.transform.position = simplifiedPath[i - 1] + directionVec / 2;
                    setMaterialValues(rectangle.material, direction1);
                    scaleRectangle(rectangle.transform, direction1, directionVec.magnitude - 1);
                    allMeshes_prefabIndex_renderer.Add(new Tuple<int, MeshRenderer>(0, rectangle));
                }

                MeshRenderer cornerTriangle1 = getPrefabCache(1);
                cornerTriangle1.transform.position = simplifiedPath[i];
                cornerTriangle1.transform.rotation = Quaternion.LookRotation(directionVec) * Quaternion.Euler(270, 0, 0);
                setMaterialValues(cornerTriangle1.material, directionFromVector(directionVec));
                allMeshes_prefabIndex_renderer.Add(new Tuple<int, MeshRenderer>(1, cornerTriangle1));

                directionVec = simplifiedPath[i + 1] - simplifiedPath[i];
                //Debug.DrawRay(simplifiedPath[i], directionVec);
                direction2 = directionFromVector(directionVec);
                MeshRenderer cornerTriangle2 = getPrefabCache(1);
                cornerTriangle2.transform.position = simplifiedPath[i];
                cornerTriangle2.transform.rotation = Quaternion.LookRotation(directionVec) * Quaternion.Euler(270, 0, 0);
                setMaterialValues(cornerTriangle2.material, direction2);
                allMeshes_prefabIndex_renderer.Add(new Tuple<int, MeshRenderer>(1, cornerTriangle2));

                setRotationTrianglePair(cornerTriangle1, cornerTriangle2, direction1, direction2);
            }

            directionVec = simplifiedPath[simplifiedPath.Count - 1] - simplifiedPath[simplifiedPath.Count - 2];
            direction1 = directionFromVector(directionVec);
            MeshRenderer rectangle2 = getPrefabCache(0);
            rectangle2.transform.position = simplifiedPath[simplifiedPath.Count - 2] + (simplifiedPath[simplifiedPath.Count - 1] - simplifiedPath[simplifiedPath.Count - 2]) / 2;
            setMaterialValues(rectangle2.material, direction1);
            scaleRectangle(rectangle2.transform, directionFromVector(directionVec), directionVec.magnitude - 1);
            allMeshes_prefabIndex_renderer.Add(new Tuple<int, MeshRenderer>(0, rectangle2));

            /*
            for (int i = 0; i < simplifiedPath.Count; i++)
            {
                Debug.DrawRay(simplifiedPath[i], Vector3.up);
            }*/
            //Debug.Break();

            return allMeshes_prefabIndex_renderer;
        }

        private void setRotationTrianglePair(MeshRenderer triangle1, MeshRenderer triangle2, Direction direction1, Direction direction2)
        {
            if (direction1 == Direction.up)
            {
                if (direction2 == Direction.right)
                {
                    triangle1.transform.rotation = Quaternion.Euler(-90, 0, 180);
                    triangle2.transform.rotation = Quaternion.Euler(-90, 0, 0);
                }
                else // left
                {
                    triangle1.transform.rotation = Quaternion.Euler(-90, 0, 90);
                    triangle2.transform.rotation = Quaternion.Euler(-90, 0, -90);
                }
            }
            else if (direction1 == Direction.down)
            {
                if (direction2 == Direction.right)
                {
                    triangle1.transform.rotation = Quaternion.Euler(-90, 0, -90);
                    triangle2.transform.rotation = Quaternion.Euler(-90, 0, 90);
                }
                else // left
                {
                    triangle1.transform.rotation = Quaternion.Euler(-90, 0, 0);
                    triangle2.transform.rotation = Quaternion.Euler(-90, 0, 180);
                }
            }
            else if (direction1 == Direction.right)
            {
                if (direction2 == Direction.up)
                {
                    triangle1.transform.rotation = Quaternion.Euler(-90, 0, -180);
                    triangle2.transform.rotation = Quaternion.Euler(-90, 0, 0);
                }
                else // down
                {
                    triangle1.transform.rotation = Quaternion.Euler(-90, 0, -90);
                    triangle2.transform.rotation = Quaternion.Euler(-90, 0, 90);
                }
            }
            else if (direction1 == Direction.left)
            {
                if (direction2 == Direction.up)
                {
                    triangle1.transform.rotation = Quaternion.Euler(-90, 0, 90);
                    triangle2.transform.rotation = Quaternion.Euler(-90, 0, -90);
                }
                else // down
                {
                    triangle1.transform.rotation = Quaternion.Euler(-90, 0, 0);
                    triangle2.transform.rotation = Quaternion.Euler(-90, 0, -180);
                }
            }
        }

        private Direction directionFromVector(Vector3 directionVec)
        {
            if (directionVec.x > 0)
            {
                return Direction.right;
            }
            else if (directionVec.x < 0)
            {
                return Direction.left;
            }
            else if (directionVec.z < 0)
            {
                return Direction.down;
            }
            else
            {
                return Direction.up;
            }
        }

        private void scaleRectangle(Transform rectangle, Direction direction, float length)
        {
            if (direction == Direction.up || direction == Direction.down)
            {
                rectangle.localScale = new Vector3(m_defaultRectanlgeScale, length * 50, 1);
            }
            else // right/left
            {
                rectangle.localScale = new Vector3(length * 50, m_defaultRectanlgeScale, 1);
            }
        }

        private void setMaterialValues(Material material, Direction direction)
        {
            if (direction == Direction.up)
            {
                material.SetFloat("_Rotation", 0);
                material.SetFloat("_SpeedX", 0);
                material.SetFloat("_SpeedY", -m_pathAnimationSpeed);
            }
            else if (direction == Direction.down)
            {
                material.SetFloat("_Rotation", Mathf.PI);
                material.SetFloat("_SpeedX", 0);
                material.SetFloat("_SpeedY", m_pathAnimationSpeed);
            }
            else if (direction == Direction.left)
            {
                material.SetFloat("_Rotation", Mathf.PI / 2);
                material.SetFloat("_SpeedX", m_pathAnimationSpeed);
                material.SetFloat("_SpeedY", 0);
            }
            else //(direction == Direction.right)
            {
                material.SetFloat("_Rotation", Mathf.PI * 1.5f);
                material.SetFloat("_SpeedX", -m_pathAnimationSpeed);
                material.SetFloat("_SpeedY", 0);
            }

            material.SetColor("_Color", m_pathColor);
        }

        private List<Vector3> mergeCornersInAStraightLine(List<Vector3> corners)
        {
            if (corners == null || corners.Count < 2)
            {
                throw new System.ArgumentOutOfRangeException("there must be at least 2 corners");
            }

            List<Vector3> result = new List<Vector3>();
            result.Add(corners[0]);

            Vector2 originDirection = new Vector2(corners[1].x, corners[1].z) - new Vector2(corners[0].x, corners[0].z);
            Vector2 tempDirection;
            float tempAngle;

            for (int i = 1; i < corners.Count - 1; i++)
            {
                tempDirection = new Vector2(corners[i + 1].x, corners[i + 1].z) - new Vector2(corners[i].x, corners[i].z);
                tempAngle = Vector2.Angle(originDirection, tempDirection);

                if (tempAngle == 0) // same angle
                { }
                else if (tempAngle == 90) // real corner
                {
                    result.Add(corners[i]);
                    originDirection = tempDirection;
                }
                else
                {
                    Debug.LogWarning("Unexpected angle in path: " + tempAngle + ". Make sure the path only changes in a 90° Angle!");
                }
            }

            result.Add(corners[corners.Count - 1]);

            /*Debug.Log("path simplified form " + corners.Count + " to " + result.Count + " corners");
            for (int i = 0; i < result.Count; i++)
            {
                Debug.DrawRay(result[i], Vector3.up * 2);
            }
            Debug.Break();*/

            return result;
        }

        private MeshRenderer getPrefabCache(int index)
        {
            if (m_prefabCache.Count > index && m_prefabCache[index].Count > 0)
            {
                MeshRenderer returnValue = m_prefabCache[index].Dequeue();
                returnValue.gameObject.SetActive(true);

                return returnValue;
            }
            else
            {
                if (index == 0)
                {
                    return Instantiate(m_rectanglePathPrefab).GetComponent<MeshRenderer>();
                }
                else
                {
                    return Instantiate(m_trianglePathPrefab).GetComponent<MeshRenderer>();
                }
            }
        }

        private void recyclePrefab(MeshRenderer prefabMeshRenderer, int index)
        {
            if (m_prefabCache.Count <= index)
            {
                while (m_prefabCache.Count <= index)
                {
                    m_prefabCache.Add(new Queue<MeshRenderer>());
                }
            }

            m_prefabCache[index].Enqueue(prefabMeshRenderer);
            prefabMeshRenderer.gameObject.SetActive(false);
        }

        public static void hidePath(List<Tuple<int, MeshRenderer>> path)
        {
            for (int i = 0; i < path.Count; i++)
            {
                path[i].Item2.gameObject.SetActive(false);
            }
        }

        public static void unhidePath(List<Tuple<int, MeshRenderer>> path)
        {
            for (int i = 0; i < path.Count; i++)
            {
                path[i].Item2.gameObject.SetActive(true);
            }
        }
    }
}
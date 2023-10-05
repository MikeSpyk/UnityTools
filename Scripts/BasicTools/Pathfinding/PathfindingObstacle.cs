using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools.Pathfinding
{
    /// <summary>
    /// a space in world that is impassable. will get converted to raster size
    /// </summary>
    public class PathfindingObstacle : MonoBehaviour
    {
        [SerializeField] private Rasterizer m_target;
        [SerializeField] private Vector2Int m_size;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            Vector2 viewRect = m_size * 2;

            Gizmos.DrawWireCube(transform.position, new Vector3(viewRect.x, 0, viewRect.y));
        }

        private void Awake()
        {
            for (int i = 0; i < m_size.x * 2; i++)
            {
                for (int j = 0; j < m_size.y * 2; j++)
                {
                    Vector2Int gridPos = m_target.worldToGridPosition(transform.position + new Vector3(i, 0, j) - new Vector3(m_size.x, 0, m_size.y) + new Vector3(0.5f, 0, 0.5f));
                    m_target.setCellOccupationState(Rasterizer.CellOccupationState.Occupied, gridPos);
                }
            }
        }
    }
}
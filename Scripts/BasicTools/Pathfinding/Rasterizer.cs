using System.Collections.Generic;
using UnityEngine;

namespace BasicTools.Pathfinding
{
    /// <summary>
    /// creates a raster of cells
    /// </summary>
    public class Rasterizer : MonoBehaviour
    {
        public enum CellOccupationState { Free, Occupied }

        [SerializeField] private float m_cellSize;
        [SerializeField] private Renderer m_gridRenderer;
        [SerializeField] private Vector2Int m_size;
        [SerializeField] private int m_aStarNormalCost = 10;
        [SerializeField] private bool m_DEBUG_showAllGridPoints = false;

        private int[,] m_aStarCellCosts = null;
        private List<Vector2Int> m_occupiedCells = new List<Vector2Int>();
        private Dictionary<Vector2Int, bool> m_mainPathPossibleChecks = new Dictionary<Vector2Int, bool>();
        private Dictionary<Vector2Int, List<Vector3>> m_previewPaths = new Dictionary<Vector2Int, List<Vector3>>();
        private Vector2Int m_mainPathStart = Vector2Int.zero;
        private Vector2Int m_mainPathDestination = Vector2Int.zero;
        private List<Vector3> m_mainPath = null;

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(m_size.x * m_cellSize, 0, m_size.y * m_cellSize));
        }

        private void Start()
        {
            m_gridRenderer.material.SetFloat("_RasterCellSize", m_cellSize);
        }

        private void Awake()
        {
            m_aStarCellCosts = new int[m_size.x, m_size.y];

            for (int i = 0; i < m_aStarCellCosts.GetLength(0); i++)
            {
                for (int j = 0; j < m_aStarCellCosts.GetLength(1); j++)
                {
                    m_aStarCellCosts[i, j] = m_aStarNormalCost;
                }
            }
        }

        private void Update()
        {
            if (m_DEBUG_showAllGridPoints)
            {
                showAllGridPoints();
            }
        }

        public void setCellOccupationState(CellOccupationState state, Vector2Int position)
        {
            switch (state)
            {
                case CellOccupationState.Free:
                    m_aStarCellCosts[position.x, position.y] = m_aStarNormalCost;
                    if (m_occupiedCells.Contains(position))
                    {
                        m_occupiedCells.Remove(position);
                    }
                    break;
                case CellOccupationState.Occupied:
                    m_occupiedCells.Add(position);
                    break;
                default:
                    throw new System.NotImplementedException("unkown enum-value " + state);
            }

            m_mainPathPossibleChecks.Clear();
            m_previewPaths.Clear();
        }

        public bool isCellFree(Vector2Int position)
        {
            return !m_occupiedCells.Contains(position);
        }

        public bool isPositionWithinRaster(Vector3 position)
        {
            Vector2Int gridPos = worldToGridPosition(position);

            if (gridPos.x < 0 || gridPos.x >= m_size.x || gridPos.y < 0 || gridPos.y >= m_size.y)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public Vector2Int worldToGridPosition(Vector3 worldPosition)
        {
            return worldToGridPosition(new Vector2(worldPosition.x, worldPosition.z));
        }
        public Vector2Int worldToGridPosition(Vector2 worldPosition)
        {
            Vector2Int returnValue =
            new Vector2Int(
                Mathf.RoundToInt(((worldPosition.x - m_cellSize / 2 - transform.position.x) / m_cellSize)),
                Mathf.RoundToInt(((worldPosition.y - m_cellSize / 2 - transform.position.z) / m_cellSize))
                )
                + m_size / 2;

            return returnValue;
        }

        public Vector3 gridPositionToWorldPosition(Vector2Int gridPosition)
        {
            return new Vector3(
                (gridPosition.x - m_size.x / 2) * m_cellSize + transform.position.x + m_cellSize / 2,
                transform.position.y,
                (gridPosition.y - m_size.y / 2) * m_cellSize + transform.position.z + m_cellSize / 2
                );
        }

        public void showAllGridPoints()
        {
            for (int i = 0; i < m_aStarCellCosts.GetLength(0); i++)
            {
                for (int j = 0; j < m_aStarCellCosts.GetLength(1); j++)
                {
                    Color col;

                    if (m_aStarCellCosts[i, j] > m_aStarNormalCost)
                    {
                        col = Color.red;
                    }
                    else
                    {
                        col = Color.blue;
                    }

                    Debug.DrawRay(gridPositionToWorldPosition(new Vector2Int(i, j)), Vector3.up * 2, col);
                }
            }
        }

        public bool checkPathPossibleIfPositionBlocked(Vector2Int position)
        {
            bool result;

            if (m_mainPathPossibleChecks.TryGetValue(position, out result))
            {
                return result;
            }
            else
            {
                List<Vector2Int> occupiedPositions = new List<Vector2Int>();
                occupiedPositions.AddRange(m_occupiedCells);
                occupiedPositions.Add(position);
                List<Vector3> path = calculatePath(m_mainPathStart, m_mainPathDestination, occupiedPositions);

                if (path == null || path.Count == 0)
                {
                    m_mainPathPossibleChecks.Add(position, false);
                    return false;
                }
                else
                {
                    m_mainPathPossibleChecks.Add(position, true);
                    return true;
                }
            }
        }

        /// <summary>
        /// like checkPathPossibleIfPositionBlocked but only includes positions that have already been checked. does not check new positions
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool checkPathPossibleIfPositionBlockedOnlyCache(Vector2Int position)
        {
            bool result;

            if (m_mainPathPossibleChecks.TryGetValue(position, out result))
            {
                return result;
            }

            return true;
        }

        public List<Vector3> calculateMainPath(Vector3 start, Vector3 destination)
        {
            Vector2Int newStart = worldToGridPosition(start);
            Vector2Int newDestination = worldToGridPosition(destination);

            if (m_mainPathStart != newStart || newDestination != m_mainPathDestination)
            {
                m_mainPathPossibleChecks.Clear();
            }

            m_mainPathStart = newStart;
            m_mainPathDestination = newDestination;

            m_mainPath = calculatePath(start, destination, m_occupiedCells);
            return m_mainPath;
        }

        public List<Vector3> getPreviewPath(Vector2Int blockedTileGridPos)
        {
            List<Vector3> path;

            if (m_previewPaths.TryGetValue(blockedTileGridPos, out path))
            {
                return path;
            }
            else
            {
                List<Vector2Int> occupiedCells = new List<Vector2Int>();
                occupiedCells.AddRange(m_occupiedCells);
                occupiedCells.Add(blockedTileGridPos);

                path = calculatePath(m_mainPathStart, m_mainPathDestination, occupiedCells);

                if (m_mainPath != null)
                {
                    path = getDeltaPath(m_mainPath, path);
                }

                m_previewPaths.Add(blockedTileGridPos, path);

                return path;
            }
        }

        private List<Vector3> getDeltaPath(List<Vector3> basePath, List<Vector3> newPath)
        {
            int minCount = Mathf.Min(basePath.Count, newPath.Count);
            int startDivIndex = 0;

            for (int i = 0; i < minCount; i++)
            {
                if (!basePath[i].Equals(newPath[i]))
                {
                    startDivIndex = i;
                    break;
                }
            }

            int endDivIndex = newPath.Count - 1;

            for (int i = newPath.Count - 1; i > startDivIndex; i--)
            {
                if (!basePath.Contains(newPath[i]))
                {
                    endDivIndex = i;
                    break;
                }
            }

            List<Vector3> deltaPath = new List<Vector3>();
            startDivIndex = Mathf.Max(0, startDivIndex - 1); // VisualPathManager needs 1 more point at start to render path properly
            endDivIndex = Mathf.Min(newPath.Count - 1, endDivIndex + 2); // VisualPathManager needs 1 more point at end to render path properly

            for (int i = startDivIndex; i < endDivIndex; i++)
            {
                deltaPath.Add(newPath[i]);
            }

            return deltaPath;
        }

        private List<Vector3> calculatePath(Vector3 start, Vector3 destination)
        {
            return calculatePath(start, destination, m_occupiedCells);
        }
        private List<Vector3> calculatePath(Vector3 start, Vector3 destination, List<Vector2Int> occupiedCells)
        {
            return calculatePath(worldToGridPosition(start), worldToGridPosition(destination), occupiedCells);
        }
        private List<Vector3> calculatePath(Vector2Int start, Vector2Int destination, List<Vector2Int> occupiedCells)
        {
            //Debug.DrawRay(start, Vector3.up * 2, Color.red);
            //Debug.DrawRay(destination, Vector3.up * 2, Color.red);

            //Debug.DrawRay(gridPositionToWorldPosition(worldToGridPosition(start)), Vector3.up * 2, Color.red);
            //Debug.DrawRay(gridPositionToWorldPosition(worldToGridPosition(destination)), Vector3.up * 2, Color.red);

            List<Vector2Int> path = BasicAStar.findPath(m_aStarCellCosts, start, destination, occupiedCells);

            if (path == null || path.Count == 0)
            {
                //Debug.LogWarning("no Path possible");
            }

            List<Vector3> returnValue = new List<Vector3>();

            for (int i = 0; i < path.Count; i++)
            {
                returnValue.Add(gridPositionToWorldPosition(path[i]));
            }

            return returnValue;
        }

        public void disableGridRenderer()
        {
            m_gridRenderer.gameObject.SetActive(false);
        }

        public void enableGridRenderer()
        {
            m_gridRenderer.gameObject.SetActive(true);
        }

        public void setMousePosition(float x, float y)
        {
            m_gridRenderer.material.SetVector("_MousePositon", new Vector4(x, y, 0, 0));
        }

        public Vector2 getClosestCellMid(Vector3 position)
        {
            return getClosestCellMid(new Vector2(position.x, position.z));
        }
        public Vector2 getClosestCellMid(Vector2 position)
        {
            return new Vector2(
                ((int)(position.x / m_cellSize)) * m_cellSize,
                ((int)(position.y / m_cellSize)) * m_cellSize
                )
                + new Vector2(Mathf.Sign(position.x) * m_cellSize, Mathf.Sign(position.y) * m_cellSize) / 2;
        }
    }
}
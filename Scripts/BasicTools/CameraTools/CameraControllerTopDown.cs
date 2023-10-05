using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools.CameraTools
{
    [RequireComponent(typeof(Camera))]
    public class CameraControllerTopDown : MonoBehaviour
    {
        [SerializeField] public float m_moveSpeed = 1f;
        [SerializeField] private float m_moveSpeedMultiplierTouch = 0.3f;
        [SerializeField] public float m_zoomSpeed = 0.8f;
        [SerializeField] private float m_zoomSpeedMultiplierPinchZoom = 0.1f;
        [SerializeField] private float m_minY = 1f;
        [SerializeField] private float m_maxY = 100f;
        /// <summary>
        /// upper right position
        /// </summary>
        [SerializeField] private Vector2 m_maxVisiblePosWorld;
        /// <summary>
        /// lower left position
        /// </summary>
        [SerializeField] private Vector2 m_minVisiblePosWorld;

#if UNITY_EDITOR
        /// <summary>
        /// will the camera move if cursor is at the edge of the screen (editor only)
        /// </summary>
        [SerializeField] private bool m_noMouseTranslationEditor = false;
#endif

        private Camera m_camera;
        private int m_lastFrameZoomTouch = 0;
        private float m_lastZoomTouchDistance = 0;
        public bool m_isStatic = false;
        private Vector2? m_lastTouchScreenPosition = null;

        private void Awake()
        {
            m_camera = GetComponent<Camera>();
        }

        private void Update()
        {
            if (m_isStatic)
            {
                return;
            }

            float mouseTranslationX = 0f;
            float mouseTranslationY = 0f;

            // TODO: is mouse screen movement a problem on mobile?
            if (Input.mousePosition.x / Screen.width > 0.99f)
            {
                mouseTranslationX = 1f;
            }
            else if (Input.mousePosition.x / Screen.width < 0.01f)
            {
                mouseTranslationX = -1f;
            }

            if (Input.mousePosition.y / Screen.height > 0.99f)
            {
                mouseTranslationY = 1f;
            }
            else if (Input.mousePosition.y / Screen.height < 0.01f)
            {
                mouseTranslationY = -1f;
            }

#if UNITY_EDITOR
            if (m_noMouseTranslationEditor)
            {
                mouseTranslationX = 0f;
                mouseTranslationY = 0f;
            }
#endif

            if (Input.touchCount > 0) // no translation at if cursor is at the edge of the screen in mobile
            {
                mouseTranslationX = 0f;
                mouseTranslationY = 0f;
            }

            Vector3 moveDistance = new Vector3();

            if (mouseTranslationX == 0f)
            {
                moveDistance.x = Input.GetAxis("Horizontal") * m_moveSpeed * Time.deltaTime;
            }
            else
            {
                moveDistance.x = mouseTranslationX * m_moveSpeed * Time.deltaTime;
            }

            Vector2 moveDistanceTouch = getTouchMovement();
            Vector3 touchZoomMovement = getPinchZoomMovement();

            moveDistance.x += moveDistanceTouch.x;
            moveDistance.x += touchZoomMovement.x;

            moveDistance.y = -Input.mouseScrollDelta.y * m_zoomSpeed;
            moveDistance.y += touchZoomMovement.y;

            if (mouseTranslationY == 0f)
            {
                moveDistance.z = Input.GetAxis("Vertical") * m_moveSpeed * Time.deltaTime;
            }
            else
            {
                moveDistance.z = mouseTranslationY * m_moveSpeed * Time.deltaTime;
            }

            moveDistance.z += moveDistanceTouch.y;
            moveDistance.z += touchZoomMovement.z;

            transform.position += moveDistance;

            if (transform.position.y > m_maxY)
            {
                transform.position = new Vector3(transform.position.x, m_maxY, transform.position.z);
            }
            else if (transform.position.y < m_minY)
            {
                transform.position = new Vector3(transform.position.x, m_minY, transform.position.z);
            }

            adjustPositionKeepWithinRange();
        }

        private Vector3 getPinchZoomMovement()
        {
            Vector3 returnValue = Vector3.zero;

            if (Input.touchCount > 1)
            {
                float touchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);

                if (m_lastFrameZoomTouch == Time.frameCount - 1)
                {
                    float yDistance = (m_lastZoomTouchDistance - touchDistance) * m_zoomSpeed * m_zoomSpeedMultiplierPinchZoom;

                    if (Camera.main.transform.position.y + yDistance < m_minY)
                    {
                        yDistance = -(Camera.main.transform.position.y - m_minY);
                    }

                    if (yDistance < 0)
                    {
                        Vector2 zoomCenterScreen = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2;
                        Vector3 zoomDestinationPosition = Camera.main.ScreenToWorldPoint(new Vector3(zoomCenterScreen.x, zoomCenterScreen.y, Camera.main.transform.position.y));
                        Vector3 cameraToDestination = zoomDestinationPosition - Camera.main.transform.position;
                        float lerpDistance = Mathf.Abs((yDistance / Mathf.Cos(Vector3.Angle(cameraToDestination, Vector3.down) * Mathf.Deg2Rad)) / cameraToDestination.magnitude);
                        Vector3 newCameraPosition = Vector3.Lerp(Camera.main.transform.position, zoomDestinationPosition, lerpDistance);
                        Vector3 movement = newCameraPosition - Camera.main.transform.position;
                        returnValue = new Vector3(movement.x, yDistance, movement.z);
                    }
                    else
                    {
                        returnValue = new Vector3(0, yDistance, 0);
                    }
                }

                m_lastFrameZoomTouch = Time.frameCount;
                m_lastZoomTouchDistance = touchDistance;
            }

            return returnValue;
        }

        private Vector2 getTouchMovement()
        {
            Vector2 returnValue = Vector2.zero;

            if (Input.touchCount == 1)
            {
                Vector3 currentMousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                if (m_lastTouchScreenPosition != null)
                {
                    returnValue = (m_lastTouchScreenPosition.Value - new Vector2(currentMousePos.x, currentMousePos.y)) * m_moveSpeed * m_moveSpeedMultiplierTouch;
                }

                m_lastTouchScreenPosition = new Vector2(currentMousePos.x, currentMousePos.y);
            }
            else
            {
                m_lastTouchScreenPosition = null;
            }

            return returnValue;
        }

        private void adjustPositionKeepWithinRange()
        {
            Vector2 cameraMin;
            Vector2 cameraMax;

            getCameraViewCorners(out cameraMin, out cameraMax);

            bool xTooLow = false;
            bool yTooLow = false;

            if (cameraMin.x < m_minVisiblePosWorld.x)
            {
                float distance = cameraMin.x - m_minVisiblePosWorld.x;
                transform.position = new Vector3(transform.position.x - distance, transform.position.y, transform.position.z);
                xTooLow = true;
            }
            if (cameraMax.x > m_maxVisiblePosWorld.x)
            {
                if (xTooLow) // x too low and too high -> camera too high on y
                {
                    float minXDistance = Mathf.Min(Mathf.Abs(m_minVisiblePosWorld.x), Mathf.Abs(m_maxVisiblePosWorld.x));
                    float cameraBorderAngle = Vector3.Angle(Vector3.right, new Vector3(cameraMax.x, -transform.position.y, 0));

                    float newHeight = Mathf.Tan(Mathf.Deg2Rad * cameraBorderAngle) * minXDistance;
                    transform.position = new Vector3(0, newHeight, transform.position.z);

                    Debug.LogWarning("Camera too heigh to keep within bounds: adjust max height to " + transform.position.y);
                    m_maxY = transform.position.y;
                }
                else
                {
                    float distance = cameraMax.x - m_maxVisiblePosWorld.x;
                    transform.position = new Vector3(transform.position.x - distance, transform.position.y, transform.position.z);
                }
            }

            if (cameraMin.y < m_minVisiblePosWorld.y)
            {
                float distance = cameraMin.y - m_minVisiblePosWorld.y;
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - distance);
                yTooLow = true;
            }
            if (cameraMax.y > m_maxVisiblePosWorld.y)
            {
                if (yTooLow) // z too low and too high -> camera too high on y
                {
                    float minZDistance = Mathf.Min(Mathf.Abs(m_minVisiblePosWorld.y), Mathf.Abs(m_maxVisiblePosWorld.y));
                    float cameraBorderAngle = Vector3.Angle(Vector3.forward, new Vector3(cameraMax.y, -transform.position.y, 0));

                    float newHeight = Mathf.Tan(Mathf.Deg2Rad * cameraBorderAngle) * minZDistance;
                    transform.position = new Vector3(transform.position.x, newHeight, 0);

                    Debug.LogWarning("Camera too heigh to keep within bounds: adjust max height to " + transform.position.y);
                    m_maxY = transform.position.y;
                }
                else
                {
                    float distance = cameraMax.y - m_maxVisiblePosWorld.y;
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - distance);
                }
            }
        }

        private void getCameraViewCorners(out Vector2 min, out Vector2 max)
        {
            Vector3 min3 = m_camera.ViewportToWorldPoint(new Vector3(0, 0, transform.position.y));
            Vector3 max3 = m_camera.ViewportToWorldPoint(new Vector3(1, 1, transform.position.y));

            min = new Vector2(min3.x, min3.z);
            max = new Vector2(max3.x, max3.z);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            Vector2 viewRect = m_maxVisiblePosWorld - m_minVisiblePosWorld;

            Gizmos.DrawRay(new Vector3(m_maxVisiblePosWorld.x, 0, m_maxVisiblePosWorld.y), Vector3.up);
            Gizmos.DrawRay(new Vector3(m_minVisiblePosWorld.x, 0, m_minVisiblePosWorld.y), Vector3.up);
            Gizmos.DrawWireCube(new Vector3(m_minVisiblePosWorld.x + viewRect.x / 2, 0, m_minVisiblePosWorld.y + viewRect.y / 2), new Vector3(viewRect.x, 0, viewRect.y));

            if (m_camera == null)
            {
                m_camera = GetComponent<Camera>();
            }

            Vector3 max = m_camera.ViewportToWorldPoint(new Vector3(1, 1, transform.position.y));
            Vector3 min = m_camera.ViewportToWorldPoint(new Vector3(0, 0, transform.position.y));

            Gizmos.color = Color.blue;

            Gizmos.DrawRay(max, Vector3.up * 2);
            Gizmos.DrawRay(min, Vector3.up * 2);
            Gizmos.DrawRay(transform.position, transform.forward);
        }
    }
}
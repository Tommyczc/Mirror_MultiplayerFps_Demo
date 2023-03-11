using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.UI.ModernUIPack
{
    public class TooltipManager : MonoBehaviour
    {
        // Resources
        public Canvas mainCanvas;
        public GameObject tooltipObject;
        public GameObject tooltipContent;

        // Settings
        [Range(0.01f, 0.5f)] public float tooltipSmoothness = 0.1f;
        [Range(5, 10)] public float dampSpeed = 10;
        public float preferredWidth = 375;
        public bool allowUpdating = false;

        // Bounds
        public int vBorderTop = -115;
        public int vBorderBottom = 100;
        public int hBorderLeft = 230;
        public int hBorderRight = -210;

        Vector2 uiPos;
        Vector3 cursorPos;
        Vector3 contentPos = new Vector3(0, 0, 0);
        Vector3 tooltipVelocity = Vector3.zero;
        RectTransform tooltipRect;
        RectTransform tooltipZHelper;
        [HideInInspector] public LayoutElement contentLE;

        void Start()
        {
            RectTransform sourceRect = gameObject.GetComponent<RectTransform>();
            sourceRect.anchorMin = new Vector2(0, 0);
            sourceRect.anchorMax = new Vector2(1, 1);
            sourceRect.offsetMin = new Vector2(0, 0);
            sourceRect.offsetMax = new Vector2(0, 0);

            tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(0f, tooltipContent.GetComponent<RectTransform>().pivot.y);
            tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(tooltipContent.GetComponent<RectTransform>().pivot.x, 0f);

            if (mainCanvas == null)
                mainCanvas = gameObject.GetComponentInParent<Canvas>();

            tooltipZHelper = gameObject.GetComponentInParent<RectTransform>();
            tooltipRect = tooltipObject.GetComponent<RectTransform>();
            contentPos = new Vector3(vBorderTop, hBorderLeft, 0);
            gameObject.transform.SetAsLastSibling();
        }

        void Update()
        {
            if (allowUpdating == false)
                return;

#if ENABLE_LEGACY_INPUT_MANAGER
            cursorPos = Input.mousePosition;
#elif ENABLE_INPUT_SYSTEM
                cursorPos = Mouse.current.position.ReadValue();
#endif
            cursorPos.z = tooltipZHelper.position.z;
            uiPos = tooltipRect.anchoredPosition;
            CheckForBounds();

            if (mainCanvas.renderMode == RenderMode.ScreenSpaceCamera || mainCanvas.renderMode == RenderMode.WorldSpace)
            {
                tooltipRect.position = Camera.main.ScreenToWorldPoint(cursorPos);
                tooltipContent.transform.localPosition = Vector3.SmoothDamp(tooltipContent.transform.localPosition, contentPos, ref tooltipVelocity, tooltipSmoothness, dampSpeed * 1000, Time.unscaledDeltaTime);
            }

            else if (mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                tooltipRect.position = cursorPos;
                tooltipContent.transform.position = Vector3.SmoothDamp(tooltipContent.transform.position, cursorPos + contentPos, ref tooltipVelocity, tooltipSmoothness, dampSpeed * 1000, Time.unscaledDeltaTime);
            }
        }

        public void CheckForBounds()
        {
            if (uiPos.x <= -400)
            {
                contentPos = new Vector3(hBorderLeft, contentPos.y, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(0f, tooltipContent.GetComponent<RectTransform>().pivot.y);
            }

            else if (uiPos.x >= 400)
            {
                contentPos = new Vector3(hBorderRight, contentPos.y, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(1f, tooltipContent.GetComponent<RectTransform>().pivot.y);
            }

            if (uiPos.y <= -325)
            {
                contentPos = new Vector3(contentPos.x, vBorderBottom, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(tooltipContent.GetComponent<RectTransform>().pivot.x, 0f);
            }

            else if (uiPos.y >= 325)
            {
                contentPos = new Vector3(contentPos.x, vBorderTop, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(tooltipContent.GetComponent<RectTransform>().pivot.x, 1f);
            }
        }
    }
}
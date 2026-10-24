using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NepalTrial.Vehicles.Car
{
    /// <summary>
    /// Selectable left/right car indicators for U-turn assessment.
    /// Keyboard Z/X or two mobile buttons. Signals do not apply vehicle torque.
    /// </summary>
    public sealed class CarTurnIndicators : MonoBehaviour
    {
        public bool Left { get; private set; }
        public bool Right { get; private set; }

        public void ToggleLeft()
        {
            bool next=!Left;
            Left=next;
            Right=false;
        }
        public void ToggleRight()
        {
            bool next=!Right;
            Right=next;
            Left=false;
        }
        public void ResetIndicators() {Left=Right=false;}

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if(Keyboard.current==null)return;
            if(Keyboard.current.zKey.wasPressedThisFrame)ToggleLeft();
            if(Keyboard.current.xKey.wasPressedThisFrame)ToggleRight();
#endif
        }

        private void Start()
        {
            Canvas canvas=FindFirstObjectByType<Canvas>();
            if(canvas==null)return;
            AddButton(canvas.transform,"Signal Left [Z]",new Vector2(25,-335),ToggleLeft);
            AddButton(canvas.transform,"Signal Right [X]",new Vector2(215,-335),ToggleRight);
        }

        private static void AddButton(Transform canvas,string title,Vector2 position,
            UnityEngine.Events.UnityAction click)
        {
            var root=new GameObject(title,typeof(RectTransform),typeof(CanvasRenderer),
                typeof(Image),typeof(Button));
            root.transform.SetParent(canvas,false);
            RectTransform box=root.GetComponent<RectTransform>();
            box.anchorMin=box.anchorMax=new Vector2(0,1);
            box.pivot=new Vector2(0,1);
            box.anchoredPosition=position;
            box.sizeDelta=new Vector2(170,58);
            root.GetComponent<Image>().color=new Color(.11f,.28f,.36f,.95f);
            root.GetComponent<Button>().onClick.AddListener(click);
            var label=new GameObject("Label",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text))
                .GetComponent<Text>();
            label.transform.SetParent(root.transform,false);
            RectTransform rect=label.GetComponent<RectTransform>();
            rect.anchorMin=Vector2.zero;
            rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize=19;
            label.color=Color.white;
            label.text=title;
            label.alignment=TextAnchor.MiddleCenter;
            label.raycastTarget=false;
        }
    }
}

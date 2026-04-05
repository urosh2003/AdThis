using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Texture2D hoverCursor;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;
    public Image jimmyMatrix;
    public Sprite jimmyRelaxSprite;

    public Sprite jimmyHandSprite;

    public void OnPointerEnter(PointerEventData eventData)
    {
        jimmyMatrix.sprite = jimmyHandSprite;
        //Cursor.SetCursor(hoverCursor, hotspot, cursorMode);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        jimmyMatrix.sprite = jimmyRelaxSprite;
        //Cursor.SetCursor(null, Vector2.zero, cursorMode); // reset to default
    }
}
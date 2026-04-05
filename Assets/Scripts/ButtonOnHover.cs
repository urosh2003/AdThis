using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image jimmyMatrix;
    public Sprite jimmyRelaxSprite;

    public Sprite jimmyHandSprite;

    public void OnPointerEnter(PointerEventData eventData)
    {
        jimmyMatrix.sprite = jimmyHandSprite;
        ButtonSFX.instance.PlayRandomButtonSFX();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        jimmyMatrix.sprite = jimmyRelaxSprite;
    }
}
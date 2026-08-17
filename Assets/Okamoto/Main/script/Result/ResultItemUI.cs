using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultItemUI : MonoBehaviour
{
    public Image itemImage;
    public TMP_Text amountText;

    [Header("ÉAÉCÉRÉì")]
    public Sprite exp1Sprite;
    public Sprite exp2Sprite;
    public Sprite exp3Sprite;
    public Sprite exp4Sprite;


    public void SetItem(
        DropItemType type,
        int amount)
    {
        if (amountText != null)
        {
            amountText.text = "Å~" + amount;
        }

        if (itemImage == null)
            return;


        switch (type)
        {
            case DropItemType.Exp1:
                itemImage.sprite = exp1Sprite;
                break;

            case DropItemType.Exp2:
                itemImage.sprite = exp2Sprite;
                break;

            case DropItemType.Exp3:
                itemImage.sprite = exp3Sprite;
                break;

            case DropItemType.Exp4:
                itemImage.sprite = exp4Sprite;
                break;
        }
    }
}
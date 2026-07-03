using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AmmoSlot
{
    public Image emptyImage; // 空枠
    public Image image;      // 弾

    public AmmoType ammoType;

    [HideInInspector]
    public GameObject recoverEffectObject;

    [HideInInspector]
    public bool isRecovering;

    [HideInInspector]
    public bool isLoaded = true;
}
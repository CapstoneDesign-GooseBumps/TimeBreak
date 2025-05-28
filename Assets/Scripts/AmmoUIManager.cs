using UnityEngine;
using TMPro;

public class AmmoUIManager : MonoBehaviour
{
    public TextMeshProUGUI ammoText;

    public void UpdateAmmo(int current, int max)
    {
        ammoText.text = $"{current} / {max}";
    }
}

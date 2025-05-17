using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI ammoText;

    public void UpdateAmmo(int current, int max)
    {
        ammoText.text = $"{current} / {max}";
    }
}

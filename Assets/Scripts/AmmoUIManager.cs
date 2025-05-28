using UnityEngine;
using TMPro;

public class AmmoUIManager : MonoBehaviour
{
    public TextMeshProUGUI ammoText;

    public void UpdateAmmo(int magazine, int reserve)
    {
        ammoText.text = $"{magazine} / {reserve}";
    }
}
using UnityEngine;
using TMPro;

public class AmmoUIManager : MonoBehaviour
{
    public TextMeshProUGUI ammoText;

    public void UpdateAmmo(int magazine, int reserve)
    {
        ammoText.text = $"{magazine} / {reserve}";
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
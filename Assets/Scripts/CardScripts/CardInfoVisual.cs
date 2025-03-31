using TMPro;
using UnityEngine;

public class CardInfoVisual : MonoBehaviour
{
    [SerializeField] private bool isInit = false;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI cardDescription;

    public void Init(string cname, string description)
    {
        cardName.text = cname;
        cardDescription.text = description;
        isInit = true;
        this.gameObject.SetActive(false);
    }

    public void ShowInfo()
    {
        if (!isInit) return;
        this.gameObject.SetActive(true);
    }

    public void HideInfo()
    {
        if (!isInit) return;
        this.gameObject.SetActive(false);
    }
}

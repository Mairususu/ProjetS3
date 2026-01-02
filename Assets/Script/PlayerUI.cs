using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private RectTransform lifeFill;
    [SerializeField] private TMP_Text lifeText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text reloadText;

    public void UpdateLife(float currentLife, float maxLife)
    {
        lifeFill.anchorMax = new Vector2(currentLife/maxLife, lifeFill.anchorMax.y);
        lifeText.text = currentLife + "/" + maxLife + " PV";
    }

    public void UpdateDamage(float damage)
    {
        
    }

    public void UpdateReload(float reload)
    {
        
    }
}

using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private RectTransform lifeFill;
    [SerializeField] private TMP_Text lifeText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text reloadText;
    [SerializeField] private TMP_Text speedText;

    public void UpdateLife(float currentLife, float maxLife)
    {
        lifeFill.anchorMax = new Vector2(currentLife/maxLife, lifeFill.anchorMax.y);
        lifeText.text = currentLife + "/" + maxLife + " PV";
    }

    public void UpdateDamage(float damage)
    {
        damageText.text = damage.ToString()+ "/balle";
    }

    public void UpdateReload(float reload)
    {
        reloadText.text = reload.ToString()+"/balle";
    }

    public void UpdateSpeed(float speed)
    {
        speedText.text = speed.ToString();
    }
}

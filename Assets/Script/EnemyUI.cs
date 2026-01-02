using UnityEngine;

public class EnemyUI : MonoBehaviour
{
    [SerializeField] private RectTransform lifeFill;
    [SerializeField] private GameObject UI;

    public void UpdateLife(float currentLife, float maxLife)
    {
        if (currentLife >= maxLife)
        {
            UI.SetActive(false);
        }
        else if(currentLife <= 0)

        {
            UI.SetActive(false);
        }
        else
        {
            UI.SetActive(true);
            lifeFill.anchorMax = new Vector2(currentLife/maxLife, lifeFill.anchorMax.y);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class MinimapTracker : MonoBehaviour
{
    public Transform playerTransform;
    public Transform targetTransform;
    private float mapRadius = 110f;

    private RectTransform rectTransform;
    private Image image;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }
    private void LateUpdate()
    {
        float distToTarget = Vector3.Distance(targetTransform.position, playerTransform.position);
        if (distToTarget < 5f)
        {
            image.enabled = false;
            return;
        } else
        {

            image.enabled = true;
        }
      
        Vector3 directionToTarget = (targetTransform.position - playerTransform.position).normalized;


        float angle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg - playerTransform.eulerAngles.y;

        angle = -angle + 90;
        if (angle < 0) angle += 360;

        rectTransform.localEulerAngles = new Vector3(0, 0, angle - 90);

        rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * mapRadius;
    
    }




}

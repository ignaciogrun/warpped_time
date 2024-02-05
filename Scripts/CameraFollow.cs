using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public PlayerController target;

    public float xSmoothSpeed = 35f;
    public float ySmoothSpeed = 50f;
    public float xSmoothSpeed = 35f;

    public float verticalOffset = 1.5f;
    public float facingOffset = 0.75f;
    
    public Vector3 offset;

    private Vector3 velocity;
    private bool isShaking = false;

    void LateUpdate()
    {
        if (!target.dead)
            SmoothFollow();
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        if (!isShaking)
            StartCoroutine(Shake(duration, magnitude));
    }

    private void SmoothFollow()
    {
        Vector3 curPos = transform.position;
        Vector3 targetPos = target.transform.position + offset;

        targetPos.x += GetFacingOffset();
        
        float posX = Mathf.SmoothDamp(curPos.x, targetPos.x, ref velocity.x, xSmoothSpeed * Time.deltaTime);
        float posY = Mathf.SmoothDamp(curPos.y, targetPos.y, ref velocity.y, ySmoothSpeed * Time.deltaTime);

        transform.position = new Vector3(posX, posY, offset.z);
    }

    private float GetFacingOffset()
    {
        if (target.moveDirection.x > 0)
            return facingOffset;
        
        if (target.moveDirection.x < 0)
            return -facingOffset;

        return 0;
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        isShaking = true;
        
        Vector3 orignalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position += new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return 0;
        }

        transform.position = orignalPosition;
        isShaking = false;
    }
}

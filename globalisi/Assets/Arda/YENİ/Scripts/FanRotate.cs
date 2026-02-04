using UnityEngine;

public class FanRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Pervanenin dönme hızı (derece/saniye)")]
    public float rotationSpeed = 180f;

    void Update()
    {
        // Y ekseninde sürekli dönüş
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}

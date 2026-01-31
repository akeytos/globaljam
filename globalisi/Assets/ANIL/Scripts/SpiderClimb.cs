using UnityEngine;

public class SpiderClimb : MonoBehaviour
{
    public float climbSpeed = 5f;
    public float rayDistance = 1.2f;
    public LayerMask climbLayer;

    private CharacterController controller;
    private PlayerMovement pm;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        pm = GetComponent<PlayerMovement>();
        if (climbLayer == 0) climbLayer = LayerMask.GetMask("Climbable");
    }

    void Update()
    {
        // Iþýn baþlangýcýný biraz daha öne aldým (Kendi kapsülüne çarpmasýn diye)
        Vector3 origin = transform.position + Vector3.up * 1.2f + transform.forward * 0.4f;
        RaycastHit hit;

        bool isWall = Physics.Raycast(origin, transform.forward, out hit, rayDistance, climbLayer);
        Debug.DrawRay(origin, transform.forward * rayDistance, isWall ? Color.green : Color.red);

        if (isWall)
        {
            pm.isClimbingNow = true;
            controller.stepOffset = 0f;

            float v = Input.GetAxisRaw("Vertical");

            // LOG BURAYA EKLENDÝ
            if (v != 0) Debug.Log("Duvara týrmanýyorum! Giriþ: " + v);

            Vector3 move = new Vector3(0, v * climbSpeed, 0) + (transform.forward * 0.2f);
            controller.Move(move * Time.deltaTime);
        }
        else
        {
            if (pm.isClimbingNow)
            {
                pm.isClimbingNow = false;
                controller.stepOffset = 0.3f;
            }
        }
    }
}
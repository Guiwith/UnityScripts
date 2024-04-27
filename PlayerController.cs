using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform mainCamera;
    private Rigidbody rb;

    public float moveSpeed = 6f;
    public float rotateSpeed = 6f;
    public float jumpForce = 3f;

    void Start()
    {
        mainCamera = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        Vector3 cameraForward = Vector3.Scale(mainCamera.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveDir = cameraForward * Input.GetAxis("Vertical") + mainCamera.right * Input.GetAxis("Horizontal");
        Move(moveDir);

        Vector3 targetDir = Vector3.Lerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(targetDir);
    }

    void Move(Vector3 moveDir)
    {
        Vector3 moveAmount = moveDir * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveAmount);
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}

using System.CodeDom.Compiler;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 throwVector;

    [Header("Movement Props")]
    public float speed;
    public float throwForce;
    public float maxVelocity;

    [Header("References")]
    public GameObject rockPrefab;
    public GameObject projectilesObject;

    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {
        transform.position += new Vector3(Input.GetAxis("Horizontal"), 0, 0) * speed;
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);
    }

    void Update() {
        if (Input.GetMouseButtonDown(0))
        {
            // Find vector/direction to throw player
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 distance = mousePosition - this.transform.position;
            throwVector = -distance.normalized * throwForce * 100f;

            // Create and throw rock + add force to player
            var newRock = Instantiate(rockPrefab, transform.position, transform.rotation, projectilesObject.transform);
            newRock.GetComponent<Rigidbody2D>().AddForce(-throwVector);
            Destroy(newRock, 10);

            rb.AddForce(throwVector);
        }
    }

}

using System.CodeDom.Compiler;
using TMPro;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 throwVector;

    private bool throwing; // is the player throwing
    private float throwForce; // how long player is holding left click

    [Header("Movement Props")]
    public float speed;
    public float throwForceMultiplier;
    public float maxVelocity;

    [Header("References")]
    public GameObject rockPrefab;
    public GameObject projectilesObject;
    public TextMeshProUGUI throwForceText;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        transform.position += new Vector3(Input.GetAxis("Horizontal"), 0, 0) * speed;
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);
    }

    void Update()
    {
        throwForceText.text = throwForce.ToString();
        if (Input.GetMouseButton(0))
        {
            throwing = true;
            throwForce += Time.deltaTime * throwForceMultiplier * 100f;
        }
        else
        {
            if (throwing)
            {
                // Find vector/direction to throw player
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 distance = mousePosition - this.transform.position;
                throwVector = -distance.normalized * throwForce;

                // Create and throw rock + add force to player
                var newRock = Instantiate(rockPrefab, transform.position, transform.rotation, projectilesObject.transform);
                newRock.GetComponent<Rigidbody2D>().AddForce(-throwVector);
                Destroy(newRock, 10);

                rb.AddForce(throwVector, ForceMode2D.Force);

                throwForce = 0f;
                throwing = false;
            }
        }
    }

}

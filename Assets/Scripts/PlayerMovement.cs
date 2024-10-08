using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer playerSprite;

    private Vector2 throwVector;
    private bool throwing; // is the player throwing
    private float throwForce; // how long player is holding left click

    private float throwDebounce; // debounce vars
    private bool canThrow;

    [Header("Movement Props")]
    public float horizontalSpeed;
    public float throwForceMultiplier;
    public float maxThrowForce;
    public float initalForce; // How much force is applied if the player simply clicks
    public float throwCooldown;
    public int rockCount; // Does not need to be public

    [Header("References")]
    public GameObject rockPrefab;
    public GameObject projectilesObject;
    public TextMeshProUGUI throwForceText;
    public TextMeshProUGUI throwDebounceText;
    public TextMeshProUGUI rockCountText;

    [Header("Ground Check")]
    public Vector2 boxSize;
    public float castDistance;
    public LayerMask groundLayer;
    public float boxXOffset;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerSprite = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        var horizontalMovement = Input.GetAxis("Horizontal");
        if (isGrounded()) {
            transform.position += new Vector3(horizontalMovement, 0, 0) * horizontalSpeed; // Move player based on input
        }
        boxCollider.sharedMaterial.friction = isGrounded() && Mathf.Abs(horizontalMovement) > 0f ? 0f : 5f; // Set friction if player is moving/grounded
    }

    void Update()
    {
        throwForceText.text = "throwForce: " + Mathf.Clamp(throwForce, 0f, maxThrowForce).ToString("F2");
        throwDebounceText.text = "throwDebounce: " + throwDebounce.ToString("F3");

        rockCountText.text = "Rocks: " + rockCount.ToString();

        throwDebounce -= throwDebounce <= 0f ? 0f : Time.deltaTime;
        canThrow = throwDebounce <= 0f;

        if (Input.GetMouseButton(0) && canThrow && (rockCount > 0 || rockCount < 0))
        {
            throwing = true;
            throwForce += Time.deltaTime * throwForceMultiplier * 100f;
        }
        else if (throwing)
        {
            throwRock(throwForce <= maxThrowForce / 5f ? initalForce : throwForce);
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mousePos.x > transform.position.x)
        {
            transform.localScale = new Vector3(-0.9f, 0.9f, 0.9f);
            boxXOffset = 0.04f;
        }
        else if (mousePos.x < transform.position.x)
        {
            transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            boxXOffset = -0.04f;
        }
    }

    private void throwRock(float force)
    {
        // Find vector/direction to throw player
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 distance = mousePosition - this.transform.position;
        throwVector = -distance.normalized * Mathf.Clamp(force, 0f, maxThrowForce);

        // Create and throw rock + add force to player
        var newRock = Instantiate(rockPrefab, transform.position, transform.rotation, projectilesObject.transform);
        newRock.GetComponent<Rigidbody2D>().AddForce(-throwVector);
        Destroy(newRock, 10);

        rb.AddForce(throwVector, ForceMode2D.Force);

        throwForce = 0f;
        throwing = false;
        rockCount--;
        throwDebounce = throwCooldown;
    }


    private bool isGrounded()
    {
        return Physics2D.BoxCast(transform.position - new Vector3(boxXOffset, 0), boxSize, 0, Vector2.down, castDistance, groundLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - new Vector3(boxXOffset, 0) - transform.up * castDistance, boxSize);
    }
}

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer playerSprite;
    private LineRenderer lineRenderer;

    private Vector2 dragStartPos; // for trajectory display

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
    public Image cooldownCircle;
    public Image cooldownCover;

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
        lineRenderer = GetComponent<LineRenderer>();
    }

    void FixedUpdate()
    {
        var horizontalMovement = Input.GetAxis("Horizontal");
        if (isGrounded())
        {
            transform.position += new Vector3(horizontalMovement, 0, 0) * horizontalSpeed; // Move player based on input
        }
        //boxCollider.sharedMaterial.friction = isGrounded() && Mathf.Abs(horizontalMovement) > 0f ? 0f : 5f; // Set friction if player is moving/grounded
    }

    void Update()
    {
        throwForceText.text = "throwForce: " + Mathf.Clamp(throwForce, 0f, maxThrowForce).ToString("F2");
        throwDebounceText.text = "throwDebounce: " + throwDebounce.ToString("F3");

        rockCountText.text = "Rocks: " + rockCount.ToString();

        if (throwDebounce > 0f)
        {
            throwDebounce -= Time.deltaTime;
            cooldownCircle.enabled = true;
        }
        else
        {
            throwDebounce = 0f;
            cooldownCircle.enabled = false;
        }

        canThrow = throwDebounce <= 0f;

        cooldownCover.fillAmount = throwDebounce / throwCooldown;

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
            transform.localScale = new Vector3(-1f, 1f, 1f);
            boxXOffset = 0.04f;
        }
        else if (mousePos.x < transform.position.x)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            boxXOffset = -0.04f;
        }

        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 dragEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 dragVelocity = findThrowVector(throwForce) * 0.1f;

            Vector2[] trajectory = PlotTrajectory(rb, (Vector2)transform.position, dragVelocity, Mathf.Clamp((int)(throwForce / 15f), 0, 55));
            lineRenderer.positionCount = trajectory.Length;

            Vector3[] positions = new Vector3[trajectory.Length];
            for (int i = 0; i < trajectory.Length; i++)
            {
                positions[i] = trajectory[i];
            }
            lineRenderer.SetPositions(positions);
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
        /*
        if (Input.GetMouseButtonUp(0)) {
            Vector2 dragEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dragVelocity = (dragEndPos - dragStartPos) * 5f;
        }*/
    }

    private Vector2[] PlotTrajectory(Rigidbody2D rigidbody, Vector2 pos, Vector2 velocity, int steps)
    {
        Vector2[] results = new Vector2[steps];

        float timeStep = Time.fixedDeltaTime / Physics2D.velocityIterations;
        Vector2 gravityAccel = Physics2D.gravity * rigidbody.gravityScale * timeStep * timeStep * 45f;

        float drag = 1f - timeStep * rigidbody.drag;
        Vector2 moveStep = velocity * timeStep;

        for (int i = 0; i < steps; i++)
        {
            moveStep += gravityAccel;
            moveStep *= drag;
            pos += moveStep;
            results[i] = pos;
        }

        return results;
    }

    private Vector2 findThrowVector(float force)
    {
        // Find vector/direction to throw player
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 distance = mousePosition - this.transform.position;
        //throwVector = 
        return -distance.normalized * Mathf.Clamp(force, 0f, maxThrowForce);
    }

    private void throwRock(float force)
    {
        var throwVector = findThrowVector(force);

        /* Create and throw rock + add force to player 
        var newRock = Instantiate(rockPrefab, transform.position, transform.rotation, projectilesObject.transform);
        newRock.GetComponent<Rigidbody2D>().AddForce(-throwVector);
        Destroy(newRock, 10);*/

        IEnumerator coroutine = DelayedThrow(-throwVector);
        StartCoroutine(coroutine);

        rb.AddForce(throwVector, ForceMode2D.Force);

        throwForce = 0f;
        throwing = false;
        rockCount--;
        throwDebounce = throwCooldown;
    }

    private IEnumerator DelayedThrow(Vector2 _throwVector)
    {
        yield return new WaitForSeconds(0.1f);
        var newRock = Instantiate(rockPrefab, transform.position, transform.rotation, projectilesObject.transform);
        newRock.GetComponent<Rigidbody2D>().AddForce(_throwVector);
        Destroy(newRock, 10);
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

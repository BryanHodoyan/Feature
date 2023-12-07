using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebShooter : MonoBehaviour
{
    // Web Shooter 
    private LineRenderer Lr;
    private Vector3 webPoint;
    public LayerMask whatIsWebable;
    public Transform WebShooterNozzle, mainCamera, player;
    private float maxDistance = 100;
    private Rigidbody playerRb;
    private bool isSwinging = false;
    private float currentSwingForce = 0f;
    public float maxSwingForce = 20f;
    public float maxSwingSpeed = 10f;
    public float swingDamping = 5f;

    // Spider-man Air Control
    public Transform Orientation;
    public Rigidbody rb;
    public float horizontalForce;
    public float ForwardForce;
    public float webShortenSpeed = 10f;

    // Web Swinging Predetiction
    public RaycastHit webPredictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;


    private void Awake()
    {
        Lr = GetComponent<LineRenderer>();
        playerRb = player.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isSwinging)
        {
            StartWeb();
        }
        else if (Input.GetMouseButtonUp(0) && isSwinging)
        {
            StopWeb();
        }


        
        CheckForSwingPoints();


        if (isSwinging)
        {
            SpiderManAirControl();
        }
    }

    void LateUpdate()
    {
        DrawWeb();
    }

    private void CheckForSwingPoints()
    {
        RaycastHit sphereCastHit;
        Physics.SphereCast(mainCamera.position, predictionSphereCastRadius, mainCamera.forward, out sphereCastHit, maxDistance, whatIsWebable);

        RaycastHit raycastHit;
        Physics.Raycast(mainCamera.position, mainCamera.forward, out raycastHit, maxDistance, whatIsWebable);

        Vector3 realHitPoint;
        // Option 1 - Direct Hit
        if (raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;

        // Option 2 - Indirect (predicted) Hit
        else if (sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;

        // Option 3 - Miss
        else
            realHitPoint = Vector3.zero;

        // realHitPoint found
        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        // realHitPoint not found
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        webPredictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;

    }

    void StartWeb()
    {
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.position, mainCamera.forward, out hit, maxDistance, whatIsWebable))
        {
            webPoint = hit.point;

            // Calculate the direction from player to the web point
            Vector3 webDirection = webPoint - player.position;
            webDirection.Normalize();

            // Reset current swing force when starting a new swing
            currentSwingForce = 0f;

            isSwinging = true;

            Lr.positionCount = 2;
        }
    }

    void StopWeb()
    {
        Lr.positionCount = 0;
        isSwinging = false;
        currentSwingForce = 0f;
    }

    void SpiderManAirControl()
    {
        // If the player is moving to the right in the air
        if (Input.GetKey(KeyCode.D)) playerRb.AddForce(Orientation.right * 1 * horizontalForce * Time.deltaTime);
        // If the player is moving to the left in the air
        if (Input.GetKey(KeyCode.A)) playerRb.AddForce(Orientation.right * -1 * horizontalForce * Time.deltaTime);

        // If the player is moving forward in the air
        if (Input.GetKey(KeyCode.W)) playerRb.AddForce(Orientation.forward * ForwardForce * Time.deltaTime);

        // Increase the swing force while the button is held down, up to a maximum value
        currentSwingForce = Mathf.Min(currentSwingForce + Time.deltaTime * webShortenSpeed, maxSwingForce);

        // Calculate swing force based on the direction towards the web point
        Vector3 swingForce = (webPoint - player.position).normalized * currentSwingForce;

        // Limit the swing speed
        Vector3 currentVelocity = playerRb.velocity;
        float currentSpeed = currentVelocity.magnitude;

        if (currentSpeed > maxSwingSpeed)
        {
            float speedScale = maxSwingSpeed / currentSpeed;
            playerRb.velocity = currentVelocity * speedScale;
        }

        // Apply the swing force with damping
        playerRb.AddForce(swingForce, ForceMode.VelocityChange);
        playerRb.velocity *= Mathf.Clamp01(1f - Time.deltaTime * swingDamping);
    }

    void DrawWeb()
    {
        // if not webbing to anything, don't draw the web
        if (!isSwinging) return;

        Lr.SetPosition(0, WebShooterNozzle.position);
        Lr.SetPosition(1, webPoint);
    }
}

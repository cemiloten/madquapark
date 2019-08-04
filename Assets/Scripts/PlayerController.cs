using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Range(0.8f, 0.99f)] public float goBackToMiddleSpeed = 0.9f;
    [Range(1.0f, 5.0f)] public float mouseSensitivity = 2.0f;
    public float jumpStrength = 50f;
    public float flyingSpeed = 0.1f;
    public float flyingRotationSpeed = 2f;

    private PathFollower pathFollower;
    private Rigidbody rb;

    private bool flying;
    private bool holdingLeftClick;

    private float xPos;
    private float startingXPos;
    private Vector2 currentPosOnSlide;

    private float MouseScreenXPos => Input.mousePosition.x / Screen.width * mouseSensitivity;

    private void Start() {
        pathFollower = GetComponent<PathFollower>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            startingXPos = MouseScreenXPos;
            holdingLeftClick = true;
        }

        if (Input.GetMouseButtonUp(0)) {
            holdingLeftClick = false;
            xPos = 0f;
        }

        if (flying) {
            FlyingUpdate();
        } else {
            SlidingUpdate();
        }
    }

    private void SlidingUpdate() {
        if (holdingLeftClick) {
            xPos = MouseScreenXPos - startingXPos;
            if (xPos > 1f || xPos < -1f) {
                // Mouse was dragged too far from the slide's center, time to jump !
                flying = true;
                rb.WakeUp();
                rb.AddForce(0f, jumpStrength, 0f);
                rb.useGravity = true;
            }

            // Calculate XY position from mouse's distance to beginning of click hold.
            currentPosOnSlide = PathManager.Instance.GetPositionOnSlideCurve(xPos);
        } else {
            // Player moves back to the middle of the slide over time.
            currentPosOnSlide *= goBackToMiddleSpeed;
        }

        pathFollower.UpdatePositionOnPath();
        transform.position = new Vector3(pathFollower.PositionOnPath.x + currentPosOnSlide.x,
                                         pathFollower.PositionOnPath.y + currentPosOnSlide.y,
                                         pathFollower.PositionOnPath.z);
    }

    private void FlyingUpdate() {
        Transform trs = transform;
        Vector3 pos = trs.position;
        pos += trs.forward * flyingSpeed;
        trs.position = pos;

        if (!holdingLeftClick) {
            return;
        }

        xPos = MouseScreenXPos - startingXPos;
        transform.Rotate(transform.up, xPos * flyingRotationSpeed);
    }

    private void OnTriggerEnter(Collider collision) {
        switch (collision.gameObject.name) {
            case "Sea":
                pathFollower.PlaceOnFirstPoint(transform);
                break;
            case "Slide":
                Land();
                break;
        }
    }

    private void Land() {
        flying = false;
        pathFollower.PlaceToClosestPoint(transform);
        
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.Sleep();
    }

    private void OnGUI() {
        GUI.contentColor = Color.black;
        GUI.Label(new Rect(20, 20, 500, 50), $"mouse x pos: {xPos}");
    }
}
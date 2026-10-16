
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class CarController : MonoBehaviour
{
    [Header("Movement")]
    public float moveStep = 1f;
    public Vector3 moveDirection = Vector3.forward;

    [Header("Jump")]
    public float jumpHeight = 3.5f;
    public float jumpDuration = 0.8f;
    public float jumpDistance = 11f;

    [Header("Finish Line")]
    public Transform finishLine;
    public float finishDistance = 0.5f;

    [Header("UI")]
    public TMP_Text distanceText;

    [Header("Collision")]
    public Collider carCollider;

    [Header("Sound Effects")]
    public AudioClip jumpSound;
    public AudioClip warningSound;
    public AudioClip victorySound;

    private AudioSource audioSource;
    private Vector3 direction;
    private Vector3 jumpStartPosition;
    private float groundY;
    private float jumpTimer;

    private bool isJumping;
    private bool isFinished;

    private HashSet<Collider> touchingObstacles =
        new HashSet<Collider>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        carCollider = GetComponent<Collider>();

        groundY = transform.position.y;

        direction = moveDirection;
        direction.y = 0f;
        direction.Normalize();

        UpdateDistanceUI();
    }

    void Update()
    {
        if (isFinished || finishLine == null ||
            direction == Vector3.zero)
            return;

        if (!isJumping &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            transform.position += direction * moveStep;
        }

        if (!isJumping &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            isJumping = true;
            jumpTimer = 0f;
            jumpStartPosition = transform.position;

            PlaySound(jumpSound);
        }

        if (isJumping)
        {
            jumpTimer += Time.deltaTime;

            float t = Mathf.Clamp01(
                jumpTimer / Mathf.Max(0.01f, jumpDuration)
            );

            float height =
                4f * jumpHeight * t * (1f - t);

            Vector3 newPosition =
                jumpStartPosition + direction * jumpDistance * t;

            newPosition.y = groundY + height;
            transform.position = newPosition;

            if (t >= 1f)
                isJumping = false;
        }

        Physics.SyncTransforms();

        CheckAllObstacles();
        CheckFinish();
        UpdateDistanceUI();
    }

    void CheckAllObstacles()
    {
        if (carCollider == null) return;

        HashSet<Collider> currentlyTouching =
            new HashSet<Collider>();

        GameObject[] obstacles =
            GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject obj in obstacles)
        {
            Collider obstacleCollider =
                obj.GetComponent<Collider>();

            if (obstacleCollider == null) continue;

            bool touching = Physics.ComputePenetration(
                carCollider,
                carCollider.transform.position,
                carCollider.transform.rotation,
                obstacleCollider,
                obstacleCollider.transform.position,
                obstacleCollider.transform.rotation,
                out Vector3 pushDirection,
                out float pushDistance
            );

            if (touching)
            {
                currentlyTouching.Add(obstacleCollider);

                if (!touchingObstacles.Contains(obstacleCollider))
                {
                    PlaySound(warningSound);
                }
            }
        }

        touchingObstacles = currentlyTouching;
    }

    void CheckFinish()
    {
        Vector3 difference =
            finishLine.position - transform.position;

        difference.y = 0f;

        float remaining =
            Vector3.Dot(difference, direction);

        if (remaining <= finishDistance)
        {
            isFinished = true;
            isJumping = false;

            PlaySound(victorySound);
            UpdateDistanceUI();
        }
    }

    void UpdateDistanceUI()
    {
        if (distanceText == null ||
            finishLine == null)
            return;

        if (isFinished)
        {
            distanceText.text = "Finish!";
            return;
        }

        Vector3 difference =
            finishLine.position - transform.position;

        difference.y = 0f;

        distanceText.text =
            "Distance: " +
            difference.magnitude.ToString("F1") + " m";
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}

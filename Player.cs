using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class Player : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PowerBar powerBar;
    [SerializeField] private Slider fishSlider;

    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform throwIndicator;
    [SerializeField] private RectTransform hitBar;
    [SerializeField] private RectTransform hitIndicator;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private TMP_Text fishText;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask waterMask;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float throwSpeed;

    private Vector2 moveDirection;
    private Vector3 lookRotation;
    private Vector3 velocity;
    private Vector3 prevPos;

    private float gravity = -9.82f;
    private float checkRadius = 0.4f;
    private float lookXRotation = 0.0f;
    private float throwPower = 0.0f;
    private float maxPower = 15.0f;
    private float fishCheck = 0.0f;
    private float fishTimer = 0.0f;

    private float[] clickY      = { -70, -50, -30, 0, 20, 40, 60 };
    private float[] minClick    = { 0.0f, 1.0f, 3.0f, 6.5f, 8.0f, 10.0f, 12.0f };
    private float[] maxClick    = { 2.0f, 4.0f, 6.0f, 9.5f, 11.0f, 13.0f, 15.0f };
    private float biasClick = 0.25f;

    private int power = 1;
    private int clickIndex = 1;
    private int maxClickIndex = 6;

    private bool isGrounded = false;
    private bool hasClicked = false;
    private bool nextClick = false;
    private bool canDrag = false;
    private bool dragging = false;
    private bool fishOnLine = false;

    private string fishString = "";

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerCameraTransform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Vector3 newPos = hitIndicator.transform.localPosition;

        newPos.x = clickY[maxClickIndex];

        hitIndicator.transform.localPosition = newPos;

        fishSlider.gameObject.SetActive(false);
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, checkRadius, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 move = transform.right * moveDirection.x + transform.forward * moveDirection.y;

        characterController.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        bool maxDepth = Physics.CheckSphere(groundCheck.position, 0.2f, waterMask);

        if (maxDepth)
        {
            characterController.transform.position = prevPos;
        }
        else
        {
            prevPos = characterController.transform.position;
        }

        if (hasClicked)
        {
            ThrowPower();

            Vector3 newPos = hitBar.transform.localPosition;

            if (nextClick)
                newPos.x = clickY[0];
            else
                newPos.x = clickY[clickIndex];

            hitBar.transform.localPosition = newPos;
        }

        if (canDrag)
        {
            if (dragging)
            {
                Vector3 indicatorPos = throwIndicator.localPosition;
                indicatorPos.y = 4.3f - transform.position.y;
                indicatorPos.z -= Time.deltaTime;
                throwIndicator.localPosition = indicatorPos;

                if (indicatorPos.z < 2.5f)
                {
                    canDrag = false;
                    dragging = false;

                    if (fishOnLine)
                    {
                        infoText.text = "Fish caught";
                        fishText.text = "";
                        fishSlider.gameObject.SetActive(false);
                        fishOnLine = false;
                        fishCheck = 0.0f;
                    }
                }

                fishCheck += Time.deltaTime;

                if (!fishOnLine)
                {
                    if (fishCheck >= 0.1f && Random.value > 0.9975f)
                    {
                        fishCheck = 0.0f;
                        infoText.text = "Fish!";
                        fishOnLine = true;
                    }
                    else if (fishCheck >= 0.1f)
                    {
                        fishCheck = 0.0f;
                    }
                }
            }

            RaycastHit hit;
            if (Physics.SphereCast(throwIndicator.position, 0.1f, Vector3.down, out hit))
            {
                if (hit.collider.name == "Terrain" && hit.transform != throwIndicator)
                {
                    canDrag = false;
                    dragging = false;
                    infoText.text = "Hit ground";
                }
            }
        }


        if (fishOnLine)
        {
            if (fishString == "")
            {
                fishCheck += Time.deltaTime;

                float randomValue = Random.value;

                if (fishCheck >= 5.0f && randomValue < 0.25f)
                {
                    fishText.text = "W";
                    fishString = "W";
                    fishCheck = 0.0f;
                    fishSlider.gameObject.SetActive(true);
                }

                if (fishCheck >= 5.0f && randomValue > 0.25f && randomValue < 0.5f)
                {
                    fishText.text = "S";
                    fishString = "S";
                    fishCheck = 0.0f;
                    fishSlider.gameObject.SetActive(true);
                }

                if (fishCheck >= 5.0f && randomValue > 0.5f && randomValue < 0.75f)
                {
                    fishText.text = "D";
                    fishString = "D";
                    fishCheck = 0.0f;
                    fishSlider.gameObject.SetActive(true);
                }

                if (fishCheck >= 5.0f && randomValue > 0.75f)
                {
                    fishText.text = "A";
                    fishString = "A";
                    fishCheck = 0.0f;
                    fishSlider.gameObject.SetActive(true);
                }
            }
            else
            {
                fishTimer += Time.deltaTime;

                fishSlider.value = 3.0f - fishTimer;

                if (fishTimer > 3.0f)
                {
                    infoText.text = "Fish missed";
                    fishText.text = "";
                    fishSlider.gameObject.SetActive(false);
                    fishCheck = 0.0f;
                    fishTimer = 0.0f;
                    fishOnLine = false;
                    canDrag = false;
                    dragging = false;
                }
            }
        }

        lineText.text = throwIndicator.localPosition.z.ToString("0.00") + " / 25";
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!canDrag)
        {
            moveDirection = context.ReadValue<Vector2>();
        }
    }

    public void Turn(InputAction.CallbackContext context)
    {
        lookRotation = context.ReadValue<Vector2>();

        float lookX = lookRotation.x * mouseSensitivity;
        float lookY = lookRotation.y * mouseSensitivity;

        lookXRotation -= lookY;
        lookXRotation = Mathf.Clamp(lookXRotation, -90f, 90f);

        playerCameraTransform.localRotation = Quaternion.Euler(lookXRotation, 0f, 0f);
        transform.Rotate(Vector3.up, lookX);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (isGrounded && context.performed)
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed)
            moveSpeed = 10f;
        else
            moveSpeed = 5f;
    }

    public void Cast(InputAction.CallbackContext context)
    {
        if (!canDrag)
        {
            if (context.performed && !hasClicked)
            {
                hasClicked = true;
                throwPower = 0.0f;
                infoText.text = "";
            }

            else if (context.performed && hasClicked && !nextClick)
            {
                if (throwPower > minClick[clickIndex] - biasClick && throwPower < maxClick[clickIndex] + biasClick)
                {
                    if (clickIndex < maxClickIndex)
                        clickIndex++;

                    power = -1;
                    nextClick = true;

                    infoText.text = "";
                }
                else
                {
                    if (clickIndex > 1)
                        clickIndex--;

                    power = -1;
                    nextClick = true;

                    infoText.text = "Too early";
                }
            }
            else if (context.performed && hasClicked)
            {
                if (throwPower > minClick[0] - biasClick && throwPower < maxClick[0] + biasClick)
                {
                    power = 1;
                    nextClick = false;

                    infoText.text = "";
                }
                else
                {
                    if (clickIndex > 1)
                        clickIndex--;

                    power = 1;
                    nextClick = false;

                    infoText.text = "Too early";
                }
            }
        }
    }
    public void Throw(InputAction.CallbackContext context)
    {
        if (context.performed && hasClicked && !canDrag)
        {
            if (throwPower > minClick[clickIndex] - biasClick && throwPower < maxClick[clickIndex] + biasClick)
            {
                hasClicked = false;
                clickIndex = 1;
                canDrag = true;

                infoText.text = "Cast success";

                RaycastHit hit;
                if (Physics.SphereCast(throwIndicator.position, 0.1f, Vector3.down, out hit))
                {
                    if (hit.collider.name == "Terrain" && hit.transform != throwIndicator)
                    {
                        canDrag = false;
                        dragging = false;
                        infoText.text = "Hit ground";
                    }
                }
            }
            else
            {
                hasClicked = false;
                clickIndex = 1;
                infoText.text = "Missed cast";
            }
        }
    }
    public void SetPower(InputAction.CallbackContext context)
    {
        float power = context.ReadValue<float>();

        if (context.performed)
        {
            if (power > 0)
            {
                if (maxClickIndex < 6)
                    maxClickIndex++;
            }
            else
            {
                if (maxClickIndex > 1)
                    maxClickIndex--;
            }
        }

        Vector3 newPos = hitIndicator.transform.localPosition;

        newPos.x = clickY[maxClickIndex];

        hitIndicator.transform.localPosition = newPos;
    }
    public void Drag(InputAction.CallbackContext context)
    {
        if (context.performed && canDrag)
        {
            dragging = true;
        }

        if (context.canceled && canDrag)
        {
            dragging = false;
        }
    }

    public void HitFish(InputAction.CallbackContext context)
    {
        if (fishOnLine)
        {
            Vector2 fishDirection = context.ReadValue<Vector2>();

            if (fishString == "W" && fishDirection.y > 0.0f)
            {
                fishText.text = "";
                fishString = "";
                fishTimer = 0.0f;
                fishSlider.gameObject.SetActive(false);
            }

            if (fishString == "S" && fishDirection.y < 0.0f)
            {
                fishText.text = "";
                fishString = "";
                fishTimer = 0.0f;
                fishSlider.gameObject.SetActive(false);
            }

            if (fishString == "D" && fishDirection.x > 0.0f)
            {
                fishText.text = "";
                fishString = "";
                fishTimer = 0.0f;
                fishSlider.gameObject.SetActive(false);
            }

            if (fishString == "A" && fishDirection.x < 0.0f)
            {
                fishText.text = "";
                fishString = "";
                fishTimer = 0.0f;
                fishSlider.gameObject.SetActive(false);
            }
        }
    }

    private void ThrowPower()
    {
        if (throwPower >= maxPower + biasClick || throwPower >= maxClick[clickIndex] + biasClick)
        {
            power = -1;
            nextClick = true;

            if (clickIndex > 1)
                clickIndex--;

            infoText.text = "Too late";
        }
        if (throwPower < 0.0f - biasClick && nextClick)
        {
            power = 1;
            nextClick = false;

            if (clickIndex > 1)
                clickIndex--;

            infoText.text = "Too late";
        }

        float powerMultiplier = throwPower * 0.25f;
        if (powerMultiplier < 1.0f)
            powerMultiplier = 1.0f;
        throwPower += Time.deltaTime * power * throwSpeed * powerMultiplier;
        powerBar.SetPower(throwPower);
        Vector3 indicatorPos = throwIndicator.localPosition;
        indicatorPos.y = 4.3f - transform.position.y;
        indicatorPos.z = (throwPower * throwPower) / 9.82f;
        throwIndicator.localPosition = indicatorPos;
    }

    public void Quit(InputAction.CallbackContext context)
    {
        if (context.performed)
            Application.Quit();
    }
}

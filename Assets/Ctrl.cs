using UnityEngine;

public static class Ctrl
{
    public static Vector2 WASD;
    public static bool jump;
    public static bool jumpUp;
    public static bool jumpDown;
    public static bool sprint;
    public static bool sprintUp;
    public static bool sprintDown;
    public static bool crouch;
    public static bool crouchUp;
    public static bool crouchDown;
    public static bool forward;
    public static bool forwardUp;
    public static bool forwardDown;
    public static bool back;
    public static bool backUp;
    public static bool backDown;
    public static bool right;
    public static bool rightUp;
    public static bool rightDown;
    public static bool left;
    public static bool leftUp;
    public static bool leftDown;
    public static bool LMB;
    public static bool LMBUp;
    public static bool LMBDown;
    public static bool RMB;
    public static bool RMBUp;
    public static bool RMBDown;

    public static void HandleInput()
    {
        WASD = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        jump = Input.GetKey(KeyCode.Space);
        jumpUp = Input.GetKeyUp(KeyCode.Space);
        jumpDown = Input.GetKeyDown(KeyCode.Space);

        sprint = Input.GetKey(KeyCode.LeftShift);
        sprintUp = Input.GetKeyUp(KeyCode.LeftShift);
        sprintDown = Input.GetKeyDown(KeyCode.LeftShift);

        crouch = Input.GetKey(KeyCode.LeftControl);
        crouchUp = Input.GetKeyUp(KeyCode.LeftControl);
        crouchDown = Input.GetKeyDown(KeyCode.LeftControl);

        left = Input.GetKey(KeyCode.A);
        leftUp = Input.GetKeyUp(KeyCode.A);
        leftDown = Input.GetKeyDown(KeyCode.A);

        right = Input.GetKey(KeyCode.D);
        rightUp = Input.GetKeyUp(KeyCode.D);
        rightDown = Input.GetKeyDown(KeyCode.D);

        forward = Input.GetKey(KeyCode.W);
        forwardUp = Input.GetKeyUp(KeyCode.W);
        forwardDown = Input.GetKeyDown(KeyCode.W);

        back = Input.GetKey(KeyCode.S);
        backUp = Input.GetKeyUp(KeyCode.S);
        backDown = Input.GetKeyDown(KeyCode.S);

        LMB = Input.GetMouseButton(0);
        LMBUp = Input.GetMouseButtonUp(0);
        LMBDown = Input.GetMouseButtonDown(0);

        RMB = Input.GetMouseButton(1);
        RMBUp = Input.GetMouseButtonUp(1);
        RMBDown = Input.GetMouseButtonDown(1);
    }
}

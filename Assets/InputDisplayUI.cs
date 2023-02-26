using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputDisplayUI : MonoBehaviour
{
    [SerializeField] Color onColor = Color.white;
    [SerializeField] Color offColor = Color.white;
    [SerializeField] Image W;
    [SerializeField] Image A;
    [SerializeField] Image S;
    [SerializeField] Image D;
    [SerializeField] Image Shift;
    [SerializeField] Image Control;
    [SerializeField] Image Space;
    [SerializeField] Image LMB;
    [SerializeField] Image RMB;
    [SerializeField] Image mouseImage;
    [SerializeField] Image mouseBGImage;
    [SerializeField] float mouseImageMax = 9;
    [SerializeField] float mouseDeltaMax = 720;


    void Update()
    {
        Check(W, Ctrl.forward);
        Check(A, Ctrl.left);
        Check(S, Ctrl.back);
        Check(D, Ctrl.right);
        Check(Shift, Ctrl.sprint);
        Check(Control, Ctrl.crouch);
        Check(Space, Ctrl.jump);
        Check(LMB, Ctrl.LMB);
        Check(RMB, Ctrl.RMB);

        var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        var ratio = mouseDelta / mouseDeltaMax;
        var imagePos = mouseImageMax * ratio;
        //clamp
        imagePos = new Vector2(Mathf.Clamp(imagePos.x, -mouseImageMax, mouseImageMax), Mathf.Clamp(imagePos.y, -mouseImageMax, mouseImageMax));
        mouseImage.rectTransform.localPosition = imagePos;
        Check(mouseImage, mouseDelta.magnitude > 1f);
        Check(mouseBGImage, mouseDelta.magnitude > 1f);
    }

    void Check(Image _image, bool _bool)
    {
        _image.color = _bool ? onColor : offColor;
    }
}

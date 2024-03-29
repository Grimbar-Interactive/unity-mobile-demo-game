using UnityEngine;

public class MobileInputHandler : InputHandler
{
    public override void UpdateState(ref Vector3 inputPosition)
    {
        switch (Input.touchCount)
        {
            case 0:
            {
                break;
            }

            case > 0:
            {
                var touch = Input.GetTouch(0);

                var isPanTouchOverUI = IsTouchOverUI(touch);
                IsDragging = touch.phase switch
                {
                    TouchPhase.Began when isPanTouchOverUI == false => true,
                    TouchPhase.Ended or TouchPhase.Canceled => false,
                    _ => IsDragging
                };

                inputPosition = touch.position;
                break;
            }
        }
    }
}
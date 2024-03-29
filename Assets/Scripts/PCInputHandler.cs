using UnityEngine;

public class PCInputHandler : InputHandler
{
    public override void UpdateState(ref Vector3 inputPosition)
    {
        var mouseOverUI = Mouse.IsOverUI();

        if (Input.GetMouseButtonDown(0) && !mouseOverUI)
        {
            IsDragging = true;
        } else if (Input.GetMouseButtonUp(0))
        {
            IsDragging = false;
        }
        
        inputPosition = Input.mousePosition;
    }
}
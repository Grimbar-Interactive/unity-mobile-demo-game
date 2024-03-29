using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class InputHandler
{
    public bool IsDragging { get; protected set; }

    public abstract void UpdateState(ref Vector3 inputPosition);
    
    protected static bool IsTouchOverUI(Touch touch)
    {
        if (EventSystem.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = touch.position
        };

        var castResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, castResults);

        return castResults.Any(r => r.gameObject.layer == LayerMask.NameToLayer("UI"));
    }
}

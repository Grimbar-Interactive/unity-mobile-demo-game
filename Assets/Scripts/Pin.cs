using GI.UnityToolkit.Variables;
using UnityEngine;

public class Pin : MonoBehaviour
{
    [SerializeField] private float minAngle = 0.9f;
    [SerializeField] private float considerKnockedOverAfter = 3.0f;
    [SerializeField] private IntVariable knockedOverCountVariable;
    
    private float _timer;
    private bool _isKnockedOver = false;

    private void Update()
    {
        if (transform.up.y < minAngle && !_isKnockedOver)
        {
            _timer += Time.deltaTime;
            if (_timer < considerKnockedOverAfter) return;
            _isKnockedOver = true;
            knockedOverCountVariable.ApplyChange(1);
        }
        else
        {
            _timer = 0;
        }
    }
}

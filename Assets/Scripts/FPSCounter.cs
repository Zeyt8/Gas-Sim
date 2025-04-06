using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _text.text = $"FPS: {Mathf.Round(1 / Time.deltaTime)}";
    }
}

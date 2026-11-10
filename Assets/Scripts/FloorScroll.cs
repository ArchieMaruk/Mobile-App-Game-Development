using UnityEngine;

public class FloorScroll : MonoBehaviour
{
    public float floorLength = 100f; // floor length in meters (from step 3)
    public bool flip;                // tick if it scrolls the wrong way

    Material mat;
    float offset;

    void Awake() => mat = GetComponent<Renderer>().material;

    void Update()
    {
        if (GameManager.Instance == null) return;

        float tilesPerMeter = mat.mainTextureScale.y / floorLength;
        float dir = flip ? -1f : 1f;
        offset += GameManager.Instance.Speed * Time.deltaTime * tilesPerMeter * dir;
        offset %= 1f;

        mat.mainTextureOffset = new Vector2(0f, offset);
    }
}
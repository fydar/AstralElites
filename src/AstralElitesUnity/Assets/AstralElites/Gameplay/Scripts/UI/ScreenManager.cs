using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public static Vector3 MinPosition { get; private set; }
    public static Vector3 MaxPosition { get; private set; }
    public static Vector2 Scale { get; private set; }

    [Header("Settings")]
    [SerializeField] private float defaultSize = 5.0f;
    [SerializeField] private Camera environmentCamera;
    [SerializeField] private Transform[] scaleToScene;

    private int _lastWidth;
    private int _lastHeight;

    private void Awake()
    {
        if (environmentCamera == null)
        {
            environmentCamera = Camera.main;
        }

        Recalculate();
    }

    private void Update()
    {
        if (Screen.width != _lastWidth
            || Screen.height != _lastHeight)
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            Recalculate();
        }
    }

    private void Recalculate()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        environmentCamera.orthographicSize = Mathf.Max(5.0f, defaultSize / aspectRatio);

        MinPosition = environmentCamera.ViewportToWorldPoint(new Vector3(0, 0, environmentCamera.nearClipPlane));
        MaxPosition = environmentCamera.ViewportToWorldPoint(new Vector3(1, 1, environmentCamera.nearClipPlane));

        Scale = new Vector2(MaxPosition.x - MinPosition.x, MaxPosition.y - MinPosition.y);

        if (scaleToScene != null)
        {
            foreach (var scaleObject in scaleToScene)
            {
                if (scaleObject == null)
                {
                    continue;
                }

                scaleObject.localScale = new Vector3(Scale.x, Scale.y, scaleObject.localScale.z);
            }
        }
    }

    public static void Clamp(Transform target, float border = 0.0f)
    {
        Vector3 pos = target.position;
        pos.x = Mathf.Clamp(pos.x, MinPosition.x + border, MaxPosition.x - border);
        pos.y = Mathf.Clamp(pos.y, MinPosition.y + border, MaxPosition.y - border);
        target.position = pos;
    }

    public static bool IsOutside(Vector3 position, float border = 0.0f)
    {
        return position.x < MinPosition.x + border ||
               position.x > MaxPosition.x - border ||
               position.y < MinPosition.y + border ||
               position.y > MaxPosition.y - border;
    }


    public static Vector3 RandomBorderPoint(float border = 0.0f)
    {
        return RandomBorderPoint(new Vector2(border, border));
    }

    public static Vector3 RandomBorderPoint(Vector2 border)
    {
        var minPosition = new Vector3(MinPosition.x + border.x, MinPosition.y + border.x);
        var maxPosition = new Vector3(MaxPosition.x - border.y, MaxPosition.y - border.y);

        float rand = Random.value;
        return Random.Range(0, 4) switch
        {
            0 => new Vector3(Mathf.Lerp(minPosition.x, maxPosition.x, rand), maxPosition.y, 0),
            1 => new Vector3(Mathf.Lerp(minPosition.x, maxPosition.x, rand), minPosition.y, 0),
            2 => new Vector3(maxPosition.x, Mathf.Lerp(minPosition.y, maxPosition.y, rand), 0),
            3 => new Vector3(minPosition.x, Mathf.Lerp(minPosition.y, maxPosition.y, rand), 0),
            _ => Vector3.zero,
        };
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(MinPosition.x, MaxPosition.y, 0.0f), new Vector3(MaxPosition.x, MaxPosition.y, 0.0f));
        Gizmos.DrawLine(new Vector3(MinPosition.x, MinPosition.y, 0.0f), new Vector3(MaxPosition.x, MinPosition.y, 0.0f));

        Gizmos.DrawLine(new Vector3(MinPosition.x, MinPosition.y, 0.0f), new Vector3(MinPosition.x, MaxPosition.y, 0.0f));
        Gizmos.DrawLine(new Vector3(MaxPosition.x, MinPosition.y, 0.0f), new Vector3(MaxPosition.x, MaxPosition.y, 0.0f));
    }
#endif
}

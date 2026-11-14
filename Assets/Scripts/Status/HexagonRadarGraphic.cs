
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 六角形レーダーチャートの共通描画クラス。
/// ゲーム固有のステータス管理やショップ処理は持たない。
/// </summary>
public class HexagonRadarGraphic : Graphic
{
    [Header("現在値（0～100）")]
    [SerializeField, Range(0, 100)]
    private float[] values = { 50, 50, 50, 50, 50, 50 };

    [Header("プレビュー")]
    [SerializeField]
    private bool showPreview;

    [SerializeField, Range(0, 100)]
    private float[] previewValues = { 50, 50, 50, 50, 50, 50 };

    [Header("現在値の色")]
    public Color meterColor = new Color(1f, 0.3f, 0.1f, 0.4f);
    public Color meterLineColor = new Color(1f, 0.2f, 0.05f, 1f);
    public float meterLineWidth = 3f;

    [Header("プレビューの色")]
    public Color previewFillColor = new Color(1f, 0.85f, 0.1f, 0.12f);
    public Color previewLineColor = new Color(1f, 0.85f, 0.1f, 0.8f);
    public float previewLineWidth = 2f;

    [Header("外枠")]
    public Color frameColor = Color.black;
    public float frameLineWidth = 4f;

    [Header("補助線")]
    public Color guideColor = new Color(0f, 0f, 0f, 0.2f);
    public float guideLineWidth = 1.5f;

    [Range(1, 5)]
    public int guideCount = 5;

    private const int AxisCount = 6;

    protected override void OnEnable()
    {
        base.OnEnable();
        EnsureArrays();
        SetVerticesDirty();
    }

    private void OnValidate()
    {
        EnsureArrays();

        meterLineWidth = Mathf.Max(0f, meterLineWidth);
        previewLineWidth = Mathf.Max(0f, previewLineWidth);
        frameLineWidth = Mathf.Max(0f, frameLineWidth);
        guideLineWidth = Mathf.Max(0f, guideLineWidth);

        SetVerticesDirty();
    }

    private void EnsureArrays()
    {
        if (values == null || values.Length != AxisCount)
            values = new float[] { 50, 50, 50, 50, 50, 50 };

        if (previewValues == null || previewValues.Length != AxisCount)
            previewValues = new float[] { 50, 50, 50, 50, 50, 50 };
    }

    /// <summary>
    /// 現在値を設定。順番は呼び出し元が決める6項目。
    /// </summary>
    public void SetValues(
        float v0, float v1, float v2,
        float v3, float v4, float v5)
    {
        values = new float[]
        {
            ClampValue(v0), ClampValue(v1), ClampValue(v2),
            ClampValue(v3), ClampValue(v4), ClampValue(v5)
        };

        SetVerticesDirty();
    }

    /// <summary>
    /// 予測値を表示する。
    /// </summary>
    public void SetPreviewValues(
        float v0, float v1, float v2,
        float v3, float v4, float v5)
    {
        previewValues = new float[]
        {
            ClampValue(v0), ClampValue(v1), ClampValue(v2),
            ClampValue(v3), ClampValue(v4), ClampValue(v5)
        };

        showPreview = true;
        SetVerticesDirty();
    }

    public void ClearPreview()
    {
        showPreview = false;
        SetVerticesDirty();
    }

    private float ClampValue(float value)
    {
        return Mathf.Clamp(value, 0f, 100f);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        EnsureArrays();

        float radius = Mathf.Min(
            rectTransform.rect.width,
            rectTransform.rect.height
        ) * 0.45f;

        Vector2[] directions = new Vector2[AxisCount];

        for (int i = 0; i < AxisCount; i++)
        {
            float angle = 90f - i * 60f;

            directions[i] = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
        }

        Vector2[] frame = MakePoints(directions, radius, null);

        // 補助線
        for (int level = 1; level <= guideCount; level++)
        {
            float r = radius * level / guideCount;
            Vector2[] guide = MakePoints(directions, r, null);

            for (int i = 0; i < AxisCount; i++)
            {
                DrawLine(
                    vh,
                    guide[i],
                    guide[(i + 1) % AxisCount],
                    guideColor,
                    guideLineWidth
                );
            }
        }

        // 中心から各頂点への線
        for (int i = 0; i < AxisCount; i++)
        {
            DrawLine(
                vh, Vector2.zero, frame[i],
                guideColor, guideLineWidth
            );
        }

        // 現在値
        Vector2[] currentPoints =
            MakePoints(directions, radius, values);

        FillPolygon(vh, currentPoints, meterColor);
        DrawPolygon(
            vh, currentPoints, meterLineColor, meterLineWidth
        );

        // 予測値
        if (showPreview)
        {
            Vector2[] points =
                MakePoints(directions, radius, previewValues);

            FillPolygon(vh, points, previewFillColor);
            DrawPolygon(
                vh, points, previewLineColor, previewLineWidth
            );
        }

        // 外枠
        for (int i = 0; i < AxisCount; i++)
        {
            DrawLine(
                vh, frame[i], frame[(i + 1) % AxisCount],
                frameColor, frameLineWidth
            );
        }
    }

    private Vector2[] MakePoints(
        Vector2[] directions, float radius, float[] data)
    {
        Vector2[] points = new Vector2[AxisCount];

        for (int i = 0; i < AxisCount; i++)
        {
            float rate = data == null
                ? 1f
                : Mathf.Clamp01(data[i] / 100f);

            points[i] = directions[i] * radius * rate;
        }

        return points;
    }

    private void FillPolygon(
        VertexHelper vh, Vector2[] points, Color color)
    {
        for (int i = 0; i < AxisCount; i++)
        {
            int index = vh.currentVertCount;

            AddVertex(vh, Vector2.zero, color);
            AddVertex(vh, points[i], color);
            AddVertex(vh, points[(i + 1) % AxisCount], color);

            vh.AddTriangle(index, index + 1, index + 2);
        }
    }

    private void DrawPolygon(
        VertexHelper vh, Vector2[] points, Color color, float width)
    {
        for (int i = 0; i < AxisCount; i++)
        {
            DrawLine(
                vh, points[i], points[(i + 1) % AxisCount],
                color, width
            );
        }
    }

    private void AddVertex(
        VertexHelper vh, Vector2 position, Color color)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.position = position;
        vertex.color = color;
        vh.AddVert(vertex);
    }

    private void DrawLine(
        VertexHelper vh, Vector2 start, Vector2 end,
        Color color, float width)
    {
        if (width <= 0f)
            return;

        Vector2 direction = (end - start).normalized;
        Vector2 normal =
            new Vector2(-direction.y, direction.x) * width * 0.5f;

        int index = vh.currentVertCount;

        AddVertex(vh, start + normal, color);
        AddVertex(vh, start - normal, color);
        AddVertex(vh, end - normal, color);
        AddVertex(vh, end + normal, color);

        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }
}


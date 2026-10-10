
using UnityEngine;
using UnityEngine.UI;

public class StatusRadar : Graphic
{
    [Header("ステータス（0～100）")]
    [Range(0, 100)] public float HP = 50;
    [Range(0, 100)] public float ATK = 50;
    [Range(0, 100)] public float DEF = 50;
    [Range(0, 100)] public float CRI = 50;
    [Range(0, 100)] public float TEC = 50;
    [Range(0, 100)] public float Other = 50;

    [Header("現在のステータス")]
    public Color meterColor = new Color(1f, 0.3f, 0.1f, 0.4f);
    public Color meterLineColor = new Color(1f, 0.2f, 0.05f, 1f);
    public float meterLineWidth = 3f;

    [Header("プレビュー")]
    public Color previewColor = new Color(1f, 0.85f, 0.1f, 1f);
    public float previewLineWidth = 3f;

    [Header("外枠")]
    public Color frameColor = Color.black;
    public float lineWidth = 4f;

    [Header("補助線")]
    public Color guideColor = new Color(0f, 0f, 0f, 0.2f);
    public float guideWidth = 1.5f;
    [Range(1, 5)] public int guideCount = 5;

    [Header("アニメーション")]
    [Tooltip("大きいほど素早く変化します")]
    public float animationSpeed = 4f;

    private float[] displayed = new float[6];
    private float[] actual = new float[6];
    private float[] preview = new float[6];

    private bool initialized;
    private bool previewing;

    protected override void Start()
    {
        base.Start();

        actual = GetActualValues();
        displayed = (float[])actual.Clone();
        preview = (float[])actual.Clone();

        initialized = true;
        SetVerticesDirty();
    }

    private float[] GetActualValues()
    {
        return new float[] { HP, ATK, DEF, Other, CRI, TEC };
    }

    private void Update()
    {
        if (!initialized) return;

        // 現在値は実際のステータスだけを追いかける
        bool changed = false;

        for (int i = 0; i < 6; i++)
        {
            float next = Mathf.Lerp(
                displayed[i],
                actual[i],
                Mathf.Clamp01(animationSpeed * Time.unscaledDeltaTime)
            );

            if (Mathf.Abs(next - actual[i]) < 0.05f)
                next = actual[i];

            if (!Mathf.Approximately(displayed[i], next))
            {
                displayed[i] = next;
                changed = true;
            }
        }

        if (changed)
            SetVerticesDirty();
    }

    public void SetStatus(
        float hp, float atk, float def,
        float cri, float tec, float other)
    {
        HP = Mathf.Clamp(hp, 0f, 100f);
        ATK = Mathf.Clamp(atk, 0f, 100f);
        DEF = Mathf.Clamp(def, 0f, 100f);
        CRI = Mathf.Clamp(cri, 0f, 100f);
        TEC = Mathf.Clamp(tec, 0f, 100f);
        Other = Mathf.Clamp(other, 0f, 100f);

        actual = GetActualValues();
        SetVerticesDirty();
    }

    public void SetPreviewStatus(
        float hp, float atk, float def,
        float cri, float tec, float other)
    {
        // プレビューは現在値と別の配列に保存
        preview = new float[]
        {
            Mathf.Clamp(hp, 0f, 100f),
            Mathf.Clamp(atk, 0f, 100f),
            Mathf.Clamp(def, 0f, 100f),
            Mathf.Clamp(other, 0f, 100f),
            Mathf.Clamp(cri, 0f, 100f),
            Mathf.Clamp(tec, 0f, 100f)
        };

        previewing = true;
        SetVerticesDirty();
    }

    public void ClearPreview()
    {
        previewing = false;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float radius = Mathf.Min(
            rectTransform.rect.width,
            rectTransform.rect.height
        ) * 0.45f;

        Vector2[] directions = new Vector2[6];

        for (int i = 0; i < 6; i++)
        {
            float angle = 90f - i * 60f;

            directions[i] = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
        }

        Vector2[] frame = MakePoints(directions, radius, null);

        // 内側の六角形ガイド線
        for (int level = 1; level <= guideCount; level++)
        {
            float r = radius * level / guideCount;
            Vector2[] guide = MakePoints(directions, r, null);

            for (int i = 0; i < 6; i++)
                DrawLine(vh, guide[i], guide[(i + 1) % 6],
                    guideColor, guideWidth);
        }

        // 中心からのガイド線
        for (int i = 0; i < 6; i++)
            DrawLine(vh, Vector2.zero, frame[i],
                guideColor, guideWidth);

        // 現在のステータス（オレンジ）を常に描く
        Vector2[] currentPoints = MakePoints(directions, radius, displayed);
        FillPolygon(vh, currentPoints, meterColor);
        DrawPolygon(vh, currentPoints, meterLineColor, meterLineWidth);

        // プレビュー中は黄色い輪郭だけを上に重ねる
        // 現在値のオレンジ色の面は消さない
        if (previewing)
        {
            Vector2[] previewPoints = MakePoints(directions, radius, preview);
            DrawPolygon(vh, previewPoints, previewColor, previewLineWidth);
        }

        // 外枠
        for (int i = 0; i < 6; i++)
            DrawLine(vh, frame[i], frame[(i + 1) % 6],
                frameColor, lineWidth);
    }

    private Vector2[] MakePoints(
        Vector2[] directions, float radius, float[] values)
    {
        Vector2[] points = new Vector2[6];

        for (int i = 0; i < 6; i++)
        {
            float rate = values == null
                ? 1f
                : Mathf.Clamp01(values[i] / 100f);

            points[i] = directions[i] * radius * rate;
        }

        return points;
    }

    private void FillPolygon(
        VertexHelper vh, Vector2[] points, Color color)
    {
        for (int i = 0; i < 6; i++)
        {
            int index = vh.currentVertCount;

            AddVertex(vh, Vector2.zero, color);
            AddVertex(vh, points[i], color);
            AddVertex(vh, points[(i + 1) % 6], color);

            vh.AddTriangle(index, index + 1, index + 2);
        }
    }

    private void DrawPolygon(
        VertexHelper vh, Vector2[] points, Color color, float width)
    {
        for (int i = 0; i < 6; i++)
            DrawLine(vh, points[i], points[(i + 1) % 6], color, width);
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
        Vector2 direction = (end - start).normalized;
        Vector2 normal = new Vector2(-direction.y, direction.x)
            * width * 0.5f;

        int index = vh.currentVertCount;

        AddVertex(vh, start + normal, color);
        AddVertex(vh, start - normal, color);
        AddVertex(vh, end - normal, color);
        AddVertex(vh, end + normal, color);

        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }
}




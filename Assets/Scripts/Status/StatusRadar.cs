using UnityEngine;
using UnityEngine.UI;

public class StatusRadar : Graphic
{
    [Header("ステータス（0～100）")]
    [Range(0, 100)]
    public float HP = 100;

    [Range(0, 100)]
    public float ATK = 100;

    [Range(0, 100)]
    public float DEF = 100;

    [Range(0, 100)]
    public float CRI = 100;

    [Range(0, 100)]
    public float TEC = 100;

    // 6個目。画像では下側にラベルがないので、とりあえず0
    [Range(0, 100)]
    public float Other = 100;

    [Header("色")]
    public Color meterColor = new Color(1f, 0.3f, 0.1f, 0.5f);

    public Color lineColor = Color.black;

    [Header("線の太さ")]
    public float lineWidth = 5f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float radius = Mathf.Min(
            rectTransform.rect.width,
            rectTransform.rect.height
        ) * 0.45f;

        // 六角形の方向
        Vector2[] directions = new Vector2[6];

        for (int i = 0; i < 6; i++)
        {
            float angle = 90f - i * 60f;

            directions[i] = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
        }

        // ステータス
        float[] values =
        {
            HP,
            ATK,
            DEF,
            Other,
            CRI,
            TEC
        };

        // 実際の頂点
        Vector2[] points = new Vector2[6];

        for (int i = 0; i < 6; i++)
        {
            float value = Mathf.Clamp01(values[i] / 100f);

            points[i] = directions[i] * radius * value;
        }

        // =========================
        // メーター内部
        // =========================

        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;

            int index = vh.currentVertCount;

            AddVertex(vh, Vector2.zero, meterColor);
            AddVertex(vh, points[i], meterColor);
            AddVertex(vh, points[next], meterColor);

            vh.AddTriangle(
                index,
                index + 1,
                index + 2
            );
        }

        // =========================
        // 外周
        // =========================

        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;

            DrawLine(
                vh,
                points[i],
                points[next]
            );
        }
    }

    private void AddVertex(
        VertexHelper vh,
        Vector2 position,
        Color color)
    {
        UIVertex vertex = UIVertex.simpleVert;

        vertex.position = position;
        vertex.color = color;

        vh.AddVert(vertex);
    }

    private void DrawLine(
        VertexHelper vh,
        Vector2 start,
        Vector2 end)
    {
        Vector2 direction =
            (end - start).normalized;

        Vector2 normal =
            new Vector2(-direction.y, direction.x)
            * lineWidth * 0.5f;

        int index = vh.currentVertCount;

        AddVertex(
            vh,
            start + normal,
            lineColor
        );

        AddVertex(
            vh,
            start - normal,
            lineColor
        );

        AddVertex(
            vh,
            end - normal,
            lineColor
        );

        AddVertex(
            vh,
            end + normal,
            lineColor
        );

        vh.AddTriangle(
            index,
            index + 1,
            index + 2
        );

        vh.AddTriangle(
            index,
            index + 2,
            index + 3
        );
    }

    // 外部から数値を入れる場合
    public void SetStatus(
        float hp,
        float atk,
        float def,
        float cri,
        float tec)
    {
        HP = hp;
        ATK = atk;
        DEF = def;
        CRI = cri;
        TEC = tec;

        SetVerticesDirty();
    }
}


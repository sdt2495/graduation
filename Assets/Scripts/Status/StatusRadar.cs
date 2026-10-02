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

    [Range(0, 100)]
    public float Other = 100;

    [Header("メーター色")]
    public Color meterColor = new Color(1f, 0.3f, 0.1f, 0.5f);

    [Header("外枠")]
    public Color frameColor = Color.black;

    [Header("補助線")]
    public Color guideColor = new Color(0f, 0f, 0f, 0.3f);

    [Header("線の太さ")]
    public float lineWidth = 5f;

    [Header("補助線の太さ")]
    public float guideWidth = 2f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float radius = Mathf.Min(
            rectTransform.rect.width,
            rectTransform.rect.height
        ) * 0.45f;

        // ========================================
        // 六角形の方向
        // ========================================

        Vector2[] directions = new Vector2[6];

        for (int i = 0; i < 6; i++)
        {
            float angle = 90f - i * 60f;

            directions[i] = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
        }

        // ========================================
        // 最大値100の外側の六角形
        // ========================================

        Vector2[] framePoints = new Vector2[6];

        for (int i = 0; i < 6; i++)
        {
            framePoints[i] = directions[i] * radius;
        }

        // ========================================
        // ステータス
        // ========================================

        float[] values =
        {
            HP,
            ATK,
            DEF,
            Other,
            CRI,
            TEC
        };

        // ========================================
        // 実際のステータス頂点
        // ========================================

        Vector2[] points = new Vector2[6];

        for (int i = 0; i < 6; i++)
        {
            float value = Mathf.Clamp01(values[i] / 100f);

            points[i] =
                directions[i] * radius * value;
        }

        // ========================================
        // ① 補助線
        // ========================================

        // 中心から各頂点へ線を引く
        for (int i = 0; i < 6; i++)
        {
            DrawLine(
                vh,
                Vector2.zero,
                framePoints[i],
                guideColor,
                guideWidth
            );
        }

        // ========================================
        // ② 外側の六角形の枠
        // ========================================

        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;

            DrawLine(
                vh,
                framePoints[i],
                framePoints[next],
                frameColor,
                lineWidth
            );
        }

        // ========================================
        // ③ メーター内部
        // ========================================

        // 0のステータスでも完全に消えないようにする
        float minDisplayRadius = 4f;

        // パラメーターの頂点を描画用に調整
        Vector2[] displayPoints = new Vector2[6];

        for (int i = 0; i < 6; i++)
        {
            // 元のポイント
            Vector2 point = points[i];

            // 0の場合だけ、中心から少しだけ離す
            if (point.magnitude < 0.001f)
            {
                displayPoints[i] =
                    directions[i] * minDisplayRadius;
            }
            else
            {
                displayPoints[i] = point;
            }
        }

        // ========================================
        // 六角形を塗りつぶす
        // ========================================

        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;

            int index = vh.currentVertCount;

            AddVertex(
                vh,
                Vector2.zero,
                meterColor
            );

            AddVertex(
                vh,
                displayPoints[i],
                meterColor
            );

            AddVertex(
                vh,
                displayPoints[next],
                meterColor
            );

            vh.AddTriangle(
                index,
                index + 1,
                index + 2
            );
        }
        /*
        // ========================================
        // ④ ステータス部分の外周
        // ========================================

        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;

            DrawLine(
                vh,
                points[i],
                points[next],
                frameColor,
                lineWidth
            );
        }
        */
    }

    // ========================================
    // 頂点追加
    // ========================================

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

    // ========================================
    // 線を描く
    // ========================================

    private void DrawLine(
        VertexHelper vh,
        Vector2 start,
        Vector2 end,
        Color color,
        float width)
    {
        Vector2 direction =
            (end - start).normalized;

        Vector2 normal =
            new Vector2(-direction.y, direction.x)
            * width * 0.5f;

        int index = vh.currentVertCount;

        AddVertex(
            vh,
            start + normal,
            color
        );

        AddVertex(
            vh,
            start - normal,
            color
        );

        AddVertex(
            vh,
            end - normal,
            color
        );

        AddVertex(
            vh,
            end + normal,
            color
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

    // ========================================
    // 外部からステータスを設定
    // ========================================

    public void SetStatus(
    float hp,
    float atk,
    float def,
    float cri,
    float tec,
    float other)
    {
        HP = hp;
        ATK = atk;
        DEF = def;
        CRI = cri;
        TEC = tec;
        Other = other;

        SetVerticesDirty();
    }
}


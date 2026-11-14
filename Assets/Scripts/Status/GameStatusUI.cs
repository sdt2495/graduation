

using UnityEngine;
using TMPro;

public class GameStatusUI : MonoBehaviour
{
    [Header("メインゲーム用レーダーチャート")]
    public StatusRadar statusRadar;

    [Header("ステータス数値表示")]
    public TMP_Text hpValueText;
    public TMP_Text atkValueText;
    public TMP_Text defValueText;
    public TMP_Text criValueText;
    public TMP_Text sanValueText;
    public TMP_Text tecValueText;

    [Header("初期ステータス")]
    [Range(0f, 100f)] public float initialHP = 50f;
    [Range(0f, 100f)] public float initialATK = 50f;
    [Range(0f, 100f)] public float initialDEF = 50f;
    [Range(0f, 100f)] public float initialCRI = 50f;
    [Range(0f, 100f)] public float initialSAN = 50f;
    [Range(0f, 100f)] public float initialTEC = 50f;

    [Header("現在のステータス（0～100）")]
    [Range(0f, 100f)] public float HP = 50f;
    [Range(0f, 100f)] public float ATK = 50f;
    [Range(0f, 100f)] public float DEF = 50f;
    [Range(0f, 100f)] public float CRI = 50f;
    [Range(0f, 100f)] public float SAN = 50f;
    [Range(0f, 100f)] public float TEC = 50f;

    private void Start()
    {
        SetStatus(
            initialHP,
            initialATK,
            initialDEF,
            initialCRI,
            initialTEC,
            initialSAN
        );
    }

    /// <summary>
    /// 6つのステータスをまとめて設定する。
    /// 引数の順番：HP, ATK, DEF, CRI, TEC, SAN
    /// </summary>
    public void SetStatus(
        float hp,
        float atk,
        float def,
        float cri,
        float tec,
        float san)
    {
        HP = Mathf.Clamp(hp, 0f, 100f);
        ATK = Mathf.Clamp(atk, 0f, 100f);
        DEF = Mathf.Clamp(def, 0f, 100f);
        CRI = Mathf.Clamp(cri, 0f, 100f);
        TEC = Mathf.Clamp(tec, 0f, 100f);
        SAN = Mathf.Clamp(san, 0f, 100f);

        RefreshUI();
    }

    /// <summary>
    /// 指定したステータスを1つだけ変更する。
    /// HP / ATK / DEF / CRI / TEC / SAN に対応。
    /// </summary>
    public void SetSingleStatus(string statusName, float value)
    {
        if (string.IsNullOrWhiteSpace(statusName))
        {
            Debug.LogWarning("GameStatusUI: ステータス名が空です。", this);
            return;
        }

        value = Mathf.Clamp(value, 0f, 100f);

        switch (statusName.Trim().ToUpperInvariant())
        {
            case "HP":
                HP = value;
                break;

            case "ATK":
                ATK = value;
                break;

            case "DEF":
                DEF = value;
                break;

            case "CRI":
            case "CRL":
                CRI = value;
                break;

            case "TEC":
                TEC = value;
                break;

            case "SAN":
            case "OTHER":
                SAN = value;
                break;

            default:
                Debug.LogWarning(
                    "GameStatusUI: 不明なステータス名です: " + statusName,
                    this
                );
                return;
        }

        RefreshUI();
    }

    /// <summary>
    /// 数値表示とレーダーチャートを更新する。
    /// </summary>
    public void RefreshUI()
    {
        // 数値表示
        if (hpValueText != null)
            hpValueText.text = "HP  " + HP.ToString("F0");

        if (atkValueText != null)
            atkValueText.text = "ATK  " + ATK.ToString("F0");

        if (defValueText != null)
            defValueText.text = "DEF  " + DEF.ToString("F0");

        if (criValueText != null)
            criValueText.text = "CRI  " + CRI.ToString("F0");

        if (sanValueText != null)
            sanValueText.text = "SAN  " + SAN.ToString("F0");

        if (tecValueText != null)
            tecValueText.text = "TEC  " + TEC.ToString("F0");

        // レーダーチャートを更新
        // StatusRadar の SetStatus の引数順：
        // HP, ATK, DEF, CRI, TEC, SAN
        if (statusRadar != null)
        {
            statusRadar.SetStatus(
                HP,
                ATK,
                DEF,
                CRI,
                TEC,
                SAN
            );
        }
    }
}



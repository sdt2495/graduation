using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("プレイヤーのステータス（0～100）")]
    [SerializeField, Range(0f, 100f)] private float hp = 50f;
    [SerializeField, Range(0f, 100f)] private float atk = 50f;
    [SerializeField, Range(0f, 100f)] private float def = 50f;
    [SerializeField, Range(0f, 100f)] private float cri = 50f;
    [SerializeField, Range(0f, 100f)] private float san = 50f;
    [SerializeField, Range(0f, 100f)] private float tec = 50f;

    // 初期化済みかどうか
    public bool IsInitialized { get; private set; }

    // ほかのクラスから読み取り可能
    public float HP => hp;
    public float ATK => atk;
    public float DEF => def;
    public float CRI => cri;
    public float SAN => san;
    public float TEC => tec;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        IsInitialized = true;
    }

    // 初回だけ初期値を設定する
    // シーン移動後に初期値で上書きしない
    public void InitializeIfNeeded(float initialValue = 50f)
    {
        if (IsInitialized)
            return;

        float value = Mathf.Clamp(initialValue, 0f, 100f);

        hp = value;
        atk = value;
        def = value;
        cri = value;
        san = value;
        tec = value;

        IsInitialized = true;
    }

    // 名前を指定してステータスを取得
    public float GetStat(string statName)
    {
        switch (statName.ToUpperInvariant())
        {
            case "HP": return hp;
            case "ATK": return atk;
            case "DEF": return def;
            case "CRI":
            case "CRL": return cri;
            case "SAN":
            case "OTHER": return san;
            case "TEC": return tec;
            default:
                Debug.LogWarning("不明なステータス名: " + statName);
                return 0f;
        }
    }

    // 名前を指定してステータスを変更
    public void SetStat(string statName, float value)
    {
        value = Mathf.Clamp(value, 0f, 100f);

        switch (statName.ToUpperInvariant())
        {
            case "HP": hp = value; break;
            case "ATK": atk = value; break;
            case "DEF": def = value; break;
            case "CRI":
            case "CRL": cri = value; break;
            case "SAN":
            case "OTHER": san = value; break;
            case "TEC": tec = value; break;
            default:
                Debug.LogWarning("不明なステータス名: " + statName);
                return;
        }

        IsInitialized = true;
    }

    // 全ステータスを一括設定
    public void SetAllStats(
        float newHP,
        float newATK,
        float newDEF,
        float newCRI,
        float newSAN,
        float newTEC)
    {
        hp = Mathf.Clamp(newHP, 0f, 100f);
        atk = Mathf.Clamp(newATK, 0f, 100f);
        def = Mathf.Clamp(newDEF, 0f, 100f);
        cri = Mathf.Clamp(newCRI, 0f, 100f);
        san = Mathf.Clamp(newSAN, 0f, 100f);
        tec = Mathf.Clamp(newTEC, 0f, 100f);

        IsInitialized = true;
    }

    // 増減させたいときに使う
    public void AddStat(string statName, float amount)
    {
        SetStat(statName, GetStat(statName) + amount);
    }
}

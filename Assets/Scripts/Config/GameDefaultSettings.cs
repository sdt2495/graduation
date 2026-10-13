
using UnityEngine;

/// <summary>
/// ゲーム設定の初期値を一括管理
/// </summary>
public static class GameDefaultSettings
{

    #region AudioSetting

    public const float MasterVolume = 1.0f; // マスター音量(0～1)
    public const float BGMVolume = 1.0f; // BGM音量(0～1)
    public const float SystemSEVolume = 1.0f; // システムSE音量(0～1)
    public const float VoiceVolume = 1.0f; // ボイス音量(0～1)
    public const float SEVolume = 1.0f; // SE音量(0～1)
    #endregion

    #region TextSetting

    public const float TextSpeed = 0.05f; // テキスト表示速度(0.01～0.2)
    public const float AutoSpeed = 1.0f; // オート待機時間(0～3)
    public const bool PauseAtPunctuation = true; // 句読点待機

    public const float MessageWindowOpacity = 0.7f; // ウィンドウ不透明度(0～1)
    #endregion
}

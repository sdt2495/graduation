
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("基本ステータス")]
    [Min(1)] public int maxHP = 100;
    [Min(0)] public int attack = 30;
    [Min(0)] public int defense = 20;
}
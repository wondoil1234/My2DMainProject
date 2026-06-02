using UnityEngine;

public class TowerData : MonoBehaviour
{
    [Header("타워 등급")]
    public string towerName;
    public enum TowerGrade { Normal, Rare, Epic, Unique, Legendary }
    public TowerGrade grade;

    [Header("스탯")]
    public int damage = 1;
    public float attackRange = 5f;
    public float attackCooldown = 1f;
}
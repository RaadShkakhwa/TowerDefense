using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New EnemySummonData" , menuName ="Creat EnemySummonData")]
public class EnemySummonData : ScriptableObject
{
    public GameObject EnemyPrefab;
    public int EnemyID;
}

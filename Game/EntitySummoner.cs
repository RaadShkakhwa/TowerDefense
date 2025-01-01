using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EntitySummoner : MonoBehaviour
{
    public static List<Transform> EnemiesinGameTransform;
    public static List<Enemy> EnemiesInGame;
    public static Dictionary<int, GameObject> EnemyPrefabs;
    public static Dictionary<int, Queue<Enemy>> EnemyObjectPools;

    private static bool IsInitialized;

    public static void Init()
    {

        if (!IsInitialized)
        {
            EnemyPrefabs = new Dictionary<int, GameObject>();
            EnemyObjectPools = new Dictionary<int, Queue<Enemy>>();
            EnemiesinGameTransform = new List<Transform>();
            EnemiesInGame = new List<Enemy>();
            EnemySummonData[] Enemies = Resources.LoadAll<EnemySummonData>("Enemies");

            foreach (EnemySummonData enemy in Enemies)
            {

                EnemyPrefabs.Add(enemy.EnemyID, enemy.EnemyPrefab);
                EnemyObjectPools.Add(enemy.EnemyID, new Queue<Enemy>());
            }

            IsInitialized = true;
        }
        else
        {
            Debug.Log("ENTITYSUMMONER : THIS CLASS IS ALREADY INITIALIZED");
        }
    }

    public static Enemy SummonEnemy(int EnemyID)
    {

        Enemy SummonEnemy = null;

        if (EnemyPrefabs.ContainsKey(EnemyID))
        {

            Queue<Enemy> ReferencedQueue = EnemyObjectPools[EnemyID];

            if (ReferencedQueue.Count > 0)
            {
                SummonEnemy = ReferencedQueue.Dequeue();
                SummonEnemy.Init();

                SummonEnemy.gameObject.SetActive(true);
            }
            else
            {
                GameObject NewEnemy = Instantiate(EnemyPrefabs[EnemyID], GameLoopMaster.NodePosition[0], Quaternion.identity);
                SummonEnemy = NewEnemy.GetComponent<Enemy>();
                SummonEnemy.Init();
            }
        }

        else
        {
            Debug.Log($"ENTITIYSUMMONER: ENEMY WITH ID OF {EnemyID} DOES NOT EXIST!");
            return null;
        }
        if(!EnemiesInGame.Contains(SummonEnemy)) EnemiesinGameTransform.Add(SummonEnemy.transform);
        if(!EnemiesInGame.Contains(SummonEnemy)) EnemiesInGame.Add( SummonEnemy );
        SummonEnemy.ID = EnemyID;
        return SummonEnemy;

    }

    public static void RemoveEnemy(Enemy EnemyToRemove)
    {

        EnemyObjectPools[EnemyToRemove.ID].Enqueue(EnemyToRemove);
        EnemyToRemove.gameObject.SetActive(false);
        EnemiesinGameTransform.Remove(EnemyToRemove.transform);
        EnemiesInGame.Remove(EnemyToRemove);

    }
}
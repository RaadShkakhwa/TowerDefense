using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;

public class GameLoopMaster : MonoBehaviour
{
    public static Vector3[] NodePosition;
    public static float[] NodeDistance;
    public static List<TowerBehavior> TowersInGame;

    private static Queue<EnemyDamageData> DamageData;
    private static Queue<Enemy> EnemiesToRemove;
    private static Queue<int> EnemyIDsToSummon;


    public Transform NodeParent;
    public bool LoopSouldEnd;


    private void Start()
    {
        DamageData = new Queue<EnemyDamageData>();
        TowersInGame = new List<TowerBehavior>();
        EnemyIDsToSummon = new Queue<int>();
        EnemiesToRemove = new Queue<Enemy>();
        EntitySummoner.Init();


        NodePosition = new Vector3[NodeParent.childCount];

        for (int i = 0; i < NodePosition.Length; i++)
        {
            NodePosition[i] = NodeParent.GetChild(i).position;
        }

        NodeDistance = new float[NodePosition.Length - 1];

        for (int i = 0; i < NodeDistance.Length; i++)
        {
            NodeDistance[i] = Vector3.Distance(NodePosition[i], NodePosition[i + 1]);
        }
        StartCoroutine(GameLoop());
        InvokeRepeating("SummonTest", 0f, 1f);
    }

    void SummonTest()
    {
        EnqueueEnemyIDToSummon(1); // Test summoning an enemy with ID 1
    }

    IEnumerator GameLoop()
    {
        while (!LoopSouldEnd)
        {
            // Summon enemies
            if (EnemyIDsToSummon.Count > 0)
            {
                // Collect all IDs before dequeuing
                int summonCount = EnemyIDsToSummon.Count;
                for (int i = 0; i < summonCount; i++)
                {
                    EntitySummoner.SummonEnemy(EnemyIDsToSummon.Dequeue());
                }
            }

            // Move Enemies
            NativeArray<Vector3> NodesToUse = new NativeArray<Vector3>(NodePosition, Allocator.TempJob);
            NativeArray<float> EnemySpeeds = new NativeArray<float>(EntitySummoner.EnemiesInGame.Count, Allocator.TempJob);
            NativeArray<int> NodeIndices = new NativeArray<int>(EntitySummoner.EnemiesInGame.Count, Allocator.TempJob);
            TransformAccessArray EnemyAccess = new TransformAccessArray(EntitySummoner.EnemiesinGameTransform.ToArray(), 2);

            for (int i = 0; i < EntitySummoner.EnemiesInGame.Count; i++)
            {
                EnemySpeeds[i] = EntitySummoner.EnemiesInGame[i].baseSpeed;
                NodeIndices[i] = EntitySummoner.EnemiesInGame[i].NodeIndex;
            }

            MoveEnemiesJob MoveJob = new MoveEnemiesJob
            {
                NodePosition = NodesToUse,
                Enemyspeed = EnemySpeeds,
                NodeIndex = NodeIndices,
                deltaTime = Time.deltaTime
            };

            JobHandle MoveJobHandle = MoveJob.Schedule(EnemyAccess);
            MoveJobHandle.Complete();

            // Update Enemy States and Check for Removal
            for (int i = 0; i < EntitySummoner.EnemiesInGame.Count; i++)
            {
                EntitySummoner.EnemiesInGame[i].NodeIndex = NodeIndices[i];

                if (EntitySummoner.EnemiesInGame[i].NodeIndex == NodePosition.Length)
                {
                    EnqueueEnemyToRemove(EntitySummoner.EnemiesInGame[i]);
                }
            }

            // Dispose of Native Arrays and TransformAccessArray after usage
            EnemySpeeds.Dispose();
            NodeIndices.Dispose();
            EnemyAccess.Dispose();
            NodesToUse.Dispose();




            //Damage Enemies:

            if (DamageData.Count > 0)
            {
                for (int i = 0; i < DamageData.Count; i++)
                {

                    EnemyDamageData CurrentDamageData = DamageData.Dequeue();
                    CurrentDamageData.TargetedEnemy.Health -= CurrentDamageData.TotalDamage / CurrentDamageData.Resistance;


                    if (CurrentDamageData.TargetedEnemy.Health <= 0f)
                    {
                        if (!EnemiesToRemove.Contains(CurrentDamageData.TargetedEnemy))
                        {

                            EnqueueEnemyToRemove(CurrentDamageData.TargetedEnemy);


                        }
                    }

                }
            }


            // Remove Enemies

            if (DamageData.Count > 0)
            {
                int damageCount = DamageData.Count; // Store initial count as dequeuing reduces the count dynamically

                while (DamageData.Count > 0)
                {
                    EnemyDamageData currentDamageData = DamageData.Dequeue();
                    if (currentDamageData.TargetedEnemy != null)
                    {
                        currentDamageData.TargetedEnemy.ApplyDamage(currentDamageData.TotalDamage);
                    }
                }

            }


            // Wait for the next frame
            yield return null;
        









            foreach (TowerBehavior tower in TowersInGame)
            {
                tower.Target = TowerTargeting.GetTarget(tower, TowerTargeting.TargetType.Close);
                tower.Tick();
            }


        }
    }






    public static void EnqueueDamageData(EnemyDamageData damageData) 
    {
    
    DamageData.Enqueue(damageData);
    
    }


    public static void EnqueueEnemyIDToSummon(int ID)
    {
        EnemyIDsToSummon.Enqueue(ID);
    }

    public static void EnqueueEnemyToRemove(Enemy EnemyToRemove)
    {
        EnemiesToRemove.Enqueue(EnemyToRemove);
    }
}

public struct EnemyDamageData
{
    public EnemyDamageData(Enemy target , float damage , float resistance)
    {
        TargetedEnemy = target;
        TotalDamage = damage;
        Resistance = resistance;
    }
    
    
    public Enemy TargetedEnemy;
    public float TotalDamage;
    public float Resistance;

}





public struct MoveEnemiesJob : IJobParallelForTransform
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> NodePosition;

    [NativeDisableParallelForRestriction]
    public NativeArray<float> Enemyspeed;

    [NativeDisableParallelForRestriction]
    public NativeArray<int> NodeIndex;

    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        if (NodeIndex[index] < NodePosition.Length)
        {
            Vector3 PositionToMoveTo = NodePosition[NodeIndex[index]];
            transform.position = Vector3.MoveTowards(transform.position, PositionToMoveTo, Enemyspeed[index] * deltaTime);

            if (transform.position == PositionToMoveTo)
            {
                NodeIndex[index]++;
            }
        }
    }
}
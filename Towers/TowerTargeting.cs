using Unity.Collections;
using UnityEngine;
using System.Collections;
using Unity.Jobs;

public class TowerTargeting
{
    public enum TargetType
    {
        First,
        Last,
        Close
    }

    public static Enemy GetTarget(TowerBehavior CurrentTower, TargetType TargetMethod)
    {
        // Find enemies in range
        Collider[] EnemiesInRange = Physics.OverlapSphere(CurrentTower.transform.position, CurrentTower.Range, CurrentTower.EnemiesLayer);

        // Exit early if no enemies are in range
        if (EnemiesInRange.Length == 0)
            return null;

        NativeArray<EnemyData> EnemiesToCalculate = new NativeArray<EnemyData>(EnemiesInRange.Length, Allocator.TempJob);
        NativeArray<Vector3> NodePostion = new NativeArray<Vector3>(GameLoopMaster.NodePosition, Allocator.TempJob);
        NativeArray<float> NodeDistance = new NativeArray<float>(GameLoopMaster.NodeDistance, Allocator.TempJob);
        NativeArray<int> EnemyToIndex = new NativeArray<int>(1, Allocator.TempJob);

        for (int i = 0; i < EnemiesInRange.Length; i++)
        {
            Enemy CurrentEnemy = EnemiesInRange[i].transform.parent?.GetComponent<Enemy>();

            // Skip if the enemy is null or not valid
            if (CurrentEnemy == null)
                continue;

            int EnemyIndexList = EntitySummoner.EnemiesInGame.FindIndex(x => x == CurrentEnemy);

            if (EnemyIndexList >= 0)
            {
                EnemiesToCalculate[i] = new EnemyData(CurrentEnemy.transform.position, CurrentEnemy.NodeIndex, CurrentEnemy.Health, EnemyIndexList);
            }
        }

        SearchForEnemy EnemySearchJob = new SearchForEnemy
        {
            _EnemiesToCalculate = EnemiesToCalculate,
            _NodeDistance = NodeDistance,
            _NodePostions = NodePostion,
            _EnemyToIndex = EnemyToIndex,
            CompareValue = Mathf.Infinity,
            TargetingType = (int)TargetMethod,
            TowerPosition = CurrentTower.transform.position
        };

        switch ((int)TargetMethod)
        {
            case 0: // First
                EnemySearchJob.CompareValue = Mathf.Infinity;
                break;
            case 1: // Last
                EnemySearchJob.CompareValue = Mathf.NegativeInfinity;
                break;
            case 2: // Close
                goto case 0;
        }

        JobHandle dependency = new JobHandle();
        JobHandle SearchJobHandle = EnemySearchJob.Schedule(EnemiesInRange.Length, dependency);
        SearchJobHandle.Complete();
        int EnemyIndexToReturn;
        if (EnemyToIndex[0] != -1)
        { 
            EnemyIndexToReturn = EnemiesToCalculate[EnemyToIndex[0]].EnemyIndex;

            EnemiesToCalculate.Dispose();
            NodePostion.Dispose();
            NodeDistance.Dispose();
            EnemyToIndex.Dispose();

            return EntitySummoner.EnemiesInGame[EnemyIndexToReturn];
        }
        // Safeguard against invalid indices
        


         

        EnemiesToCalculate.Dispose();
        NodePostion.Dispose();
        NodeDistance.Dispose();
        EnemyToIndex.Dispose();

        // Return the targeted enemy if valid
     

        return null;
    }


    struct EnemyData
    {
        public EnemyData(Vector3 position, int nodeindex, float hp, int enemyIndex)
        {
            EnemyPostion = position;
            NodeIndex = nodeindex;
            Health = hp;
            EnemyIndex = enemyIndex;
        }

        public Vector3 EnemyPostion;
        public int EnemyIndex;
        public int NodeIndex;
        public float Health;
    }

    struct SearchForEnemy : IJobFor
    {
        public NativeArray<EnemyData> _EnemiesToCalculate;
        public NativeArray<Vector3> _NodePostions;
        public NativeArray<float> _NodeDistance;
        public NativeArray<int> _EnemyToIndex;
        public Vector3 TowerPosition;
        public float CompareValue;
        public int TargetingType;

        public void Execute(int index)
        {
            if (_EnemiesToCalculate[index].EnemyIndex == -1)
                return;

            float CurrentEnemyDistanceToEnd = 0;
            float DistanceToEnemy = 0;

            switch (TargetingType)
            {
                case 0: // First
                    CurrentEnemyDistanceToEnd = GetDistanceToEnd(_EnemiesToCalculate[index]);
                    if (CurrentEnemyDistanceToEnd < CompareValue)
                    {
                        _EnemyToIndex[0] = index;
                        CompareValue = CurrentEnemyDistanceToEnd;
                    }
                    break;

                case 1: // Last
                    DistanceToEnemy = GetDistanceToEnd(_EnemiesToCalculate[index]);
                    if (DistanceToEnemy > CompareValue)
                    {
                        _EnemyToIndex[0] = index;
                        CompareValue = DistanceToEnemy;
                    }
                    break;

                case 2: // Close
                    CurrentEnemyDistanceToEnd = Vector3.Distance(TowerPosition, _EnemiesToCalculate[index].EnemyPostion);
                    if (CurrentEnemyDistanceToEnd < CompareValue)
                    {
                        _EnemyToIndex[0] = index;
                        CompareValue = CurrentEnemyDistanceToEnd;
                    }
                    break;
            }
        }

        private float GetDistanceToEnd(EnemyData EnemyToEvaluate)
        {
            float FinalDistance = Vector3.Distance(EnemyToEvaluate.EnemyPostion, _NodePostions[EnemyToEvaluate.NodeIndex]);

            for (int i = EnemyToEvaluate.NodeIndex; i < _NodeDistance.Length; i++)
            {
                FinalDistance += _NodeDistance[i];
            }

            return FinalDistance;
        }
    }
}

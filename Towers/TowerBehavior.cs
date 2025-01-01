using UnityEngine;



public class TowerBehavior : MonoBehaviour
{

    public LayerMask EnemiesLayer;

    public Enemy Target;
    public Transform TowerPivot;

    public float Damage;
    public float Firerate;
    public float Range;
    public float Delay;
    private Enemy currentTarget;


    private IDamageMethod CurrentDamageMethodClass;


    void Start()
    {
        CurrentDamageMethodClass = GetComponent<IDamageMethod>();

        if (CurrentDamageMethodClass == null )
        {
            Debug.LogError("TOWERS ; No damage class attached to given tower!");
        }
       
        else
        {
            CurrentDamageMethodClass.Init(Damage, Firerate);

        }



        Delay = 1 / Firerate;

    }

    public void Tick()
    {

        CurrentDamageMethodClass.DamageTick(Target);


        if (Target != null)
        {

            TowerPivot.transform.rotation = Quaternion.LookRotation(Target.transform.position - transform.position);

        }



    }

    void Update()
    {
        currentTarget = TowerTargeting.GetTarget(this, TowerTargeting.TargetType.First);

        if (currentTarget != null)
        {
            Debug.Log($"Target acquired: {currentTarget.name}");
            // Add your targeting or attack logic here
        }
    }
}

using UnityEngine;
using UnityEngine.Rendering;


public interface IDamageMethod
{

    public void DamageTick(Enemy Target);
    public void Init(float Damage, float Firerate);

}





public class StandardDamage : MonoBehaviour, IDamageMethod
{
    private float Damage;
    private float Firerate;
    private float Delay;


    public void Init(float Damage, float Firerate)
    {

        this.Damage = Damage;
        this.Firerate = Firerate;
        Delay = 1f / Firerate;


    }

    public void DamageTick(Enemy Target)
    {
        if (Target)
        {


            if (Delay > 0f)
            {
                Delay -= Time.deltaTime;
                return;
            }


            GameLoopMaster.EnqueueDamageData(new EnemyDamageData(Target, Damage, Target.DamageResistance));
            Delay = 1f / Firerate;
            return;

        }


    }
}













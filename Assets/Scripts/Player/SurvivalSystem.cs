using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalSystem : Singleton<SurvivalSystem>
{
    [SerializeField] private Player player;

    [SerializeField] private int hearts = 20;
    [SerializeField] private int hunger = 10;
    [SerializeField] private float air = 10;
    [SerializeField] private float saturuation = 10f;
    public bool alive => hearts > 0;
    private int maxHearts = 20;
    private int maxSaturation = 20;
    bool _restarted;
    int instantHealthDiff;
    public float health
    {
        get
        {
            return hearts;
        }
        set
        {
            if(value != hearts)
            {
                instantHealthDiff = (int)value - hearts;

                hearts = (int)value;
            }
        }
    }
    [SerializeField] private UI ui;
    public void StartGame()
    {
        _ = StartCoroutine(LifeCycle());
        _ = StartCoroutine(UpdateSurvivalStats());
    }
    public void OnFall(float height)
    {
        if(height > 3)
        {
            height -= 3;
            health = Mathf.Clamp(hearts - height, 0, maxHearts);
        }
    }
    public void OnEat()
    {

    }
    internal bool suffocating;
    IEnumerator UpdateSurvivalStats()
    {
        float second = 0;
        while (Gameplay.instance.isPlaying)
        {
            if (alive)
            {
                if (instantHealthDiff != 0)
                {

                }
                else
                {

                }
                second += Time.deltaTime;
                if (second > 1)
                {
                    second = 0;
                }
                ui.hp.currentValue = health / maxHearts;
                ui.hunger.currentValue = hunger / 10f;
            }
            yield return null;
        }
        yield return null;
    }
    IEnumerator LifeCycle()
    {
        while (Gameplay.instance.isPlaying)
        {
            hearts = maxHearts;
            saturuation = 10f;
            hunger = 10;
            float second = 0;
            while (alive)
            {
                second += Time.deltaTime;
                var headBlockProp = player.physics.headBlock.block;
                if (!headBlockProp.isSolid && !headBlockProp.isWater)
                {
                    suffocating = false;
                }
                else
                {
                    suffocating = true;
                }
                if (suffocating)
                {
                    air = Mathf.Clamp(air - ((headBlockProp.isWater ? 1 : 3) * Time.deltaTime), 0, 10);
                }
                else
                {
                    air = Mathf.Clamp(air + (3 * Time.deltaTime), 0, 10);
                }
                if (second > 1)
                {
                    second = 0;
                    if (health < maxHearts && saturuation > 0)
                    {
                        saturuation = Mathf.Clamp(saturuation - (Time.deltaTime * 1), 0, maxSaturation);
                        health += 1;
                    }
                    if (air == 0)
                    {
                        int damage = 0;
                        if (headBlockProp.isWater)
                        {
                            damage = 2;
                        }
                        else if (headBlockProp.transparency > 0)
                        {
                            damage = 3;
                        }
                        else
                        {
                            damage = 4;
                        }
                        health -= damage;
                    }
                }
                yield return null;
            }
            while (!_restarted)
            {

                yield return null;
            }
            yield return null;
        }
        yield break;
    }
    [System.Serializable]
    public class UI
    {
        public PropertyBarSystem hp, hunger, xp;
    }

}

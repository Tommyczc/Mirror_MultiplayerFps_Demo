using System;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UI;


public class CharacterHealthState : NetworkBehaviour,IDamageable
{
    [Header("Customized")] 
    [Tooltip("if true, this object could be damaged")]
    public bool damageable;
    [Tooltip("if true, this object could be destroyed when health is 0")]
    public bool destroyable;
    public bool showWorldSpaceHealthBar;

    [Header("State Value")]
    [SyncVar]
    public float Health;
    [SerializeField]
    [SyncVar(hook = nameof(onCurrentHealthChanged))]
    public float currentHealth;

    [Header("Health Bar UI")] 
    public GameObject sliderObject;
    private Slider healthSlider;

    [Header("Heal self")] 
    [Tooltip("how long to start cover self")]
    public float healCd;
    public bool allowHealSelf;
    public float healAmountPerSecond;
    private float healTimer;
    private bool healing;
    private Coroutine healCoroutine;

    public virtual void OnValidate()
    {
        currentHealth = Health;
        healing = false;
    }

    private void FixedUpdate()
    {
    }

    void onCurrentHealthChanged(float oldValue,float newValue)
    {
        onHealthChanged();

        float ratio = (float)Math.Round(newValue / Health,2);
        if (showWorldSpaceHealthBar)
        {
            if (healthSlider != null)
            {
                healthSlider.value = healthSlider.maxValue * ratio;
            }
        }
        
        if (destroyable && newValue<=0)
        {
            Destroy();
        }
    }

    public virtual void onHealthChanged()
    {
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        if (sliderObject != null)
        {
            healthSlider = sliderObject.GetComponent<Slider>();
            if (!showWorldSpaceHealthBar)
            {
                sliderObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Damage(float damage)
    {
        Debug.LogWarning($"is damaged, amount: {damage}");
        CmdDamage(damage);
    }

    [Command(requiresAuthority = false)]
    void CmdDamage(float damage)
    {
        if (!damageable) return;
        
        if (currentHealth <= damage)
        {
            currentHealth = 0;
        }
        else
        {
            currentHealth -= damage;
        }
        
        if (currentHealth<Health&&allowHealSelf)
        {
            if (healCoroutine != null)
            {
                StopCoroutine(healCoroutine);
            }
            healing = false;
            healTimer = healCd;
            healCoroutine=StartCoroutine(healProccess());
        }
    }

    public void Heal(float heal=0)
    {
        float healAmount = heal == 0 ? healAmountPerSecond : heal;
        Debug.LogWarning($"is healed, amount: {healAmount}");
        CmdHeal(healAmount);
    }
    
    [Command(requiresAuthority = false)]
    void CmdHeal(float heal)
    {
        if (currentHealth+heal >= Health)
        {
            currentHealth = Health;
        }
        else
        {
            currentHealth += heal;
        }
    }

    protected virtual void Destroy()
    {
    }

    [Server]
    IEnumerator healProccess()
    {
        Debug.LogWarning("start healing");
        healing = true;
        while (healTimer > 0)
        {
            if (healing == false)
            {
                yield break;
            }

            healTimer -= 1;
            yield return new WaitForSeconds(1f);
        }
        
        while (currentHealth < Health)
        {
            if (healTimer>0)
            {
                yield break;
            }
            Heal(healAmountPerSecond);
            yield return new WaitForSeconds(1f);
        }

        if (currentHealth >= Health)
        {
            healing = false;
            yield break;
        }
    }
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class SubmarineHealth: MonoBehaviour
{
    public float CurrentHealth { get => _health; }
    public float MaxHealth { get => maxHealth; }

    [SerializeField]
    private float maxHealth = 100;
    private float _health;

    public void Start()
    {
        _health = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;
    }

    public void Restore()
    {
        _health = maxHealth;
    }    
}
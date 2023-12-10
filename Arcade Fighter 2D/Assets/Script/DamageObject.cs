using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageObject : MonoBehaviour
{
    [SerializeField] private int damage = 0;
    [SerializeField] private int nockBackForce = 0;
    private PlayerController controller;

    public void Init(PlayerController controller)
    {
        this.controller = controller;
    }

    public void ActiveDamage(int amount, int force)
    {
        damage = amount;
        nockBackForce = force;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter = " + other);
        var health = other.GetComponent<Health>();
        Debug.Log("health == " + health);
        if (health)
        {
            StartCoroutine(NockBackProcess(health.gameObject));
            health.TakeDamage(damage);
        }
    }

    IEnumerator NockBackProcess(GameObject target)
    {
        var targetController = target.GetComponentInParent<PlayerController>();

        float direction = controller.transform.localScale.x;

        targetController.Body2d.velocity = Vector2.zero;

        targetController.transform.localScale = new Vector2(-direction, targetController.transform.localScale.y);
        targetController.Body2d.velocity = new Vector2(-direction * nockBackForce * 2, targetController.Body2d.velocity.y);
        yield return new WaitForSeconds(0.2f);
        targetController.Body2d.velocity = Vector2.zero;
    }

}

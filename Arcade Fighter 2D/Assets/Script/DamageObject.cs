using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageObject : MonoBehaviour
{
    [SerializeField] private int damage = 0;
    [SerializeField] private int nockBackForce = 0;
    private PlayerController controller;

    private Vector3 originalPosition;
    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

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

    public void ShakeObject()
    {
        StartCoroutine(Shake(3, 0.1f));
    }

    private IEnumerator Shake(int amount, float magnitude)
    {
        while (amount > 0)
        {
            float x = originalPosition.x + (UnityEngine.Random.Range(-1f, 1f) * magnitude);
            // float y = originalPosition.y + UnityEngine.Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, originalPosition.y, originalPosition.z);

            amount -= 1;

            yield return new WaitForSeconds(0.01f);
        }

        transform.localPosition = originalPosition;
    }

}

using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Log setiap kali ada sesuatu yang masuk ke trigger hitbox player
        Debug.Log($"[AttackHitbox] Terjadi Trigger dengan: '{other.gameObject.name}', Tag: '{other.tag}', Layer: '{LayerMask.LayerToName(other.gameObject.layer)}'");

        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"[AttackHitbox] Hitbox berhasil mengenai Musuh '{other.gameObject.name}'!");

            Health hp = other.GetComponent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                Debug.Log($"[AttackHitbox] Mengurangi darah '{other.gameObject.name}' sebesar {damage}. Sisa darah: {hp.currentHealth}");
            }
            else
            {
                Debug.LogWarning($"[AttackHitbox] PERINGATAN: Target '{other.gameObject.name}' tidak memiliki komponen 'Health'!");
            }

            HitEffect flash = other.GetComponent<HitEffect>();
            if (flash != null)
            {
                flash.Flash();
                Debug.Log($"[AttackHitbox] Memicu efek kedip merah (HitEffect) pada '{other.gameObject.name}'");
            }
            else
            {
                Debug.LogWarning($"[AttackHitbox] PERINGATAN: Target '{other.gameObject.name}' tidak memiliki komponen 'HitEffect'!");
            }

            Knockback knockback = other.GetComponent<Knockback>();
            if (knockback != null)
            {
                knockback.ApplyKnockback(transform.position);
                Debug.Log($"[AttackHitbox] Menerapkan Knockback ke '{other.gameObject.name}'");
            }
            else
            {
                Debug.LogWarning($"[AttackHitbox] PERINGATAN: Target '{other.gameObject.name}' tidak memiliki komponen 'Knockback'!");
            }

            HitSound sound = other.GetComponent<HitSound>();
            if (sound != null)
            {
                sound.PlayHitSound();
                Debug.Log($"[AttackHitbox] Memutar HitSound pada '{other.gameObject.name}'");
            }
            else
            {
                Debug.LogWarning($"[AttackHitbox] PERINGATAN: Target '{other.gameObject.name}' tidak memiliki komponen 'HitSound'!");
            }
        }
    }
}

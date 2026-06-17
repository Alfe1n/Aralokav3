using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Log setiap kali ada sesuatu yang masuk ke trigger hitbox musuh
        Debug.Log($"[EnemyAttackHitbox] Terjadi Trigger dengan: '{other.gameObject.name}', Tag: '{other.tag}', Layer: '{LayerMask.LayerToName(other.gameObject.layer)}'");

        if (other.CompareTag("Player") || other.CompareTag("Player-Orang Utan"))
        {
            Debug.Log($"[EnemyAttackHitbox] Hitbox berhasil mengenai Player '{other.gameObject.name}'!");

            Health hp = other.GetComponent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                Debug.Log($"[EnemyAttackHitbox] Mengurangi darah '{other.gameObject.name}' sebesar {damage}. Sisa darah: {hp.currentHealth}");
            }
            else
            {
                Debug.LogWarning($"[EnemyAttackHitbox] PERINGATAN: Target '{other.gameObject.name}' tidak memiliki komponen 'Health'!");
            }

            HitEffect flash = other.GetComponent<HitEffect>();
            if (flash != null)
            {
                flash.Flash();
                Debug.Log($"[EnemyAttackHitbox] Memicu efek kedip merah (HitEffect) pada '{other.gameObject.name}'");
            }
            else
            {
                Debug.LogWarning($"[EnemyAttackHitbox] PERINGATAN: Target '{other.gameObject.name}' tidak memiliki komponen 'HitEffect'!");
            }

            Knockback knockback = other.GetComponent<Knockback>();
            if (knockback != null)
            {
                knockback.ApplyKnockback(transform.position);
                Debug.Log($"[EnemyAttackHitbox] Menerapkan Knockback ke '{other.gameObject.name}'");
            }
            else
            {
                Debug.LogWarning($"[EnemyAttackHitbox] PERINGATAN: Target '{other.gameObject.name}' tidak memiliki komponen 'Knockback'!");
            }

            // Coba mainkan HitSound dari target (player) terlebih dahulu, fallback ke attacker (musuh)
            HitSound sound = other.GetComponent<HitSound>();
            if (sound == null)
            {
                sound = GetComponentInParent<HitSound>();
                if (sound != null)
                {
                    Debug.Log($"[EnemyAttackHitbox] Target tidak memiliki HitSound, menggunakan fallback HitSound dari Attacker (Musuh)");
                }
            }
            else
            {
                Debug.Log($"[EnemyAttackHitbox] Memutar HitSound dari Target Player");
            }

            if (sound != null)
            {
                sound.PlayHitSound();
            }
            else
            {
                Debug.LogWarning($"[EnemyAttackHitbox] PERINGATAN: Tidak ditemukan komponen 'HitSound' di target maupun attacker!");
            }
        }
    }
}

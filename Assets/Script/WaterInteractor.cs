using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

public class WaterInteractor : MonoBehaviour
{
    private bool isInWater = false;
    private WaterZone currentZone;

    // Original properties to restore
    private float originalSpeed = -1f;
    private float originalChargeSpeed = -1f;
    private float originalZ;

    // Collider settings
    private CapsuleCollider2D capsuleCollider;
    private Vector2 originalCapsuleOffset;
    private Vector2 originalCapsuleSize;
    private bool hasCapsuleCollider = false;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Water Mask Settings")]
    [Tooltip("If left empty, will automatically search for child named 'WaterMask' or 'Watermask'")]
    [SerializeField] private SpriteMask waterMask;

    [SerializeField, Tooltip("Manual offset to raise/lower the collider's bottom when in water")]
    private float colliderBottomOffset = 0f;



    private PlayerMovement playerMovement;
    private EnemyAI enemyAI;
    private BuayaAI buayaAI;
    private BabiHutanAI babiHutanAI;
    private UlarAI ularAI;

    private Sprite solidSquareSprite;

    private Sprite GetSolidSquareSprite()
    {
        if (solidSquareSprite == null)
        {
            Texture2D tex = Texture2D.whiteTexture;
            solidSquareSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
        }
        return solidSquareSprite;
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        // Auto-attach SortingGroup to isolate SpriteMask to this character's renderers
        SortingGroup sg = GetComponent<SortingGroup>();
        if (sg == null)
        {
            sg = gameObject.AddComponent<SortingGroup>();
            if (sr != null)
            {
                sg.sortingLayerName = sr.sortingLayerName;
                sg.sortingOrder = sr.sortingOrder;
            }
        }
        
        playerMovement = GetComponent<PlayerMovement>();
        enemyAI = GetComponent<EnemyAI>();
        buayaAI = GetComponent<BuayaAI>();
        babiHutanAI = GetComponent<BabiHutanAI>();
        ularAI = GetComponent<UlarAI>();

        capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (capsuleCollider != null)
        {
            hasCapsuleCollider = true;
            originalCapsuleOffset = capsuleCollider.offset;
            originalCapsuleSize = capsuleCollider.size;
        }

        // Auto-find waterMask child if not manually assigned
        if (waterMask == null)
        {
            Transform maskChild = transform.Find("Watermask");
            if (maskChild == null) maskChild = transform.Find("WaterMask");
            if (maskChild != null)
            {
                waterMask = maskChild.GetComponent<SpriteMask>();
            }
        }
    }

    public void EnterWater(WaterZone zone)
    {
        if (isInWater) return;
        isInWater = true;
        currentZone = zone;
        Debug.Log($"[WaterInteractor] {gameObject.name} ENTERING WATER (slow: {zone.slowMultiplier})");

        // Shift Z coordinate to -1f for sorting/rendering order (from original WaterZone.cs)
        originalZ = transform.position.z;
        Vector3 pos = transform.position;
        pos.z = -1f;
        transform.position = pos;

        ModifySpeed(zone.slowMultiplier);

        if (zone.sfxSource != null && zone.splashEnterClip != null)
        {
            zone.sfxSource.PlayOneShot(zone.splashEnterClip);
        }

        SetupSpriteMask();

        // Adjust collider so its bottom matches the top of the watermask
        if (hasCapsuleCollider && capsuleCollider != null)
        {
            float maskTopY = 0f;
            if (waterMask != null && waterMask.sprite != null)
            {
                float unitsToTop = (waterMask.sprite.rect.height - waterMask.sprite.pivot.y) / waterMask.sprite.pixelsPerUnit;
                maskTopY = waterMask.transform.localPosition.y + (unitsToTop * waterMask.transform.localScale.y);
            }
            else if (waterMask != null)
            {
                maskTopY = waterMask.transform.localPosition.y + (waterMask.transform.localScale.y / 2f);
            }
            else
            {
                maskTopY = (zone != null) ? zone.sinkAmount : 0.5f;
            }

            maskTopY += colliderBottomOffset;
            
            float origBottom = originalCapsuleOffset.y - originalCapsuleSize.y / 2f;
            
            float newBottom = Mathf.Max(origBottom, maskTopY);
            float newSizeY = originalCapsuleSize.y;
            float newOffsetY = newBottom + newSizeY / 2f;

            Debug.Log($"[WaterInteractor] {gameObject.name} ENTER WATER - maskTopY: {maskTopY}, origBottom: {origBottom}, newBottom: {newBottom}, newSizeY: {newSizeY}, newOffsetY: {newOffsetY}");

            capsuleCollider.size = new Vector2(originalCapsuleSize.x, newSizeY);
            capsuleCollider.offset = new Vector2(originalCapsuleOffset.x, newOffsetY);
        }

        if (sr != null)
        {
            sr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        }
    }

    public void ExitWater()
    {
        if (!isInWater) return;
        isInWater = false;
        Debug.Log($"[WaterInteractor] {gameObject.name} EXITING WATER");

        // Restore Z coordinate (from original WaterZone.cs)
        Vector3 pos = transform.position;
        pos.z = originalZ;
        transform.position = pos;

        // Restore original collider offset and size
        if (hasCapsuleCollider && capsuleCollider != null)
        {
            capsuleCollider.offset = originalCapsuleOffset;
            capsuleCollider.size = originalCapsuleSize;
        }

        RestoreSpeed();

        if (currentZone != null && currentZone.sfxSource != null && currentZone.splashExitClip != null)
        {
            currentZone.sfxSource.PlayOneShot(currentZone.splashExitClip);
        }

        if (waterMask != null)
        {
            waterMask.enabled = false;
        }

        if (sr != null)
        {
            sr.maskInteraction = SpriteMaskInteraction.None;
        }

        currentZone = null;
    }

    private void SetupSpriteMask()
    {
        if (waterMask == null)
        {
            GameObject maskGo = new GameObject("Watermask");
            maskGo.transform.SetParent(transform);
            
            float targetScaleY = (currentZone != null) ? currentZone.sinkAmount : 0.5f;
            float targetOffsetY = targetScaleY / 2f;

            maskGo.transform.localPosition = new Vector3(0f, targetOffsetY, 0f);
            maskGo.transform.localScale = new Vector3(10f, targetScaleY, 1f);
            
            waterMask = maskGo.AddComponent<SpriteMask>();
        }

        if (waterMask != null)
        {
            if (waterMask.sprite == null)
            {
                waterMask.sprite = GetSolidSquareSprite();
            }
            waterMask.enabled = true;
        }
    }

    private void ModifySpeed(float multiplier)
    {
        if (playerMovement != null)
        {
            originalSpeed = playerMovement.moveSpeed;
            playerMovement.moveSpeed *= multiplier;
            playerMovement.SetInWater(true);
        }
        else if (enemyAI != null)
        {
            originalSpeed = enemyAI.moveSpeed;
            enemyAI.moveSpeed *= multiplier;
        }
        else if (buayaAI != null)
        {
            originalSpeed = buayaAI.patrolSpeed;
            originalChargeSpeed = buayaAI.chargeSpeed;
            buayaAI.patrolSpeed *= multiplier;
            buayaAI.chargeSpeed *= multiplier;
        }
        else if (babiHutanAI != null)
        {
            originalSpeed = babiHutanAI.moveSpeed;
            babiHutanAI.moveSpeed *= multiplier;
        }
        else if (ularAI != null)
        {
            originalSpeed = ularAI.moveSpeed;
            ularAI.moveSpeed *= multiplier;
        }
    }

    private void RestoreSpeed()
    {
        if (playerMovement != null && originalSpeed > 0f)
        {
            playerMovement.moveSpeed = originalSpeed;
            playerMovement.SetInWater(false);
        }
        else if (enemyAI != null && originalSpeed > 0f)
        {
            enemyAI.moveSpeed = originalSpeed;
        }
        else if (buayaAI != null)
        {
            if (originalSpeed > 0f) buayaAI.patrolSpeed = originalSpeed;
            if (originalChargeSpeed > 0f) buayaAI.chargeSpeed = originalChargeSpeed;
        }
        else if (babiHutanAI != null && originalSpeed > 0f)
        {
            babiHutanAI.moveSpeed = originalSpeed;
        }
        else if (ularAI != null && originalSpeed > 0f)
        {
            ularAI.moveSpeed = originalSpeed;
        }
        originalSpeed = -1f;
        originalChargeSpeed = -1f;
    }

    private void OnDisable()
    {
        if (isInWater)
        {
            RestoreSpeed();
            if (sr != null) sr.maskInteraction = SpriteMaskInteraction.None;
            if (waterMask != null) waterMask.enabled = false;
            
            if (hasCapsuleCollider && capsuleCollider != null)
            {
                capsuleCollider.offset = originalCapsuleOffset;
                capsuleCollider.size = originalCapsuleSize;
            }
        }
    }
}

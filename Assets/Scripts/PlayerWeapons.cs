using UnityEngine;
using System.Collections.Generic;

public class PlayerWeapons : MonoBehaviour
{
    [Header("Arrow")]
    public GameObject arrowPrefab;
    public float fireRate = 0.3f;
    public int maxArrows = 30;
    public int currentArrows = 30;

    [Header("Boomerang")]
    public GameObject boomerangPrefab;

    [Header("Bomb")]
    public GameObject bombPrefab;
    public int maxBombs = 10;
    public int currentBombs = 10;

    [Header("Wand")]
    public GameObject fireBoltPrefab;
    public GameObject fireTrailPrefab;
    public float wandFireRate = 0.5f;

    [Header("UI")]
    public ArrowUI arrowUI;
    public BombUI bombUI;

    [Header("Animation")]
    public float shootAnimDuration = 0.3f;

    public enum SubWeapon { Boomerang, Bombs, Grapple, Wand }

    private PlayerController playerController;
    private PlayerMount playerMount;
    private PlayerGrapple playerGrapple;

    private float nextFireTime = 0f;
    private float nextWandFireTime = 0f;
    private bool boomerangOut = false;
    private bool isShooting = false;
    private float shootAnimTimer = 0f;

    private List<SubWeapon> unlockedWeapons = new List<SubWeapon>();
    private int currentWeaponIndex = 0;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerMount = GetComponent<PlayerMount>();
        playerGrapple = GetComponent<PlayerGrapple>();
    }

    void Start()
    {
        // Hide bomb HUD until the player owns the bomb bag
        if (bombUI != null) bombUI.SetVisible(playerController.hasBombs);

        if (PlayerPrefs.HasKey("SavedArrows"))
        {
            currentArrows = PlayerPrefs.GetInt("SavedArrows");
        }
        else
        {
            currentArrows = maxArrows;
        }

        if (PlayerPrefs.HasKey("SavedBombs"))
        {
            currentBombs = PlayerPrefs.GetInt("SavedBombs");
        }
        else
        {
            // Only fill bombs to max if the player owns the bomb bag.
            // Without the bag, currentBombs stays at 0 until UnlockItem("Bombs") sets it.
            currentBombs = playerController.hasBombs ? maxBombs : 0;
        }

        // Load last equipped weapon (saved as enum value, not list index)
        int savedWeaponEnum = PlayerPrefs.GetInt("EquippedWeaponIndex", 0);
        RebuildWeaponList();

        SubWeapon savedWeapon = (SubWeapon)savedWeaponEnum;
        int foundIndex = unlockedWeapons.IndexOf(savedWeapon);
        currentWeaponIndex = foundIndex >= 0 ? foundIndex : 0;

        UpdateArrowUI();
        UpdateBombUI();
    }

    void Update()
    {
        if (DialogueBox.IsActive || ShopUI.IsActive || PauseManager.IsPaused || GameOverUI.IsActive) return;
        if (InputManager.Instance == null) return;

        // Shoot-anim countdown runs even during grapple/mount (matches original PC.Update ordering)
        if (shootAnimTimer > 0f)
        {
            shootAnimTimer -= Time.deltaTime;
            if (shootAnimTimer <= 0f)
            {
                isShooting = false;
            }
        }

        if (playerGrapple.IsGrappling()) return;
        if (playerMount.IsMounted()) return;

        // Arrow - F / Middle Click / RB (hold to rapid fire)
        if (InputManager.Instance.ShootHeld && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        // Active sub-weapon - E / Right Click / Y button
        if (InputManager.Instance.UseSubWeaponPressed)
        {
            UseActiveWeapon();
        }
    }

    // --- ACTIVE ITEM SLOT ---

    public void RebuildWeaponList()
    {
        unlockedWeapons.Clear();

        if (playerController.hasBoomerang) unlockedWeapons.Add(SubWeapon.Boomerang);
        if (playerController.hasBombs) unlockedWeapons.Add(SubWeapon.Bombs);
        if (playerController.hasGrapple) unlockedWeapons.Add(SubWeapon.Grapple);
        if (playerController.hasWand) unlockedWeapons.Add(SubWeapon.Wand);

        if (unlockedWeapons.Count > 0)
        {
            currentWeaponIndex = currentWeaponIndex % unlockedWeapons.Count;
        }
        else
        {
            currentWeaponIndex = 0;
        }
    }

    public void CycleWeapon(int direction)
    {
        if (unlockedWeapons.Count <= 1) return;

        currentWeaponIndex += direction;

        if (currentWeaponIndex >= unlockedWeapons.Count)
        {
            currentWeaponIndex = 0;
        }
        else if (currentWeaponIndex < 0)
        {
            currentWeaponIndex = unlockedWeapons.Count - 1;
        }

        // Save equipped weapon (enum value, not list index — list shifts when weapons unlock)
        PlayerPrefs.SetInt("EquippedWeaponIndex", (int)unlockedWeapons[currentWeaponIndex]);
        PlayerPrefs.Save();

        Debug.Log("Equipped: " + GetActiveWeapon());
    }

    void UseActiveWeapon()
    {
        if (unlockedWeapons.Count == 0) return;

        SubWeapon active = unlockedWeapons[currentWeaponIndex];

        switch (active)
        {
            case SubWeapon.Boomerang:
                if (!boomerangOut) ThrowBoomerang();
                break;

            case SubWeapon.Bombs:
                PlaceBomb();
                break;

            case SubWeapon.Grapple:
                playerGrapple.FireGrapple();
                break;

            case SubWeapon.Wand:
                FireWand();
                break;
        }
    }

    public SubWeapon GetActiveWeapon()
    {
        if (unlockedWeapons.Count == 0) return SubWeapon.Boomerang;
        return unlockedWeapons[currentWeaponIndex];
    }

    public int GetUnlockedWeaponCount()
    {
        return unlockedWeapons.Count;
    }

    public int GetEquippedWeaponIndex()
    {
        return currentWeaponIndex;
    }

    // --- WEAPON METHODS ---

    void Shoot()
    {
        if (arrowPrefab == null) return;
        if (currentArrows <= 0) return;

        currentArrows--;
        UpdateArrowUI();

        Vector2 facing = playerController.GetFacingDirection();
        Vector3 spawnPos = transform.position + (Vector3)(facing * 0.5f);
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        arrow.GetComponent<Arrow>().SetDirection(facing);

        isShooting = true;
        shootAnimTimer = shootAnimDuration;
    }

    void ThrowBoomerang()
    {
        if (boomerangPrefab == null) return;

        boomerangOut = true;

        Vector2 facing = playerController.GetFacingDirection();
        Vector3 spawnPos = transform.position + (Vector3)(facing * 0.5f);
        GameObject boomerang = Instantiate(boomerangPrefab, spawnPos, Quaternion.identity);
        boomerang.GetComponent<Boomerang>().Initialize(transform, facing, this);
    }

    void PlaceBomb()
    {
        if (bombPrefab == null) return;
        if (currentBombs <= 0) return;

        currentBombs--;
        UpdateBombUI();

        Instantiate(bombPrefab, transform.position, Quaternion.identity);
    }

    void FireWand()
    {
        if (fireBoltPrefab == null) return;
        if (Time.time < nextWandFireTime) return;

        nextWandFireTime = Time.time + wandFireRate;

        Vector2 facing = playerController.GetFacingDirection();
        Vector3 spawnPos = transform.position + (Vector3)(facing * 0.5f);
        GameObject bolt = Instantiate(fireBoltPrefab, spawnPos, Quaternion.identity);

        FireBolt fireBolt = bolt.GetComponent<FireBolt>();
        if (fireBolt != null)
        {
            fireBolt.SetDirection(facing);

            // Book upgrade: +1 damage and enable fire trail
            if (playerController.hasBook)
            {
                fireBolt.damage += 1;
                fireBolt.hasFireTrail = true;
                fireBolt.fireTrailPrefab = fireTrailPrefab;
            }
        }
    }

    public void BoomerangReturned()
    {
        boomerangOut = false;
    }

    // --- INVENTORY ---

    public void AddArrows(int amount)
    {
        currentArrows += amount;
        if (currentArrows > maxArrows)
        {
            currentArrows = maxArrows;
        }
        UpdateArrowUI();
    }

    public bool IsAtMaxArrows()
    {
        return currentArrows >= maxArrows;
    }

    public void AddBombs(int amount)
    {
        currentBombs += amount;
        if (currentBombs > maxBombs)
        {
            currentBombs = maxBombs;
        }
        UpdateBombUI();
    }

    public bool IsAtMaxBombs()
    {
        return currentBombs >= maxBombs;
    }

    void UpdateArrowUI()
    {
        if (arrowUI != null)
        {
            arrowUI.UpdateCount(currentArrows);
        }
    }

    void UpdateBombUI()
    {
        if (bombUI != null)
        {
            bombUI.UpdateCount(currentBombs);
        }
    }

    // --- PUBLIC API for PlayerController/ResetActionStates ---

    public bool IsShooting()
    {
        return isShooting;
    }

    public void CancelShooting()
    {
        isShooting = false;
        shootAnimTimer = 0f;
    }

    public void ClearBoomerangLock()
    {
        boomerangOut = false;
    }

    public void OnBombBagUnlocked()
    {
        currentBombs = maxBombs;
        if (bombUI != null) bombUI.SetVisible(true);
        UpdateBombUI();
    }
}

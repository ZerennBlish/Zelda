# Zerenn — Bug History

**Part of the Zerenn Technical Reference.** Canonical record of every audit finding (fixed or deferred), known issues, and lessons learned.

**Audit workflow:** Three auditors per batch (Codex, Claude Code, Gemini) running in parallel on read-only review. Findings consolidated by Opus, triaged P1/P2/P3, fixed by Claude Code in grouped fix prompts.

---

## Audit Cycle 1 — April 2026 (First Full Codebase Sweep)

**Scope:** Full audit of all 70 scripts (~8,300 lines). First audit ever performed on this codebase. Six batches, three auditors per batch (Codex + Claude Code + Gemini), all running read-only and in parallel.

**Result:** 100+ findings across all severity levels. All P1s fixed. Most P2s fixed. P3s mostly deferred. Game compiles clean and plays through correctly after all fix groups landed.

---

### Batch 1 — Player Core

**Files audited:** PlayerController.cs, PlayerHealth.cs, PlayerClass.cs, PlayerAnimator.cs, PlayerShield.cs, PlayerBuff.cs, Melee.cs, InputManager.cs

#### P1 (fixed)

- **Debug keys live in shipped builds.** O/R/T keys bound unconditionally. R wiped all PlayerPrefs (including Unity audio/resolution settings) on a single accidental press. **Fix:** all three keys + handlers wrapped in `#if UNITY_EDITOR`. *Found by all three auditors.*
- **Grapple state softlock on death.** Player dying mid-grapple left collider disabled, rigidbody kinematic, isGrappling=true forever. **Fix:** PlayerController.ResetActionStates() resets grapple state in PlayerHealth.Respawn().
- **Melee state softlock on death and on mount.** isSwinging stuck true if DoSwing coroutine was interrupted. **Fix:** Melee.ResetSwingState() called in both PlayerHealth.Respawn() and PlayerController.Mount().
- **Armor math doc mismatch.** Code halved damage with min 1, comment claimed it eliminated 1-damage hits. **Fix:** comment corrected. Code is correct as-is per design ("armor doesn't grant immunity to weak hits").
- **PlayerBuff stacking corrupts stat restoration.** Second buff captured already-boosted value as "original," leaking permanent boosts on expiration. **Fix:** Initialize removes existing same-type buff before applying. OnDestroy cleanup added.
- **Boomerang state deadlock on destruction.** boomerangOut never cleared if boomerang destroyed via non-catch path. **Fix:** OnDestroy calls BoomerangReturned().
- **Heart upgrade infinite-grind exploit.** Buy → die → DeleteSave wiped HeartUpgradeBought → SaveInventory wrote SavedMaxHealth (still upgraded) → reload → buy again → +1 max HP. Repeat indefinitely. **Fix:** save split — DeleteSave wipes only run state, DeleteAllData (new) wipes everything for new game. HeartUpgradeBought now persists across death.

#### P2 (fixed)

- **PlayerShield reads input without dialogue/shop/pause guard.** Shield sprite toggled during menus. **Fix:** standardized input guard set added.
- **PlayerAnimator reads raw input, no menu guard.** Walk state could flicker during dialogue. **Fix:** standardized input guard set added.
- **Coroutines freeze under timeScale=0.** InvincibilityFrames and DoSwing used WaitForSeconds. **Fix:** changed to WaitForSecondsRealtime.
- **Keyboard sub-weapon binding missing.** UseSubWeapon only bound to right mouse and gamepad Y. **Fix:** Q key added as keyboard binding.
- **EquippedWeaponIndex saved as list index, not enum.** Unlocking new weapons between sessions shifted the list. **Fix:** save/load now uses enum value.
- **PlayerClass.SetClass downgrade double-counts hearts.** **Fix:** SetClass guards against tier <= currentClass (no-op on downgrade).
- **TimeScale persistence on scene load.** GameOver set timeScale=0 with no reset. **Fix:** explicit timeScale=1 in MainMenuController.
- **Two competing GameOver systems.** GameOverUI (saves) and GameOverManager (doesn't save). **Fix:** GameOverManager.cs deleted.
- **ShieldKnight branch duplicated 6 times in projectiles.** **Fix:** ShieldKnight.TakeDamage(int, Vector2) now returns bool; callers gate HitFlash on the return.

#### P2 (deferred to post-audit refactor)

- PlayerController is 656 lines (god class) — split into PlayerMovement / PlayerWeapons / PlayerGrapple / PlayerInventory
- Inline PlayerPrefs.Set bypasses SaveManager.SaveAll — documented as intentional hybrid pattern (bulk save at transitions + incremental save for one-time unlocks)
- Power buff restored obsolete class damage — mitigated by buff dedupe; full fix deferred until weapon system refactor
- Public mutable inventory fields — encapsulation refactor deferred
- Enter/Stay collision duplication — extract HandlePlayerContact helper across 8 enemies + PlayerController mount
- InputManager singleton inconsistent + 4 parallel 19-line lists — InputActionAsset migration is the proper fix

#### P3 (deferred)

Sprite array silent truncation, 20 unguarded Debug.Log calls, PlayerAnimator redundant calcs during pause, integer division armor rounding, SetClass near-dead code, buff blink uses scaled time, shoot animation timer leaks during mount/grapple, Melee.hitbox typed as Collider2D instead of BoxCollider2D, Mount doesn't reset flipX, InvincibilityFrames owns spriteRenderer.enabled, PlayerClass per-tier stats public, UnlockItem uses string keys, half-baked gamepad bindings, ram self-damage undocumented, Melee multi-collider double-hit (low-impact), Melee.FixedUpdate allocates per tick (low-traffic call site), PlayerPrefs loads not validated (no observed corruption).

---

### Batch 2 — Managers, Save System & World

**Files audited:** GameController.cs, GameState.cs, SaveManager.cs, PauseManager.cs, MainMenuController.cs, GameOverManager.cs, GameOverUI.cs, RoomManager.cs, RoomTransition.cs, RoomTracker.cs, Door.cs, BuildingEntrance.cs, SecretTransition.cs, CrackedWall.cs, Destructible.cs, Dropper.cs

#### P1 (fixed)

- **Death + Continue + alt-F4 = lost save.** GameOver wiped HasSave; SaveInventory never restored it. Alt-F4 before next room transition lost the save file. **Fix:** SaveInventory now sets HasSave=1.
- **Double room transition.** Two trigger colliders firing on the same frame applied direction vector twice, teleporting player off the map. **Fix:** isTransitioning guard on RoomManager.ChangeRoom and TeleportToRoom.
- **GameOverUI.SaveInventory fails on disabled player.** FindFirstObjectByType doesn't find disabled GameObjects. **Fix:** FindObjectsInactive.Include parameter added.
- **RoomTracker phantom (0,0).** Marked starting room before RoomManager loaded saved position. **Fix:** MarkVisited moved to RoomManager.Start, called after currentRoom is set.

#### P2 (fixed)

- **Cracked wall state not persisted.** Bombed walls reset on save reload, could softlock player. **Fix:** wallID-based PlayerPrefs persistence (`Wall_<id>`). Each scene wall needs unique ID set in inspector.
- **Destructible double-drop on multi-hit.** Destroy(gameObject) deferred to end of frame; two damage sources triggered TakeDamage twice. **Fix:** isDead flag.
- **SaveManager 5x FindFirstObjectByType per save.** **Fix:** cached references via CacheReferences method.
- **PauseManager no guard against game-over screen.** Player could pause over game-over screen. **Fix:** GameOverUI.IsActive added to PauseManager guard.
- **BuildingEntrance input lingers after dialogue close.** **Fix:** wasDialogueActive flag skips one frame after dialogue closes.
- **Singleton patterns inconsistent.** **Fix:** RoomManager and ShopUI now use null-check + Destroy pattern (matches MinimapUI).
- **SaveManager.SaveGame(int,int,int) dead code.** **Fix:** deleted.
- **BuildingEntrance trigger count breaks with multi-collider player.** **Fix:** root collider check in OnTriggerEnter/Exit2D across all interactables.
- **Dropper null table crash.** **Fix:** null guards on possibleDrops array and selected drop.
- **Door.cs duplicate of RoomTransition.cs.** **Fix:** Door.cs deleted (referenced by zero scenes/prefabs).

#### P3 (deferred)

MainMenuController missing timeScale=1 (fixed during audit), RoomTracker scaling concern (fine for current size), GameState lacks DontDestroyOnLoad (works as-is, documented), CrackedWall/Destructible should implement IDamageable, GameState.rupees public field, RefillEverything bypasses inventory API, PlayerPrefs.Save on every weapon cycle, GameOverUI duplicates save key list (accepted as part of hybrid save pattern).

---

### Batch 3 — NPCs, Dialogue, Shop, Items & UI

**Files audited:** NPC.cs, DialogueBox.cs, DialogueTrigger.cs, ShopKeeper.cs, ShopUI.cs, ItemPickup.cs, Collectible.cs, HeartContainer.cs, GoodAngel.cs, HealthUI.cs, LivesUI.cs, RupeeUI.cs, ArrowUI.cs, BombUI.cs, MinimapUI.cs, HitFlash.cs

#### P1 (fixed)

- **HeartContainer infinite re-collection.** Walked into → +1 max HP → die/reload → walk in again → +1 more, repeat. **Fix:** heartID + PlayerPrefs persistence (`Heart_<id>`).
- **GoodAngel infinite gift.** Same pattern. **Fix:** angelID + PlayerPrefs (`Angel_<id>`).
- **ClassUpgrade pickup respawns AND can downgrade.** **Fix:** pickupID persistence + SetClass guards against downgrade.
- **Shop opens then immediately closes.** Same-frame InteractPressed read by ShopUI after dialogue close fired its callback. **Fix:** ShopUI ignores input on opening frame via Time.frameCount check.
- **First dialogue line skipped (input bleed).** Same root cause. **Fix:** DialogueBox ignores input on opening frame.
- **NPC/DialogueTrigger/ShopKeeper missing PauseManager and GameOverUI guards.** **Fix:** standardized full guard set applied.

#### P2 (fixed)

- **ShopUI raw FindFirstObjectByType (3x per buy).** **Fix:** cached references.
- **DialogueBox/ShopUI singleton patterns inconsistent.** **Fix:** standardized null-check + Destroy pattern.
- **Shop charges before validating delivery.** Could lose rupees buying max-arrow refill or with missing PlayerHealth. **Fix:** validate target/capacity/affordability BEFORE deducting rupees.
- **Full-capacity collectibles consumed for no gain.** **Fix:** capacity check before destroy. Bomb collectible without bomb bag still spawns live bomb (intentional discovery mechanic).
- **ShopKeeper close-shop reopen-dialogue chain.** **Fix:** wasShopActive flag.
- **MinimapUI input not guarded against menus.** **Fix:** standardized guard set.
- **MinimapUI initial refresh race.** **Fix:** RefreshMap removed from Start, called by RoomManager.Start after MarkVisited.
- **Trigger exit hides prompt when weapon hitbox crosses boundary.** **Fix:** root collider check.

#### P3 (deferred)

Six unguarded ItemPickup Debug.Log calls, NPC idle animation runs regardless of distance, RupeeUI/ArrowUI dead icon fields, Collectible reads PlayerController public fields directly, HitFlash captures originalColor at Start (color desync risk), DialogueBox no null check on dialogueText, HeartContainer no audio/particle feedback, three scripts re-implement E-key dialogue trigger logic, Collectibles ignore physics/knockback (bobbing animation acceptable).

---

### Batch 4 — Weapons & Projectiles

**Files audited:** Arrow.cs, Bomb.cs, Boomerang.cs, SwordBeam.cs, SpearBeam.cs, TemplarWave.cs, FireBolt.cs, FireTrail.cs, ExplosionEffect.cs, GrapplingHook.cs, GrapplePoint.cs, MagicProjectile.cs, EnemyArrow.cs, MummyProjectile.cs

#### P1 (fixed)

- **Grapple cleanup leak on early destroy.** Mid-pull destruction left enemy kinematic forever, carried items uncollectible, player stuck in pulling state. **Fix:** GrapplingHook.OnDestroy restores enemy physics, releases carried items, calls GrappleFinished.
- **BoomerangReturned() fires twice on normal catch.** **Fix:** removed explicit call from CatchBoomerang, OnDestroy handles it.
- **Boomerang carried pickups orphaned.** Hearts/rupees with isCarried=true permanently if boomerang destroyed before return. **Fix:** OnDestroy iterates carriedItems and clears isCarried.
- **ExplosionEffect double-damage on multi-collider targets.** **Fix:** OverlapCircle (new API) with HashSet dedupe by root GameObject.
- **FireTrail global cooldown — only one enemy damaged per tick.** **Fix:** per-target Dictionary<Collider2D, float> cooldown.
- **Boomerang not destroyed on room change.** **Fix:** RoomManager.DestroyRoomLocalProjectiles() called in ChangeRoom and TeleportToRoom.
- **boomerangOut not cleared by ResetActionStates.** **Fix:** defensive reset added.
- **MagicProjectile NRE if Rigidbody2D missing.** **Fix:** [RequireComponent(typeof(Rigidbody2D))].

#### P2 (fixed)

- **ShieldKnight stun/pull bypass.** Boomerang and grapple ignored shield direction. **Fix:** ShieldKnight.IsBlockingFrom public method; Boomerang and GrapplingHook check it before stun/pull.
- **Bomb explosion blocked by shield (incorrect).** **Fix:** ExplosionEffect uses no-source TakeDamage overload.
- **GrapplingHook pulls enemies through walls.** Direct transform writes bypassed physics. **Fix:** rb.MovePosition for kinematic-respecting motion.
- **GrapplingHook + Boomerang vs CrackedWall and Destructible.** **Fix per design:** GrapplingHook blocked by CrackedWall (bombs only break them). Boomerang cuts Destructibles for 1 damage and keeps flying (classic Zelda).
- **Beam pierce can re-hit knocked-back enemies.** **Fix:** SpearBeam and TemplarWave use HashSet to track already-hit colliders.
- **HookState.Missed unreachable.** **Fix:** removed from enum.
- **Boomerang inconsistent enemy interaction.** Non-stunnable enemies took no damage. **Fix per design:** non-stunnable enemies take 1 damage from boomerang.
- **HitFlash fires even when ShieldKnight blocks.** **Fix:** TakeDamage(int, Vector2) returns bool, callers gate flash on result.

#### P2 (deferred to post-audit refactor)

Three beams duplicate ~80% of code (PlayerBeam base class).

#### P3 (deferred)

InvokeRepeating with magic string in Bomb, MummyProjectile.speed defaults to 0, GrapplingHook.DestroyHook unused, beams don't null-check spriteRenderer, public mutable fields, file organization inconsistency, GrapplePoint baseScale drift, chainMaterial GC, beam destroy with scaled time, GrapplingHook chain stretches across rooms during transition, Bomb explosion through walls (classic Zelda design — accepted), Bat one-shot brittle special-case, PlayerController mutates FireBolt.damage with +=.

---

### Batch 5 — Enemies Part 1: Simple & Goblins

**Files audited:** Bat.cs, Slime.cs, SlimeSplitter.cs, BoomShroom.cs, GoblinMaceman.cs, GoblinSpearman.cs, GoblinArcher.cs, GoblinThief.cs

#### P1 (fixed)

- **Death paths not idempotent.** Same-frame multi-hit caused double drops, duplicate slime splits, duplicate thief refunds, duplicate BoomShroom explosions. **Fix:** isDead guard pattern across all 8 enemies.
- **GoblinThief escape leaks stolen rupees.** **Fix per design:** thief now despawns on flee timeout. Rupees are permanently lost — "you snooze you lose."
- **BoomShroom double-explode.** No re-entry guard on Explode. **Fix:** hasExploded flag.
- **BoomShroom two overlapping damage paths.** Direct damage AND ExplosionEffect. **Fix:** removed direct damage, configured ExplosionEffect with tuned values like Bomb does.

#### P2 (fixed)

- **Bat moves via transform.position.** Tunneling, broken OnCollisionStay. **Fix:** rb.linearVelocity. *Inspector required: Rigidbody2D on Bat prefab if missing.*
- **GoblinSpearman pullback uses transform.position.** **Fix:** rb.MovePosition.
- **GoblinMaceman spin uses transform.rotation.** **Fix:** rb.MoveRotation.
- **Stun-end overwrites EnemyBuff color tint.** **Fix:** EnemyBuff.ReapplyTint method called after unstun.
- **BoomShroom doesn't explode on walls.** **Fix per design:** explodes on wall and CrackedWall contact.
- **EnemyArrow doesn't recognize CrackedWall.** **Fix per design:** enemy projectiles blocked by cracked walls.

#### P2 (deferred to post-audit refactor)

- No shared base class for stunnable enemies (~80% duplication across 6+ files) — EnemyBase + StunnableEnemy abstract classes
- Slime and SlimeSplitter 95% duplicate (covered by base class refactor)
- Cross-room enemy persistence — room-based enemy disable to save Update budget at scale
- OnCollisionEnter/Stay duplication — extract HandlePlayerContact helper

#### P3 (deferred)

GoblinThief.State.Steal unreachable enum (removed during fix), GoblinThief steals through walls, GoblinArcher arrow spawn clipping, SlimeSplitter nextSize ternary collapses Medium → Small, SlimeSplitter inspector health field overwritten, Bat lacks state machine and contact cooldown, GoblinArcher.Shoot null check, GoblinThief.stolenRupees public in inspector, 8 enemies repeat FindGameObjectWithTag, SlimeSplitter children inherit prefab buff state, GoblinSpearman pullbackStartPos staleness.

---

### Batch 6 — Enemies Part 2: Advanced & Mini-bosses

**Files audited:** SkeletonMage.cs, ShieldKnight.cs, FlyingSkull.cs, Mummy.cs, OrcArcher.cs, OrcChief.cs, EnemyBuff.cs

#### P1 (fixed)

- **Death paths not idempotent.** Same as Batch 5 but applied to advanced enemies. **Fix:** isDead guard on all 6.
- **OrcChief double-die spawns two PlayerBuffs.** Aggravated case — instant Heal/Resupply effect fires twice per double-kill. **Fix:** isDead guard catches it.
- **ShieldKnight permanent pacification on stun-during-cooldown.** attackTimer only decremented in Cooldown state, got stuck above zero. **Fix:** attackTimer reset on stun exit.
- **Mummy executes Update logic when dead.** Could fire one last projectile at 0 HP. **Fix:** isDead guard at top of Update.
- **Six enemies missing buff.ReapplyTint() on unstun.** **Fix:** call added across all 6.

#### P2 (fixed)

- **ShieldKnight BlockFlash overwrites stunColor mid-flash.** Coroutine restored originalColor even if Stun fired during the wait. **Fix:** state check before color restore.
- **Mummy stunned still deals contact damage.** **Fix:** Stunned and Emerging added to OnCollision early-return.
- **SkeletonMage Teleport unvalidated.** Could land in walls. **Fix:** retry loop with wall layer overlap check.
- **Mummy GoUnderground unvalidated.** Same pattern. **Fix:** retry loop.
- **FlyingSkull phases through internal walls.** **Fix:** wall layer overlap check before applying velocity.
- **Mummy Spin uses transform.rotation directly.** **Fix:** rb.MoveRotation, currentRotation modulo 360.
- **OrcChief activeBuffs orphaned on non-Die destruction.** **Fix:** cleanup moved to OnDestroy.
- **EnemyBuff.Initialize lacks dedupe.** **Fix:** defensive dedupe pattern matches PlayerBuff.
- **OrcChief buffs across room boundaries.** **Fix per design:** OverlapBox query bounded to current room only.
- **OrcChief loses buff wave permanently if all enemies pre-buffed.** **Fix:** hasBuffedEnemies only set if at least one buff applied.
- **OrcChief death strips ongoing PlayerBuff.** **Fix per design:** removed pre-removal; PlayerBuff.Initialize handles dedupe so different-type buffs survive.
- **Mummy collider active while invisible during Emerging.** **Fix:** collider gated until scale >= 0.5.
- **FlyingSkull duplicates roomWidth/roomHeight.** **Fix:** reads from RoomManager.

#### P3 (deferred)

Mummy fireRate default 20 shots/sec (tuning), ShieldKnight shield position frozen during stun (cosmetic), ShieldKnight multiple BlockFlash coroutines on rapid attacks, FlyingSkull uses OnTriggerEnter2D (intentional, document), FlyingSkull roomCenter rounding edge case, OrcChief chosenEnemyBuff hardcoded magic number, EnemyBuff.Haste hardcoded 1.5x, SkeletonMage Shoot doesn't pass direction, OrcArcher GetComponent<EnemyArrow> no null-check, Mummy synchronized phase looping (random drift fix).

---

## Known Issues (to fix post-docs)

- Starting bomb count shows 10 instead of intended starting value
- *(others to be added)*

---

## Deferred Architectural Refactors

These are P2-tier findings that are real but require dedicated refactor sessions outside the audit fix flow:

1. **PlayerController split** (656 lines → PlayerMovement + PlayerWeapons + PlayerGrapple + PlayerInventory + thin coordinator)
2. **EnemyBase + StunnableEnemy base classes** — eliminate ~70% duplication across all 14 enemy scripts
3. **Slime/SlimeSplitter merge** — covered by EnemyBase
4. **Three beams base class (PlayerBeam)** — eliminate ~80% duplication in SwordBeam/SpearBeam/TemplarWave
5. **IDirectionalDamageable interface** — eliminate ShieldKnight branch duplication across 6 weapon scripts
6. **Room-based enemy disable** — off-screen rooms keep running enemy AI; disable to save Update budget at scale
7. **InputManager → InputActionAsset migration** — eliminate 19 parallel action declarations across 5 lifecycle methods
8. **Enter/Stay collision dedup** — extract HandlePlayerContact helper across 8 enemies + PlayerController mount

These will compound friction as content scales. Schedule them before adding bosses, dungeons, or major content drops.

---

## Session 02 — May 2026 (MCP Workflow Validation)

**Scope:** First test of Unity MCP scene write capabilities for level building. Sequenced reads and writes through MCP to identify which operations are reliable and which have package limitations.

**Result:** MCP scene writes work, but tool selection matters. Codex routed around the freeze that hung Claude Code by picking a different MCP tool. Once the rule was identified, both tools work cleanly.

#### P1 (mitigated)

- **`Unity_ManageGameObject` freezes Unity on component add via MCP.** Adding a `BoxCollider2D` to a GameObject through `Unity_ManageGameObject` hangs the editor and triggers a Unity assertion failure on `Matrix4x4.GetLossyScale` (`Assertion failed on expression: 'ValidTRS()'`). Stack trace shows Newtonsoft.Json recursively serializing the full Unity object graph for the success response — `SerializeObject → SerializeValue → SerializeObject` looping through Transform → parent → children → components → GameObjects → Transforms infinitely. Reproduced under both Claude Code and Codex when calling `Unity_ManageGameObject`. **Fix:** Route MCP scene writes through `Unity_RunCommand` instead. Validated by Codex completing the same six-step sequence (create GameObject + parent + set localPosition + add BoxCollider2D + set isTrigger + set size) in under nine seconds end-to-end with zero hangs. Rule added to `CLAUDE.md` so CC always picks the correct tool.

#### Notes

- **MCP create's `position` parameter is world-space, not local.** Creating a child GameObject and passing `position: (0, 0, 0)` lands the object at world origin, not at the parent's local origin. Explicitly set `Transform.localPosition` via `component_properties` on the create call, or follow up with a modify call. Documented in `CLAUDE.md`.
- **Component reads via MCP that request full serialized field values also hit the recursion wall.** Asking for "all serialized field values" on a single component (Test 5) reproduced the same Newtonsoft.Json infinite recursion that crashed the editor on `Unity_ManageGameObject` writes. Component name reads work fine; deep field reads do not. Codex's earlier audit (Session 01 followup) hit the same wall on `get_components(Player)` and fell back to reading `.unity` scene YAML and `ProjectSettings/*.asset` files directly.
- **Unity AI cloud endpoint is deprecated.** `https://generators.ai.unity.com` returns `ApiNoLongerSupported`. The `com.unity.ai.assistant` package retries the call regardless of subscription status, including after the trial is canceled. On the desktop, this stacks with MCP traffic and contributes to editor freezes. Workaround: add `0.0.0.0 generators.ai.unity.com` to `C:\Windows\System32\drivers\etc\hosts` so the call fails instantly instead of timing out. Do NOT update `com.unity.ai.assistant` past the current version — 2.7.0 has a separate documented bug that gates MCP behind a paid tier.
- **Test artifacts in scene.** `MCPTest_Empty`, `MCPTest_Codex`, and `MCPTest_CC2` were created as children of `Room_1_0` during validation. Pending cleanup before scene save.

---

## Lessons Learned

**Same-frame input races are the most common bug class.** Pattern: two Updates in one frame both react to the same WasPressedThisFrame input. Solutions used: openFrame check, wasXActive mirror flag, root collider check on triggers. Establish the pattern; new interactables follow it.

**Death path idempotency is the second most common bug class.** Pattern: `Destroy(gameObject)` defers to end of frame, two damage sources call Die() before either Destroy completes, drops/effects/buffs fire twice. Solution: `isDead` guard, set true as first line of Die(). Apply to every enemy and every destructible.

**Singleton drift is silent.** Five different patterns existed across the codebase before audit. Pick one, document it, enforce in code review.

**Save key drift is silent and dangerous.** Every PlayerPrefs key that gets WRITTEN must also get DELETED in the right paths. Centralize the delete logic in SaveManager (DeleteSave for run state, DeleteAllData for new game). Never duplicate the key list across files.

**Lifecycle cleanup goes in OnDestroy, not custom Die methods.** Multiple destruction paths (room change, scene unload, hazard, normal death) must all funnel through a single cleanup point. OnDestroy fires from all of them; custom Die methods miss most.

**Coroutines under timeScale=0 require WaitForSecondsRealtime.** WaitForSeconds freezes when paused. If the coroutine needs to run during pause (UI animations, blink timers), use Realtime. If it needs to pause with the game (gameplay timers), use the regular version.

**Auditors disagree, that's the point.** Codex finds concrete bugs. Claude Code does implementer's-eye structural review. Gemini catches edge cases. Triple coverage means findings are rarely missed; agreement raises confidence.

**The auditor's job is not the implementer's job.** Read-only audits stay read-only. Findings route through Opus (triage) → Claude Code (implement). Every prompt to every auditor includes explicit read-only language. Gemini gets three warnings.


---

### Session 03 — May 2026 (Bomb System Fixes)

**Files touched:** `PlayerController.cs`, `Game.unity`, `Bomb.prefab`

#### P1 (fixed)

- **Bomb prefab root disabled, causing thrown bombs to be invisible and non-functional.** `Instantiate` produced an inactive GameObject so `Bomb.cs` never ran — no visual, no collision, no explosion. Bomb count still decremented correctly because that fires in `PlayerController.PlaceBomb()` before instantiation. Introduced during the Session 01/02 audit. **Fix:** Re-enabled Bomb prefab root (`activeSelf = true`) via `Unity_RunCommand`.

- **BombUI icon not hidden when bombs locked.** `BombUI.SetVisible()` correctly hides both `bombText` and `bombIcon`, but `bombIcon` was null in the Inspector so the image persisted even when `SetVisible(false)` was called. Text hid correctly; icon did not. **Fix:** Assigned `Canvas/BombUI/BombImage` to the `bombIcon` field on the BombUI component via `Unity_RunCommand`.

- **Starting bomb count showed 10 on fresh game.** `PlayerController.Start()` defaulted `currentBombs = maxBombs` when no `SavedBombs` key existed. Bombs are a pickup item — player should start with none. Additionally, `BombUI.SetVisible()` was never called from `Start()` or `UnlockItem()`. **Fix:** Fresh-game fallback now defaults to 0; `SetVisible(hasBombs)` called at end of `Start()`; `SetVisible(true)` called in `UnlockItem("Bombs")`.

---

## Known Issues (logged, not yet fixed)

- **Enemy rotation jank.** Enemies rotate oddly during movement — appears to be a pre-existing issue, not introduced by the Session 03 refactor. Freeze Rotation is confirmed on in Rigidbody2D but behavior is still wrong. Root cause unknown. Low priority until enemy base class refactor is scheduled.

using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

using static R2API.RecalculateStatsAPI;

namespace LostCargoExpansion.Items
{
	internal class SafeguardCell : ItemBase<SafeguardCell> {
		public override string ItemName => "Emergency Hard Light Safeguard Cell";
		public override string ItemLangTokenName => "SAFEGUARD_CELL";
		public override string ItemPickupDesc => "Rapidly generate shield when at critical health.";
		public override string ItemFullDescription =>
			$"Gain {ModifyText.Stylize(StyleID.Healing, baseShield.ToString())} {ModifyText.Stylize(StyleID.Stack, $"(+{stackShield} per stack)")} flat {ModifyText.Stylize(StyleID.Healing, "shield")}. " +
			$"Taking damage to below {ModifyText.Stylize(StyleID.Health, "25% health")} causes you to instantly recharge {ModifyText.Stylize(StyleID.Healing, baseShield.ToString())} {ModifyText.Stylize(StyleID.Stack, $"(+{stackShield} per stack)")} {ModifyText.Stylize(StyleID.Healing, "shield")} " +
			$"and rapidly regenerate any {ModifyText.Stylize(StyleID.Healing, "remaining shield")}. " +
			$"Recharges every {ModifyText.Stylize(StyleID.Utility, $"{baseCooldown} seconds")}.";

		public override string ItemLore =>
			"Order: Emergency Hard Light Safeguard Cell\n" +
			"Tracking Number: 505******\n" +
			"Estimated Delivery: 09/18/2056\n" +
			"Shipping Method: Priority\n" +
			"Shipping Address: [Redacted], [Redacted]\n\n" +

			"'I'm sure you're well aware that in the past few months covert operatives have taken to replacing traditional bullet proof gear with the more lightweight Hard Light based shields. " +
			"The inconspicuousness of said shield generators make them ideal for remaining unsuspected, but in sustained fire they pale in comparison. " +
			"Those shield generators have a tendency to be... unreliable, any decent sized jostle interrupts the recharge process. " +
			"That's where the Emergency Power Cell comes in! We've been testing its effect on shield generators to great success! " +
			"The overflow of power into the generator powers that shield back up near instantaneously! ...just remember that the cell may need time to cooldown as well...'";

		// This item is a healing type item and does something when at low/critical health
		public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing, ItemTag.LowHealth };
		public override ItemTier Tier => ItemTier.Tier2;

		public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
		public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

		public override void Init(ConfigFile config) {
			//CreateConfig(config);
			CreateLang();
			//CreateSound();
			//CreateBuff();
			CreateItem();
			ModifyHooks();
		}

		public override void ModifyHooks() {
			GetStatCoefficients += GrantBaseShield;
			//On.RoR2.Run.Start += PopulateBlacklistedBuffsAndDebuffs;
			//On.RoR2.CharacterBody.FixedUpdate += ForceFeedPotion;
			//RoR2Application.onLoad += OnLoadModCompatability;
			On.RoR2.CharacterBody.OnTakeDamageServer += TriggerShieldGeneration;
			On.RoR2.CharacterMaster.OnBodyDamaged += TriggerShieldGeneration;
			On.RoR2.CharacterBody.Update += ShieldLogic;
		}

		private void GrantBaseShield(CharacterBody sender, StatHookEventArgs args) {
			if (GetCount(sender) > 0) {
				args.baseShieldAdd += baseShield + (stackShield * (GetCount(sender) - 1));
			}
		}

		private void ShieldLogic(On.RoR2.CharacterBody.orig_Update orig, CharacterBody self) {
			orig(self);
			if (self.master.inventory.GetItemCount(ItemDef) <= 0)
				return;

			HealthComponent healthComp = self.healthComponent;
			if (!healthComp.alive)
				return; // Logic should only occur when player is alive

			Chat.AddMessage($"{rechargeStopwatch}"); // Debug timer

			if (rechargeStopwatch > 0) {
				rechargeStopwatch -= Time.fixedDeltaTime;
				if (rechargeStopwatch < 0) {
					rechargeStopwatch = 0;
				}
			}

			/*
			float currentShield = healthComp.shield;
			if () {

			}
			bool flag = currentShield >= self.maxShield;
			if (!flag) {
				currentShield += self.maxShield * 0.5f * Time.fixedDeltaTime;
				if (currentShield > self.maxShield) {
					currentShield = self.maxShield;
				}
			}
			if (currentShield >= self.maxShield && !flag) {
				Util.PlaySound("Play_item_proc_personal_shield_end", healthComp.gameObject);
			}
			if (!currentShield.Equals(healthComp.shield)) {
				healthComp.Networkshield = currentShield;
			}
			*/
		}

		private void TriggerShieldGeneration(On.RoR2.CharacterMaster.orig_OnBodyDamaged orig, CharacterMaster self, DamageReport damageReport) {
			orig(self, damageReport);

			if (GetCount(damageReport.victimBody) > 0 && damageReport.victim.isHealthLow) {
				if (rechargeStopwatch == 0) {
					Chat.AddMessage($"{baseShield + (stackShield * (GetCount(damageReport.victimBody) - 1))} / {damageReport.victim.fullCombinedHealth}");
					damageReport.victim.RechargeShield(baseShield + (stackShield * (GetCount(damageReport.victimBody) - 1)));
					damageReport.victim.ForceShieldRegen();
					rechargeStopwatch = baseCooldown;

					// TODO: Is increasing shield rate possible?
					// TODO: Force regen the shield to full, even if attacked
				}
				//damageReport.victim.AddBarrier(75 * GetCount(damageReport.victimBody));
				//damageReport.victim.RechargeShield((damageReport.victim.fullShield * ShieldPercentage));
				//damageReport.victimBody.netId.Value;
				//damageReport.victimBodyIndex
			}
		}

		private void TriggerShieldGeneration(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport) {
			orig(self, damageReport);

			if (GetCount(damageReport.victimBody) > 0 && damageReport.victim.isHealthLow) {
				if (rechargeStopwatch == 0) {
					Chat.AddMessage($"{baseShield + (stackShield * (GetCount(damageReport.victimBody) - 1))} / {damageReport.victim.fullCombinedHealth}");
					damageReport.victim.RechargeShield(baseShield + (stackShield * (GetCount(damageReport.victimBody) - 1)));
					damageReport.victim.ForceShieldRegen();
					rechargeStopwatch = baseCooldown;

					// TODO: Force regen the shield to full, even if attacked
				}
				//damageReport.victim.AddBarrier(75 * GetCount(damageReport.victimBody));
				//damageReport.victim.RechargeShield((damageReport.victim.fullShield * ShieldPercentage));
			}
		}

		//Add display rules here, or where an item should be visualized on the character.
		public override ItemDisplayRuleDict CreateItemDisplayRules() {
			return new ItemDisplayRuleDict(null);
		}

		private readonly float baseShield = 75f;
		private readonly float stackShield = 75f;

		private readonly float baseCooldown = 30f;
		private float rechargeStopwatch;
	}
}

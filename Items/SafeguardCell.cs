using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using static R2API.RecalculateStatsAPI;
using static RoR2.UI.HGHeaderNavigationController;

namespace LostCargoExpansion.Items
{
	internal class SafeguardCell : ItemBase<SafeguardCell> {
		public override string ItemName => "Emergency Hard Light Safeguard Cell";
		public override string ItemLangTokenName => "SAFEGUARD_CELL";
		public override string ItemPickupDesc => "Generate shield when at critical health.";
		public override string ItemFullDescription =>
			$"Falling below {LostCargoExpansion.Stylize(StyleID.Health, "25% health")} causes you to rapidly generate {LostCargoExpansion.Stylize(StyleID.Healing, $"{shieldPercentage}% of your total shield")} " +
			$"plus an additional {LostCargoExpansion.Stylize(StyleID.Healing, baseShieldBonus.ToString())} {LostCargoExpansion.Stylize(StyleID.Stack, $"(+{stackBonusShield} per stack)")} {LostCargoExpansion.Stylize(StyleID.Healing, "shield")}. " +
			$"Recharges every {LostCargoExpansion.Stylize(StyleID.Utility, "30 seconds")}.";

		public override string ItemLore =>
			"Order: Emergency Hard Light Safeguard Cell\n" +
			"Tracking Number: 505******\n" +
			"Estimated Delivery: 09/18/2056\n" +
			"Shipping Method: Priority\n" +
			"Shipping Address: [Redacted], [Redacted]\n\n" +

			"'I'm sure you're well aware that in the past few months covert operatives have taken to replacing traditional bullet proof gear with the more lightweight Hard Light based shields." +
			"The inconspicuousness of said shield generators make them ideal for remaining unsuspected, but in sustained fire they pale in comparison." +
			"Those shield generators have a tendency to be... unreliable, any decent sized jostle interrupts the recharge process." +
			"That's where the Emergency Power Cell comes in! We've been testing its effect on shield generators to great success!" +
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
			//On.RoR2.Run.Start += PopulateBlacklistedBuffsAndDebuffs;
			//On.RoR2.CharacterBody.FixedUpdate += ForceFeedPotion;
			//RoR2Application.onLoad += OnLoadModCompatability;

			//On.RoR2.CharacterBody.OnTakeDamageServer += TriggerShieldGeneration;
			//On.RoR2.CharacterMaster.OnBodyDamaged += TriggerShieldGeneration;
			On.RoR2.CharacterBody.Update += ShieldLogic;
		}

		private void ShieldLogic(On.RoR2.CharacterBody.orig_Update orig, CharacterBody self) {
			orig(self);
			if (self.master.inventory.GetItemCount(ItemDef) <= 0)
				return;

			HealthComponent healthComp = self.healthComponent;
			if (!healthComp.alive)
				return; // Logic should only occur when player is alive

			Chat.AddMessage($"{rechargeStopwatch}");

			if (rechargeStopwatch > 0) {
				rechargeStopwatch -= Time.fixedDeltaTime;
				if (rechargeStopwatch < 0) {
					rechargeStopwatch = 0;
				}
			}

			if (rechargeStopwatch == 0 && triggerEffect) {
				healthComp.ForceShieldRegen();
				rechargeStopwatch = baseCooldown;

				// TODO: Force regen the shield to full, even if attacked
			}

			triggerEffect = false;

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
				triggerEffect = true;
				//damageReport.victim.AddBarrier(75 * GetCount(damageReport.victimBody));
				//damageReport.victim.RechargeShield((damageReport.victim.fullShield * ShieldPercentage));
				//damageReport.victimBody.netId.Value;
				//damageReport.victimBodyIndex
			}
		}

		private void TriggerShieldGeneration(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport) {
			orig(self, damageReport);

			if (GetCount(damageReport.victimBody) > 0 && damageReport.victim.isHealthLow) {
				triggerEffect = true;
				//damageReport.victim.AddBarrier(75 * GetCount(damageReport.victimBody));
				//damageReport.victim.RechargeShield((damageReport.victim.fullShield * ShieldPercentage));
			}
		}

		//Add display rules here, or where an item should be visualized on the character.
		public override ItemDisplayRuleDict CreateItemDisplayRules() {
			return new ItemDisplayRuleDict(null);
		}

		private readonly int shieldPercentage = 50;
		private readonly int baseShieldBonus = 75;
		private readonly int stackBonusShield = 75;

		private readonly float baseCooldown = 30f;
		private float rechargeStopwatch;

		private bool triggerEffect;
	}
}

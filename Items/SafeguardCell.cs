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
			$"Taking damage to below {ModifyText.Stylize(StyleID.Health, "25% health")} causes you to gain and instantly regenerate {ModifyText.Stylize(StyleID.Healing, baseShield.ToString())} {ModifyText.Stylize(StyleID.Stack, $"(+{stackShield} per stack)")} {ModifyText.Stylize(StyleID.Healing, "bonus shield")}. " +
			$"The rest of your shield will also regenerate without disruption. " +
			$"Bonus shield will be lost after 7 seconds." +
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

		public static BuffDef SafeguardCellBuff;
		public static BuffDef ForceShieldBuff;

		public override void Init(ConfigFile config) {
			//CreateConfig(config);
			CreateLang();
			//CreateSound();
			CreateBuff();
			CreateItem();
			ModifyHooks();
		}

		public override void ModifyHooks() {
			GetStatCoefficients += AddBuffShield; // Adds additional shield while buff is active
			On.RoR2.CharacterBody.OnTakeDamageServer += TriggerShieldGeneration;
			On.RoR2.CharacterMaster.OnBodyDamaged += TriggerShieldGeneration;
			On.RoR2.CharacterBody.FixedUpdate += ShieldLogic;
		}

		private void AddBuffShield(CharacterBody sender, StatHookEventArgs args) {
			if (GetCount(sender) > 0 && sender.HasBuff(SafeguardCellBuff)) {
				args.baseShieldAdd += baseShield + (stackShield * (GetCount(sender) - 1));
			}
		}

		private void ShieldLogic(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
			orig(self);

			if (self.master.inventory.GetItemCount(ItemDef) <= 0)
				return;

			if (!self.healthComponent.alive)
				return; // Logic should only occur when player is alive

			Chat.AddMessage($"{rechargeStopwatch}"); // Debug timer

			if (rechargeStopwatch > 0) {
				rechargeStopwatch -= Time.fixedDeltaTime;
				if (rechargeStopwatch < 0) {
					rechargeStopwatch = 0;
				}
			}
		}

		private void TriggerShieldGeneration(On.RoR2.CharacterMaster.orig_OnBodyDamaged orig, CharacterMaster self, DamageReport damageReport) {
			orig(self, damageReport);

			// When damaged, if the user has the buff...
			if (damageReport.victimBody.HasBuff(ForceShieldBuff)) {
				damageReport.victimBody.healthComponent.ForceShieldRegen(); // force the shield to continue recharging
			}

			// When damaged, if the user has an item and is brought to critical health with the item cooldown at 0...
			if (GetCount(damageReport.victimBody) > 0 && rechargeStopwatch == 0 && damageReport.victim.isHealthLow) {
				Chat.AddMessage($"{baseShield + (stackShield * (GetCount(damageReport.victimBody) - 1))} / {damageReport.victim.fullCombinedHealth}"); // debug
				damageReport.victimBody.AddTimedBuff(SafeguardCellBuff, 7f); // Add the buff that gives bonus shield for 7 seconds
				damageReport.victimBody.AddTimedBuff(ForceShieldBuff, 2f); // Add the buff that forces shield regeneration for 2 seconds
				rechargeStopwatch = baseCooldown; // Set the cooldown back to 30 seconds
			}
		}

		private void TriggerShieldGeneration(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport) {
			orig(self, damageReport);

			// When damaged, if the user has the buff...
			if (damageReport.victimBody.HasBuff(ForceShieldBuff)) {
				damageReport.victimBody.healthComponent.ForceShieldRegen(); // force the shield to continue recharging
			}

			// When damaged, if the user has an item and is brought to critical health with the item cooldown at 0...
			if (GetCount(damageReport.victimBody) > 0 && rechargeStopwatch == 0 && damageReport.victim.isHealthLow) {
				Chat.AddMessage($"{baseShield + (stackShield * (GetCount(damageReport.victimBody) - 1))} / {damageReport.victim.fullCombinedHealth}"); // debug
				damageReport.victimBody.AddTimedBuff(SafeguardCellBuff, 7f); // Add the buff that gives bonus shield for 7 seconds
				damageReport.victimBody.AddTimedBuff(ForceShieldBuff, 2f); // Add the buff that forces shield regeneration for 2 seconds
				rechargeStopwatch = baseCooldown; // Set the cooldown back to 30 seconds
			}
		}

		private void CreateBuff() {
			SafeguardCellBuff = ScriptableObject.CreateInstance<BuffDef>();
			SafeguardCellBuff.name = "Lost Cargo: Cell Shield Bonus";
			SafeguardCellBuff.buffColor = new Color(195, 61, 100, 255);
			SafeguardCellBuff.canStack = false;
			SafeguardCellBuff.isDebuff = false;
			SafeguardCellBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
			ContentAddition.AddBuffDef(SafeguardCellBuff);

			ForceShieldBuff = ScriptableObject.CreateInstance<BuffDef>();
			ForceShieldBuff.name = "Lost Cargo: Forced Shield Recharge";
			ForceShieldBuff.buffColor = new Color(195, 61, 100, 255);
			ForceShieldBuff.canStack = false;
			ForceShieldBuff.isDebuff = false;
			ForceShieldBuff.isHidden = true;
			ForceShieldBuff.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
			ContentAddition.AddBuffDef(ForceShieldBuff);
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

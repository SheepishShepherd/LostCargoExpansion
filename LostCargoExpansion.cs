using BepInEx;
using LostCargoExpansion.Items;
using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LostCargoExpansion
{
	//This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
	//It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

	//This attribute specifies that we have a dependency on R2API, as we're using it to add our item to the game.
	//You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
	[BepInDependency(R2API.R2API.PluginGUID)]

	//This attribute is required, and lists metadata for your plugin.
	[BepInPlugin(PluginGUID, PluginName, PluginVersion)]

	// Requires everyone in a Lobby to have the same version
	[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

	//We will be using 2 modules from R2API: ItemAPI to add our item and LanguageAPI to add our language tokens.
	[R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(RecalculateStatsAPI))]

	//[BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
	//[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

	//This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
	//BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
	public class LostCargoExpansion : BaseUnityPlugin
	{
		//The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
		//If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
		public const string PluginGUID = PluginAuthor + "." + PluginName;
		public const string PluginAuthor = "SheepishShepherd";
		public const string PluginName = "TestMod";
		public const string PluginVersion = "1.0.0";

		public static AssetBundle MainAssets;
		public List<ItemBase> Items = new List<ItemBase>();

		//We need our item definition to persist through our functions, and therefore make it a class field.

		//The Awake() method is run at the very start when the game is initialized.
		public void Awake() {
			//Init our logging class so that we can properly log for debugging
			Log.Init(Logger);

			/*
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MyCoolModsNamespaceHere.mycoolmod_assets")) {
				MainAssets = AssetBundle.LoadFromStream(stream);
			}
			*/

			// This will initialize all items into the game.
			var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));
			foreach (var itemType in ItemTypes) {
				ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
				Items.Add(item);
				item.Init(Config);
				Log.LogInfo("Item: " + item.ItemName + " Initialized!");
			}

			//IL.RoR2.ShopTerminalBehavior.GenerateNewPickupServer_bool += ItemBase.BlacklistFromPrinter;
			//On.RoR2.Items.ContagiousItemManager.Init += ItemBase.RegisterVoidPairings;

			// This line of log will appear in the bepinex console when the Awake method is done.
			Log.LogInfo(nameof(Awake) + " done.");
		}

		//The Update() method is run on every frame of the game.
		private void Update() {
			//This if statement checks if the player has currently pressed F2.
			if (Input.GetKeyDown(KeyCode.F2)) {
				//Get the player body to use a position:
				var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

				//And then drop our defined item in front of the player.

				Log.LogInfo($"Player pressed F2. Spawning our custom item at coordinates {transform.position}");
				PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(SafeguardCell.instance.ItemDef.itemIndex), transform.position, transform.forward * 20f);
				PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.Items.ShieldOnly.itemIndex), transform.position, transform.forward * 20f);
				PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.Items.PersonalShield.itemIndex), transform.position, transform.forward * 20f);
			}
		}
	}
}

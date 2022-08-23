
namespace LostCargoExpansion
{
	internal class ModifyText {
		internal static string Stylize(string type, string text) => $"<style={type}>{text}</style>";
	}

	internal class StyleID
	{
		internal const string Damage = "cIsDamage";
		internal const string Healing = "cIsHealing";
		internal const string Utility = "cIsUtility";
		internal const string Health = "cIsHealth";
		internal const string Stack = "cStack";
		internal const string Mono = "cMono";
		internal const string Death = "cDeath";
		internal const string UserSettings = "cUserSetting";
		internal const string Artifact = "cArtifact";
		internal const string Sub = "cSub";
		internal const string Event = "cEvent";
		internal const string WorldEvent = "cWorldEvent";
		internal const string KeywordName = "cKeywordName";
		internal const string Shrine = "cShrine";
	}
}

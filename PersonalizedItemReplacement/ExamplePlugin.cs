using BepInEx;
using RiskOfOptions;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BepInEx.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace PersonalizedItemReplacement
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class PersonalizedItemReplacement : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "GiGaGon";
        public const string PluginName = "PersonalizedItemReplacement";
        public const string PluginVersion = "1.0.0";

        public static ConfigFile PIP_cfg = new(Paths.ConfigPath + "\\GiGaGon.PersonalizedItemReplacement.cfg", true);

        internal class ModConfig
        {
            public static ConfigEntry<float> e_percent;

            public static void InitConfig(ConfigFile config)
            {
                e_percent = config.Bind("General", "Chance per Eulogy Zero stack", 0.05f, "Chance of item replacement per Eulogy Zero stack. Supports in-game modifications.");
                ModSettingsManager.AddOption(new RiskOfOptions.Options.StepSliderOption(e_percent, new RiskOfOptions.OptionConfigs.StepSliderConfig() { min = 0, max = 1, increment = 0.01f}));
            }
        }
        public void Awake()
        {
            ModConfig.InitConfig(PIP_cfg);

            On.RoR2.Items.RandomlyLunarUtils.CanReplace += (orig, pickupDef) =>
            {
                return false;
            };

            On.RoR2.Inventory.GiveItem_ItemIndex_int += (orig, self, itemIndex, count) =>
            {
                CharacterBody body = CharacterBody.readOnlyInstancesList.ToList().Find(x => x.inventory == self);
                int e_stacks = self.GetItemCount(DLC1Content.Items.RandomlyLunar);
                if (body != null && e_stacks > 0 && ItemCatalog.GetItemDef(itemIndex).tier != ItemTier.Lunar)
                {
                    if (Run.instance != null)
                    {
                        float a = Run.instance.runRNG.nextNormalizedFloat;
                        float b = ModConfig.e_percent.Value * e_stacks;
                        if (a < b)
                        {
                            List<PickupIndex> d_table = Run.instance.availableLunarCombinedDropList;
                            PickupDropletController.CreatePickupDroplet(d_table[Run.instance.runRNG.RangeInt(0, d_table.Count)], body.transform.position, new Vector3(0, 0, 0));
                        }
                        else
                        {
                            orig(self, itemIndex, count);
                        }
                    }
                } 
                else
                {
                    orig(self, itemIndex, count);
                }
            };
        }
    }
}

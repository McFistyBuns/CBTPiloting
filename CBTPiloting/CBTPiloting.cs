using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS.DebugConsole;
using HBS.Logging;
using UnityEngine;
using Newtonsoft.Json;
using JetBrains.Annotations;

namespace CBTPiloting
{
    [HarmonyPatch(typeof(Mech), "ResolveWeaponDamage", new Type[] { typeof(WeaponHitInfo), typeof(Weapon), typeof(MeleeAttackType) })]
    public static class Mech_ResolveWeaponDamage_Patch
    {
        private static void Postfix(Mech __instance, WeaponHitInfo hitInfo, Weapon weapon, MeleeAttackType meleeAttackType)
        {
            AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
            AbstractActor actor = __instance.Combat.FindActorByGUID(hitInfo.targetId);

            if (actor is Mech)
            {
                Mech target = actor as Mech;

                float stabilityDamage = hitInfo.ConsolidateInstability(weapon.Instability(), __instance.Combat.Constants.ResolutionConstants.GlancingBlowDamageMultiplier, __instance.Combat.Constants.ResolutionConstants.NormalBlowDamageMultiplier, __instance.Combat.Constants.ResolutionConstants.SolidBlowDamageMultiplier);
                stabilityDamage *= __instance.StatCollection.GetValue<float>("ReceivedInstabilityMultiplier");
                stabilityDamage *= __instance.EntrenchedMultiplier;

                if (AttackDirector.attackLogger.IsLogEnabled)
                {
                    AttackDirector.attackLogger.Log("[CBTPiloting] Checking Piloting Stability");
                    AttackDirector.attackLogger.Log(string.Format("[CBTPiloting] Is Mech: {0}", (actor is Mech)));
                    AttackDirector.attackLogger.Log(string.Format("[CBTPiloting] Weapon Stab Dmg: {0}", stabilityDamage));
                    AttackDirector.attackLogger.Log(string.Format("[CBTPiloting] Target Dead: {0}", target.IsDead));
                    AttackDirector.attackLogger.Log(string.Format("[CBTPiloting] Target Unsteady: {0}", target.IsUnsteady));
                    AttackDirector.attackLogger.Log(string.Format("[CBTPiloting] Target IsOrWillBeProne: {0}", target.IsOrWillBeProne));
                }

                if (stabilityDamage > 0 && !target.IsDead && target.IsUnsteady && !target.IsOrWillBeProne)
                {
                    float skillBonus = (float)target.SkillPiloting / __instance.Combat.Constants.PilotingConstants.PilotingDivisor;

                    float skillRoll = __instance.Combat.NetworkRandom.Float();
                    float skillTotal = skillRoll + skillBonus;

                    if (AttackDirector.attackLogger.IsLogEnabled)
                    {
                        AttackDirector.attackLogger.Log(string.Format("[CBTPiloting] Skill Bonus: {0}", skillBonus));
                        AttackDirector.attackLogger.Log(string.Format("[CBTPiloting] Skill Roll: {0}", skillRoll));
                        AttackDirector.attackLogger.Log(string.Format("[CBTPiloting] Skill Roll Total: {0}", skillTotal));
                        AttackDirector.attackLogger.Log(string.Format("[CBTPiloting] Skill Target: {0}", CBTPiloting.Settings.PilotStabilityCheck));
                    }

                    if (skillTotal < CBTPiloting.Settings.PilotStabilityCheck)
                    {
                        if (AttackDirector.attackLogger.IsLogEnabled)
                        {
                            AttackDirector.attackLogger.Log(string.Format("[CBTPiloting] Skill Check Failed! Flagging for Knockdown"));
                        }

                        target.FlagForKnockdown();
                        target.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(target, $"Stability Check: Failed!", FloatieMessage.MessageNature.Debuff, true)));
                    }
                    else
                    {
                        if (AttackDirector.attackLogger.IsLogEnabled)
                        {
                            AttackDirector.attackLogger.Log(string.Format("[CBTPiloting] Skill Check Succeeded!"));
                        }

                        target.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(target, $"Stability Check: Passed!", FloatieMessage.MessageNature.Buff, true)));
                    }
                }
            }
        }
    }

    internal class ModSettings
    {
        [JsonProperty("PilotStabilityCheck")]
        public float PilotStabilityCheck { get; set; }
    }

    public static class CBTPiloting
    {
        internal static ModSettings Settings;

        public static void Init(string modDir, string modSettings)
        {
            var harmony = HarmonyInstance.Create("io.github.guetler.CBTPiloting");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            try
            {
                Settings = JsonConvert.DeserializeObject<ModSettings>(modSettings);
            }
            catch (Exception)
            {
                Settings = new ModSettings();
            }
        }
    }
}
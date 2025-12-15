using System;
using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;

namespace CloseConfirm
{
    public class CloseConfirm : ResoniteMod
    {
        public override string Name => "CloseConfirm";
        public override string Author => "AlexW-578";
        public override string Version => "1.2.0";
        public override string Link => "https://github.com/AlexW-578/CloseConfirm/";
        private static ModConfiguration Config;

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> Enabled =
            new ModConfigurationKey<bool>("Enabled", "Enable/Disable the Mod", () => true);
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> AllowCloseWhenDashExitOpen =
            new ModConfigurationKey<bool>("Exit immediately when exit page already open", "Allows the game to exit as standard if the exit page is already open. (i.e. if you click the X button twice)", () => false);

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> CatchEmergencyKeyBinds=
            new ModConfigurationKey<bool>("Also catch emergency keybinds when LocalHome", "Also catch the emergency keybinds when in local home (Could be dangerous)", () => false);
        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> ManualClose =
            new ModConfigurationKey<bool>("ManualClose", "ManualClose", () => false, true);

        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            if (Config.GetValue(ManualClose))
            {
                Config.Set(ManualClose, false);
            }
            Config.Save(true);
            Harmony harmony = new Harmony("co.uk.AlexW-578.CloseConfirm");
            harmony.PatchAll();
        }
        
        [HarmonyPatch(typeof(AppEnder), "OnAttach")]
        class Confirm_Patch
        {
            public static void Postfix(AppEnder __instance)
            {
                Config.Set(ManualClose,true);
            }
        }
        
        [HarmonyPatch(typeof(Engine), nameof(Engine.RequestShutdown))]
        class Shutdown_Patch
        {
            public static bool Prefix(Engine __instance)
            {
                if (!Config.GetValue(Enabled))
                {
                    return true;
                }
                if (Config.GetValue(ManualClose))
                {
                    Warn("Manual Close Detected - Closing Game.");
                    return true;
                }
                return CloseCatch();
            }
        }

        [HarmonyPatch(typeof(Userspace), nameof(Userspace.ExitApp))]
        class Emergency_Patch
        {
            public static bool Prefix(Engine __instance)
            {
                Config.Set(ManualClose,true);
            }
        }
        

                if (!Config.GetValue(CatchEmergencyKeyBinds))
                {
                    return true;
                }

                return CloseCatch();
            }
            
        }
        private static bool CloseCatch()
        {
            UserspaceRadiantDash userspaceRadiantDash = Userspace.UserspaceWorld.GetRadiantDash();
            SyncRef<RadiantDash> dash = (SyncRef<RadiantDash>) userspaceRadiantDash.GetSyncMember(5);
            ExitScreen exit = dash.Target.GetScreen<ExitScreen>();
            if (
                dash.Target.Open.Value 
                && dash.Target.CurrentScreen.Target == exit 
                && Config.GetValue(AllowCloseWhenDashExitOpen)
            ) {
                Warn("Caught close, but the dash is already open, ignoring.");
                return true;
            }
            userspaceRadiantDash.StartTask(async ()=>
            {
                await new NextUpdate();
                dash.Target.Open.Value = true;
                dash.Target.CurrentScreen.Target = exit;
                await new NextUpdate();
                Warn("Caught emergency keybind and prevented close.");
            });
            return false;
        }
    }
}
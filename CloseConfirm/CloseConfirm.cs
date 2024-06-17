using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;

namespace CloseConfirm
{
    public class CloseConfirm : ResoniteMod
    {
        public override string Name => "CloseConfirm";
        public override string Author => "AlexW-578";
        public override string Version => "1.1.0";
        public override string Link => "https://github.com/AlexW-578/CloseConfirm/";
        private static ModConfiguration Config;

        [AutoRegisterConfigKey] private static readonly ModConfigurationKey<bool> Enabled =
            new ModConfigurationKey<bool>("Enabled", "Enable/Disable the Mod", () => true);

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

        [HarmonyPatch(typeof(Engine), "RequestShutdown")]
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
                UserspaceRadiantDash userspaceRadiantDash = Userspace.UserspaceWorld.GetRadiantDash();
                userspaceRadiantDash.StartTask(async ()=>
                {
                    await new NextUpdate();
                    SyncRef<RadiantDash> dash = (SyncRef<RadiantDash>) userspaceRadiantDash.GetSyncMember(5);
                    dash.Target.Open.Value = true;
                    ExitScreen exit = dash.Target.GetScreen<ExitScreen>();
                    dash.Target.CurrentScreen.Target = exit;
                    await new NextUpdate();
                    Warn("Caught and prevented close.");
                });
                return false;
            }
        }

        [HarmonyPatch(typeof(AppEnder), "OnAttach")]
        class Confirm_Patch
        {
            public static void Postfix(AppEnder __instance)
            {
                Config.Set(ManualClose,true);
            }
        }
        

    }
}
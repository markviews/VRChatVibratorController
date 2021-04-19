using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;

namespace Vibrator_Controller.XrefScanning
{
    class Main
    {
        private static MethodInfo immobilizePlayer;
        public static void Initialize()
        {
            try
            {
                immobilizePlayer = typeof(VRCTrackingManager).GetMethods()
                    .First(mb => mb.Name.StartsWith("Method_Public_Static_Void_Boolean_")
                    && CheckUsed(mb, "Method_Public_Void_String_String_WorldTransitionInfo_Action_1_String_Boolean_0")
                    && CheckUsed(mb, "Method_Public_Static_Void_VRC_AnimatorTemporaryPoseSpace_Animator_PDM_0"));
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error occured in immobilizePlayer scan:\n{e}");
            }
        }

        public static void ImmobilizePlayer(bool active)
        {
            try
            {
                immobilizePlayer.Invoke(VRCTrackingManager.field_Private_Static_VRCTrackingManager_0, new object[1] { active });
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error occured in ImmobilizePlayer Invoke:\n{e}");
            }
        }

        private static bool CheckUsed(MethodBase methodBase, string methodName)
        {
            if (!string.IsNullOrEmpty(methodName))
            {
                try
                {
                    return XrefScanner.UsedBy(methodBase)
                        .Where(instance => instance.TryResolve() != null && instance.TryResolve().Name.Contains(methodName)).Any();
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Failed to Xref CheckUsed:\n{e}");
                }
            }
            return false;
        }
    }
}

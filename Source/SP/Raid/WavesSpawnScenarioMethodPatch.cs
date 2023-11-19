//using EFT;
//using SIT.Core.Misc;
//using SIT.Tarkov.Core;
//using System.Reflection;

//namespace SIT.Core.SP.Raid
//{
//    internal class WavesSpawnScenarioMethodPatch : ModulePatch
//    {
//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetMethodForType(typeof(EFT.WavesSpawnScenario), "method_0");
//        }

//        [PatchPrefix]
//        public static bool PrePatch(WildSpawnWave[] waves, WildSpawnType type)
//        {
//            return false;
//        }


//    }
//}

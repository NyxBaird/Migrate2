using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Nautilus.Commands;

//Diel Vertical Migration, The Mod
namespace Migrate2
{
    [BepInDependency("com.snmodding.nautilus")]
    [BepInPlugin(Globals.MyGuid, Globals.PluginName, Globals.VersionString)]
    public class Migrate2 : BaseUnityPlugin
    {
        public Globals Globals;
        public Helpers Helpers;
        
        private static readonly Harmony Harmony = new Harmony(Globals.MyGuid);

        /// <summary>
        /// Initialise the configuration settings and patch methods
        /// </summary>
        private void Awake()
        {
            Globals = new Globals();
            Globals.Instance.Log = Logger;
            
            Helpers = new Helpers();
            
            Logger.LogInfo($"PluginName: {Globals.PluginName}, VersionString: {Globals.VersionString} is loading...");
            Harmony.PatchAll();
            Logger.LogInfo($"PluginName: {Globals.PluginName}, VersionString: {Globals.VersionString} is loaded.");
        }
    }
    
    
    // Each migratory species should be initialized as a Migrator
    [HarmonyPatch(typeof(Creature))]
    [HarmonyPatch("UpdateBehaviour")]
    public class PatchCreatureBehaviour
    {
        private static Dictionary<Creature, float> lastUpdateTime = new Dictionary<Creature, float>();

        [HarmonyPostfix]
        public static void Postfix(Creature __instance)
        {
            //Ensure we only update the migratory route every x seconds
            var currentTime = UnityEngine.Time.time;
            if (lastUpdateTime.TryGetValue(__instance, out var lastTime))
            {
                if (currentTime - lastTime < 5f) // Less than 5 seconds have passed
                    return; // Do not run the behaviour again yet
            }
            
            var globals = Globals.Instance;
            if (__instance.GetBestAction() is null) 
                return;
                
            var action = __instance.GetBestAction().ToString().Split('(').Last().Replace(")", "");
            var name = __instance.name.Replace("(Clone)", "");
    
            string[] replaceableActions = { "SwimRandom" };
            string[] activeBiomes = { "kooshZone", "mountains", "grandReef", "seaTreaderPath", "dunes", "bloodKelp", "GrassyPlateaus", "SparseReef", "kelpForest" };
    
            //If our creature is a valid migrator and is performing a replaceable action and is in an active biome, process our creature for migration
            if (globals.Migrators.ContainsKey(name) && Array.IndexOf(replaceableActions, action) > -1 &&
                Array.IndexOf(activeBiomes, WorldPatch.World.GetBiome(__instance.transform.position).ToString()) > -1)
            {
                globals.Migrators[name].Migrate(__instance);
                
                // Update the last run time for this creature
                lastUpdateTime[__instance] = currentTime;
            }
            // if (globals.Migrators.ContainsKey(name))
            //     globals.Log.LogDebug(__instance + " is performing " + action);
            
        }
    }
    

    //Handle all our time/light related details
    [HarmonyPatch(typeof(DayNightCycle))] 
    [HarmonyPatch("Update")]
    public class Time
    {
        public static float OfDay;
        public static float LightScalar;
        
        [HarmonyPostfix]
        public static void Postfix(DayNightCycle __instance)
        {
            var globals = Globals.Instance;
            
            LightScalar = __instance.GetLocalLightScalar();            
            OfDay = __instance.GetDayScalar();
            
            // globals.Log.LogDebug("Light: " + LightScalar + " | Local Light: " + LightScalar);
        }
    
        //is eclipse if lightscalar dips below 5 between dayscalar of .15 and .85 
        public bool IsEclipse()
        {
            return (LightScalar < 5 && (OfDay > .15 && OfDay < .85));
        }
    }


    [HarmonyPatch(typeof(LargeWorld))]
    [HarmonyPatch("Awake")]
    public class WorldPatch
    {
        public static LargeWorld World;
        
        [HarmonyPostfix]
        public static void Postfix(LargeWorld __instance)
        {
            World = __instance;
        }
    }
    
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Awake")]
    public class PlayerPatch
    {
        public static Player Player;
        
        [HarmonyPostfix]
        public static void Postfix(Player __instance)
        {
            Player = __instance;
        }
    }
    
    public class Commands
    {
        
        [ConsoleCommand("GetBiome")]
        public static string MigrateCmd()
        {
            return $"Biome = " + WorldPatch.World.GetBiome(PlayerPatch.Player.transform.position);
        }
        
        [ConsoleCommand("playerSpeed")]
        public static string PlayerSpeedCmd(int speed = 1)
        {
            PlayerPatch.Player.movementSpeed = speed;
            
            return $"Parameters: {speed}";
        }
        
        [ConsoleCommand("margins")]
        public static string MarginsCmd()
        {
            return $"" + Globals.Margins[0] + " | " + Globals.Margins[1];
        }
    }
}
using System.Collections.Generic;
using BepInEx.Logging;

namespace Migrate2
{
    public class Globals
    {
        public bool DeveloperMode = true;
        
        private static Globals _instance;
        public static Globals Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new Globals();
                return _instance;
            }
        }
        
        public const string MyGuid = "me.nyxb.migrate";
        public const string PluginName = "Migrate2";
        public const string VersionString = "2.0";

        public Helpers Helpers;
        
        public ManualLogSource Log { get; set; }

        public enum FoodChainStatus
        {
            Predator, //Run
            Prey, //Hunt
            None //This is for creatures outside the normal foodchain, for example Reefback Leviathans
        };

        public enum MigrationTypes
        {
            //In all cases predators follow the prey
            DielNocturnal, // - Prey try to maintain constant DARKNESS throughout the day/night
            DielReverse, // - Prey try to maintain constant LIGHT throughout the day/night
            DielTwilight, // - Prey go to the surface for sunrise and sunset and sink back down shortly after
            Ontogenetic // - Creatures relocate through the water column as they live their lives (Not currently implemented)
        };

        /*
         * Creatures that migrate
         */
        public IDictionary<string, Migrator> Migrators = new Dictionary<string, Migrator>();

        /*
         * The vertical paths followed by migrating Creatures
         */
        public IDictionary<MigrationTypes, SortedList<double, double>> Traversals =
            new Dictionary<MigrationTypes, SortedList<double, double>>();

        /*
         * MigratorSizeExtremes = {smallest, largest}
         * -Initialize these with values between the known smallest and largest creature and they'll be automatically worked out
         */
        public double SmallestMigratorSize = 50;
        public double LargestMigratorSize = 50;

        /*
         * Margins = {top, bottom}
         * The amount of space preserved at the top and bottom of the water column to which creatures will not be sent by this mod
         */
        public static int[] Margins = { 10, 10 };

        //How far should we try to move the creature up and down the water column per attempt
        public static int MigrationAmount = 30;

        //This is the depth our creature will assume the water column is if the bottom is unloaded and therefore unable to be found
        public static int FalseBottom = 300;
    }
}
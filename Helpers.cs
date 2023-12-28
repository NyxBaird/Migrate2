using System;
using System.Collections.Generic;
using UnityEngine;

namespace Migrate2
{
    public class Helpers
    {
        private Globals globals;
        
        public Helpers()
        {
            this.globals = Globals.Instance;
            
            //Initialize our traversal paths
            InitTraversals();

            //Register Nocturnal Prey
            globals.Migrators.Add(
                GetCreatureName(TechType.Peeper), 
                new Migrator(
                    TechType.Peeper, 
                    Globals.MigrationTypes.DielNocturnal, 
                    Globals.FoodChainStatus.Prey, 
                    0.75
                )
            );
            globals.Migrators.Add(GetCreatureName(TechType.Oculus), new Migrator(TechType.Oculus, Globals.MigrationTypes.DielNocturnal, Globals.FoodChainStatus.Prey, 0.75));
            globals.Migrators.Add(GetCreatureName(TechType.Bladderfish), new Migrator(TechType.Bladderfish, Globals.MigrationTypes.DielNocturnal, Globals.FoodChainStatus.Prey, 0.75));
            globals.Migrators.Add(GetCreatureName(TechType.Eyeye), new Migrator(TechType.Eyeye, Globals.MigrationTypes.DielNocturnal, Globals.FoodChainStatus.Prey, 0.75));
            globals.Migrators.Add(GetCreatureName(TechType.Spadefish), new Migrator(TechType.Spadefish, Globals.MigrationTypes.DielNocturnal, Globals.FoodChainStatus.Prey, 0.75));
            
            //Register Diurnal Prey
            globals.Migrators.Add(GetCreatureName(TechType.GarryFish), new Migrator(TechType.GarryFish, Globals.MigrationTypes.DielReverse, Globals.FoodChainStatus.Prey, 1));
            globals.Migrators.Add(GetCreatureName(TechType.HoleFish), new Migrator(TechType.HoleFish, Globals.MigrationTypes.DielReverse, Globals.FoodChainStatus.Prey, 0.75));
            globals.Migrators.Add(GetCreatureName(TechType.Reginald), new Migrator(TechType.Reginald, Globals.MigrationTypes.DielReverse, Globals.FoodChainStatus.Prey, 0.8));
            
            //Register Twilight Prey
            globals.Migrators.Add(GetCreatureName(TechType.Mesmer), new Migrator(TechType.Mesmer, Globals.MigrationTypes.DielReverse, Globals.FoodChainStatus.Prey, 1.5));
            globals.Migrators.Add(GetCreatureName(TechType.Boomerang), new Migrator(TechType.Boomerang, Globals.MigrationTypes.DielNocturnal, Globals.FoodChainStatus.Prey, 0.75));
            
            //Register Nocturnal Predators
            globals.Migrators.Add(GetCreatureName(TechType.GhostLeviathan), new Migrator(TechType.GhostLeviathan, Globals.MigrationTypes.DielNocturnal, Globals.FoodChainStatus.Predator, 70)); // These are just so big that I actually sized them down from 107 in order to make this work better
            globals.Migrators.Add(GetCreatureName(TechType.ReaperLeviathan), new Migrator(TechType.ReaperLeviathan, Globals.MigrationTypes.DielNocturnal, Globals.FoodChainStatus.Predator, 55));
            globals.Migrators.Add(GetCreatureName(TechType.Shocker), new Migrator(TechType.Shocker, Globals.MigrationTypes.DielNocturnal, Globals.FoodChainStatus.Predator, 20));
            globals.Migrators.Add(GetCreatureName(TechType.Biter), new Migrator(TechType.Biter, Globals.MigrationTypes.DielNocturnal, Globals.FoodChainStatus.Predator, 1));
            
            //Register Twilight Predators
            globals.Migrators.Add(GetCreatureName(TechType.Sandshark), new Migrator(TechType.Sandshark, Globals.MigrationTypes.DielTwilight, Globals.FoodChainStatus.Predator, 2));
            
            //Some creatures have some unique locations and behaviors that make me reluctant to mess with them
            // globals.Migrators.Add(GetCreatureName(TechType.BoneShark), new Migrator(TechType.BoneShark, Globals.MigrationTypes.DielNocturnal, FoodChainStatus.Predator, 18));
            // globals.Migrators.Add(GetCreatureName(TechType.Stalker), new Migrator(TechType.Stalker, Globals.MigrationTypes.DielTwilight, FoodChainStatus.Predator, 4));
            
            //Register Creatures outside the regular food chain
            globals.Migrators.Add(GetCreatureName(TechType.Reefback), new Migrator(TechType.Reefback, Globals.MigrationTypes.DielReverse, Globals.FoodChainStatus.None, 70));
            
            //This records our biggest and smallest creature sizes for reference
            SetMigratorSizeExtremes();
            
            //This records our creature sizes relative to the biggest registered above
            SetMigratorSizePlacements();
        }
        
        
        /*
         * Returns the maximum possible depth for a given entity vector
         */
        public static float GetPossibleEntityDepth(Vector3 entityPos)
        {
            RaycastHit hit;
            if (Physics.Raycast(entityPos, -Vector3.up, out hit))
                return Math.Abs(entityPos.y) + hit.distance;

            return Globals.FalseBottom;
        }
    
        /*
         * TechTypes are easier to keep track of but we still need the actual name given to Creatures sometimes
         */
        public static string GetCreatureName(TechType type)
        {
            return type.ToString();
        }
        
        /*
         * Initialize our migratory paths
         * globals.Traversals[Type] = Vector2(lightScalar, depth% (0-1.0)
         */
        private void InitTraversals()
        {
            foreach (Globals.MigrationTypes type in (Globals.MigrationTypes[])Enum.GetValues(typeof(Globals.MigrationTypes)))
                globals.Traversals.Add(type, new SortedList<double, double>());
            
            //Diel Nocturnal Migration
            globals.Traversals[Globals.MigrationTypes.DielNocturnal].Add(0, 0.1);
            globals.Traversals[Globals.MigrationTypes.DielNocturnal].Add(0.1, 0.2);
            globals.Traversals[Globals.MigrationTypes.DielNocturnal].Add(0.3, 0.4);
            globals.Traversals[Globals.MigrationTypes.DielNocturnal].Add(0.5, 0.6);
            globals.Traversals[Globals.MigrationTypes.DielNocturnal].Add(0.7, 0.8);
            globals.Traversals[Globals.MigrationTypes.DielNocturnal].Add(0.9, 1);
            
            //Diel Reverse Migration
            globals.Traversals[Globals.MigrationTypes.DielReverse].Add(0, 1);
            globals.Traversals[Globals.MigrationTypes.DielReverse].Add(0.1, 1);
            globals.Traversals[Globals.MigrationTypes.DielReverse].Add(0.2, 0.92);
            globals.Traversals[Globals.MigrationTypes.DielReverse].Add(0.3, 0.75);
            globals.Traversals[Globals.MigrationTypes.DielReverse].Add(0.4, 0.6);
            globals.Traversals[Globals.MigrationTypes.DielReverse].Add(0.5, 0.5);
            globals.Traversals[Globals.MigrationTypes.DielReverse].Add(0.6, 0.4);
            globals.Traversals[Globals.MigrationTypes.DielReverse].Add(0.7, 0.3);
            globals.Traversals[Globals.MigrationTypes.DielReverse].Add(0.8, 0.2);
            globals.Traversals[Globals.MigrationTypes.DielReverse].Add(0.9, 0.1);
            globals.Traversals[Globals.MigrationTypes.DielReverse].Add(1, 0);
            
            //Diel Twilight Migration
            globals.Traversals[Globals.MigrationTypes.DielTwilight].Add(0, 0);
            globals.Traversals[Globals.MigrationTypes.DielTwilight].Add(0.1, 0.8);
            globals.Traversals[Globals.MigrationTypes.DielTwilight].Add(0.2, 0.9);
            globals.Traversals[Globals.MigrationTypes.DielTwilight].Add(0.3, 0.6);
            globals.Traversals[Globals.MigrationTypes.DielTwilight].Add(0.4, 0);
            globals.Traversals[Globals.MigrationTypes.DielTwilight].Add(0.5, 0);
            globals.Traversals[Globals.MigrationTypes.DielTwilight].Add(0.6, 0);
            globals.Traversals[Globals.MigrationTypes.DielTwilight].Add(0.7, 0.6);
            globals.Traversals[Globals.MigrationTypes.DielTwilight].Add(0.8, 0.9);
            globals.Traversals[Globals.MigrationTypes.DielTwilight].Add(0.9, 0.8);
            globals.Traversals[Globals.MigrationTypes.DielTwilight].Add(1, 0);
        }

        /*
         * This records our biggest and smallest creature sizes for reference
         */
        private void SetMigratorSizeExtremes()
        {
            foreach (KeyValuePair<string, Migrator> migrator in globals.Migrators)
            {
                if (migrator.Value.TypicalSize < globals.SmallestMigratorSize)
                    globals.SmallestMigratorSize = migrator.Value.TypicalSize;

                if (migrator.Value.TypicalSize > globals.LargestMigratorSize)
                    globals.LargestMigratorSize = migrator.Value.TypicalSize;
            }
        }

        /*
         * This records our creature sizes relative to the biggest creature registered 
         */
        private void SetMigratorSizePlacements()
        {
            var modifier = 2;
            
            foreach (KeyValuePair<string, Migrator> migrator in globals.Migrators)
            {
                migrator.Value.SizePlacement = (migrator.Value.TypicalSize / globals.LargestMigratorSize * 100) / modifier;
                
                globals.Log.LogDebug(migrator + " received a size placement of " + migrator.Value.SizePlacement); 
            }
        }
    }
}
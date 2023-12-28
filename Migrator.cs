using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Migrate2
{
    public class Migrator
    {
        private Globals globals;
        
        /*
         * These are available right after game launch as presets for the species
         */
        public TechType TechType;
        public Globals.MigrationTypes MigrationType;
        public Globals.FoodChainStatus FoodChainStatus;
        public double TypicalSize; //The typical size in approximate Meters
        public double SizePlacement; //The size% of our creature relative to that of the largest creature
         
        
        /*
         * These are only available at the call from our Creature UpdateBehaviour patch and are instance specific
         */
        private Creature _creature;
        private float _possibleDepth;
        private int _currentLightIndex; //The current index of our traversal according to the current light level
        
        //_traversalRange = Dictionary{current, upper, lower}
        private Dictionary<string, double> _traversalRange;

        public Migrator(TechType creature, Globals.MigrationTypes type, Globals.FoodChainStatus status, double typicalSize)
        {
            globals = Globals.Instance;
            
            TechType = creature;
            MigrationType = type;
            FoodChainStatus = status;
            TypicalSize = typicalSize;
        }
    
        /*
         * Called by our Creature.UpdateBehavior patch to send our creatures to their proper migratory zone 
         */
        public void Migrate(Creature instance)
        {
            _creature = instance;
            _possibleDepth = this.GetPossibleDepth();
            
            if (_creature.name=="ReaperLeviathan(Clone)")
                globals.Log.LogDebug(_creature.name + " has possible depth of " + _possibleDepth + " and is performing " + MigrationType);
            
            //Set the traversal range based on environmental factors
            SetTraversalRange();
            
            //Adjust the traversal range based on creature specific factors
            AdjustTraversalRangeForCreature();

            var minHeight = _traversalRange["current"] - _traversalRange["lower"];
            var maxHeight = _traversalRange["current"] + _traversalRange["upper"];
            
            var creatureHeight = _creature.transform.position.y;

            //If our creature is within its traversal range or is bigger than its traversal range then we don't need to do anything
            if (creatureHeight > minHeight && creatureHeight < maxHeight)
                return;

            // var orientation = _creature.transform.rotation * Vector3.forward;
            var nextLoc = _creature.leashPosition; //new Ray(_creature.transform.position, orientation).GetPoint(20);
            var distanceTravelled = _creature.transform.position - nextLoc;
            var avgDistance = Math.Abs((distanceTravelled.x + distanceTravelled.z) / 2);

            if (creatureHeight < minHeight)
                nextLoc.y += avgDistance;

            if (creatureHeight > maxHeight)
                nextLoc.y -= avgDistance;

            //If our fish is above the top margin send it down
            if (nextLoc.y > 0 - Globals.Margins[0])
            {
                globals.Log.LogDebug("NextLoc " + nextLoc.y + " breached our top margin. Setting to -" + Globals.Margins[0]);
                nextLoc.y = -Globals.Margins[0];
            }

            //If our fish is below the bottom margin send it up
            if (GetDistanceFromBottom() < Globals.Margins[1])
            {
                globals.Log.LogDebug("NextLoc " + nextLoc.y + " breached our bottom margin (Distance; " + GetDistanceFromBottom() + "). Setting to " + (_possibleDepth + Globals.Margins[1]) + " DFB: " + GetDistanceFromBottom());
                nextLoc.y = _possibleDepth + Globals.Margins[1];
            }

            if (nextLoc.y > 0)
                nextLoc.y = -nextLoc.y;
            
            _creature.leashPosition = nextLoc;
            
            globals.Log.LogDebug(_creature.name + " was sent from " + _creature.transform.position + " to " + nextLoc + " with an avg distance travelled of " + avgDistance);
        }

        /*
         * Returns the current migratory range of our creature based on environmental factors
         */
        private void SetTraversalRange()
        {
            var traversal = globals.Traversals[MigrationType];
            var range = new Dictionary<string, double>();
            var lightScalar = Time.LightScalar;
            
            var desiredLight = traversal.OrderBy(v => Math.Abs(v.Key - lightScalar)).First();
            _currentLightIndex = traversal.IndexOfKey(desiredLight.Key);

            //To begin, lets set our minimum traversal range to our previous stops depth
            var min = 
                _currentLightIndex == 0 
                    ? traversal.Values[0] 
                    : traversal.Values[_currentLightIndex - 1];
            
            //...and our maximum traversal range to our next stops depth
            var max = 
                traversal.Keys.Count - 1 == _currentLightIndex
                    ? traversal.Values[_currentLightIndex]
                    : traversal.Values[_currentLightIndex + 1];

            //Get the amount of traversal room that should be available above and below our creature
            var upperDiff = Math.Abs(traversal.Values[_currentLightIndex] - min);
            var lowerDiff = Math.Abs(traversal.Values[_currentLightIndex] - max);

            //Set our traversal ranges
            range["current"] = (_possibleDepth * desiredLight.Value) * -1;
            range["upper"] = _possibleDepth * upperDiff;
            range["lower"] = _possibleDepth * lowerDiff;

            if (_creature.name=="ReaperLeviathan(Clone)") {
                globals.Log.LogDebug("Light scalar; " + lightScalar);
                globals.Log.LogDebug("The water column is " + _possibleDepth + " deep. Fish is @ " + _creature.transform.position.y + " and should be between " + (range["current"] - range["lower"]) + " & " + (range["current"] + range["upper"]) + " (Target: " + range["current"] + ")");
                globals.Log.LogDebug("Player is @ " + PlayerPatch.Player.transform.position.y);
            }
            
            _traversalRange = range;
        }

        /*
         * Returns the current migratory range of our creature based on creature specific factors
         */
        private void AdjustTraversalRangeForCreature()
        {
            var medianDepth = globals.Traversals[MigrationType].Values[_currentLightIndex];
            var inColumn =  medianDepth > 0 && medianDepth < 1;
            
            //If the creature is prey and not at its deepest traversal index then limit its lower range by size
            if (FoodChainStatus == Globals.FoodChainStatus.Prey && !inColumn)
                _traversalRange["lower"] = _traversalRange["lower"] / 100 * SizePlacement;
            
            //If the creature is a predator and not at its deepest traversal index then limit its upper range by size
            if (FoodChainStatus == Globals.FoodChainStatus.Predator && !inColumn)
                _traversalRange["upper"] -= _traversalRange["upper"] / 100 * SizePlacement;
        }

        private float GetDistanceFromBottom()
        {
            RaycastHit hit;
            if (Physics.Raycast(_creature.transform.position, Vector3.down, out hit))
                return hit.distance;

            return Globals.FalseBottom;
        }

        /*
         * Return the maximum possible z axis depth for our terrain at the creatures x and z axis.
         */
        private float GetPossibleDepth()
        {
            return Helpers.GetPossibleEntityDepth(_creature.transform.position);
        }
    }
}
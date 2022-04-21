using System;
using FoodShops.Locations;

namespace FoodShops
{
    /// <summary>
    /// Represents an interior not found. 
    /// </summary>
    public class InteriorNotFoundException : Exception
    {
        /// <summary>
        /// The location that caused the exception.
        /// </summary>
        public Location Location { get; }

        public InteriorNotFoundException(Location location)
        {
            Location = location;
        }
    }

    /// <summary>
    /// Represents a model set as a ped that is not a valid ped.
    /// </summary>
    public class InvalidPedException : Exception
    {
        /// <summary>
        /// The location that caused the exception.
        /// </summary>
        public Location Location { get; }
        
        public InvalidPedException(Location location)
        {
            Location = location;
        }
    }
}

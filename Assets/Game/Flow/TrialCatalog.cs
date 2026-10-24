using System;
using System.Collections.Generic;

namespace NepalTrial.Flow
{
    public static class TrialCatalog
    {
        public const string MenuScene = "NepalTrialMainMenu";
        public const string StarterCar = "starter-car";
        public const string StarterMotorcycle = "starter-motorcycle";
        public const string StarterScooter = "starter-scooter";
        public static readonly string[] Vehicles = { StarterCar, StarterMotorcycle, StarterScooter };
        public static readonly string[] CarCourses = {
            "figure-eight", "u-turn", "hill-start", "traffic-light", "speed-breaker", "garage"
        };
        public static readonly string[] TwoWheelerCourses = {
            "figure-eight", "narrow-plank", "u-turn", "traffic-light", "speed-breaker", "ramp", "behavior"
        };

        public static bool IsKnownVehicle(string id)
        {
            foreach (string vehicle in Vehicles) if (vehicle == id) return true;
            return false;
        }

        public static bool IsTwoWheeler(string vehicle) =>
            vehicle == StarterMotorcycle || vehicle == StarterScooter;

        public static string[] CoursesFor(string vehicle)
        {
            if (!IsKnownVehicle(vehicle)) throw new ArgumentException("Unknown vehicle ID.", nameof(vehicle));
            return (string[])(vehicle == StarterCar ? CarCourses : TwoWheelerCourses).Clone();
        }

        public static string SceneFor(string vehicle, string course)
        {
            if (!IsKnownVehicle(vehicle) || string.IsNullOrEmpty(course)) return null;
            var courses = vehicle == StarterCar ? CarCourses : TwoWheelerCourses;
            bool found = false;
            foreach (string c in courses) if (c == course) { found = true; break; }
            if (!found) return null;
            if (vehicle == StarterCar) return "CarTrial_" + course;
            return "BikeTrial_" + (vehicle == StarterMotorcycle ? "motorcycle_" : "scooter_") + course;
        }

        public static string DisplayVehicle(string vehicle) =>
            vehicle == StarterCar ? "Car" :
            vehicle == StarterMotorcycle ? "Motorcycle" :
            vehicle == StarterScooter ? "Scooter" : "Unknown vehicle";
    }
}

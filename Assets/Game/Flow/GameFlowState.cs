using System;
using System.Collections.Generic;

namespace NepalTrial.Flow
{
    public enum GameScreen { Home, VehicleSelect, ModeSelect, CourseSelect, Driving, Results }
    public enum DriveMode { FreeDrive, Practice, Exam }

    /// <summary>UI-independent state transitions for menus and gameplay.</summary>
    public sealed class GameFlowState
    {
        private readonly HashSet<string> vehicles = new HashSet<string>();
        public GameScreen Screen { get; private set; } = GameScreen.Home;
        public DriveMode Mode { get; private set; } = DriveMode.Practice;
        public string VehicleId { get; private set; }
        public string CourseId { get; private set; }
        public bool Paused { get; private set; }

        public void SetAvailableVehicles(IEnumerable<string> ids)
        {
            vehicles.Clear();
            if (ids == null) return;
            foreach (var id in ids) if (!string.IsNullOrEmpty(id)) vehicles.Add(id);
            if (VehicleId != null && !vehicles.Contains(VehicleId)) VehicleId = null;
        }

        public bool OpenVehicleSelect()
        {
            if (Screen != GameScreen.Home && Screen != GameScreen.Results) return false;
            Screen = GameScreen.VehicleSelect; Paused = false; return true;
        }

        public bool SelectVehicle(string id)
        {
            if (Screen != GameScreen.VehicleSelect || !vehicles.Contains(id)) return false;
            VehicleId = id;
            Screen = GameScreen.ModeSelect;
            return true;
        }

        public bool SelectMode(DriveMode mode)
        {
            if (Screen != GameScreen.ModeSelect || !Enum.IsDefined(typeof(DriveMode), mode)) return false;
            Mode = mode;
            Screen = GameScreen.CourseSelect;
            return true;
        }

        public bool StartCourse(string courseId)
        {
            if (Screen != GameScreen.CourseSelect || VehicleId == null || string.IsNullOrEmpty(courseId)) return false;
            CourseId = courseId;
            Screen = GameScreen.Driving;
            Paused = false;
            return true;
        }

        public bool TogglePause()
        {
            if (Screen != GameScreen.Driving) return false;
            Paused = !Paused;
            return true;
        }

        public bool CompleteCourse()
        {
            if (Screen != GameScreen.Driving || Paused) return false;
            Screen = GameScreen.Results;
            return true;
        }

        public bool Retry()
        {
            if (Screen != GameScreen.Results || string.IsNullOrEmpty(CourseId)) return false;
            Screen = GameScreen.Driving;
            Paused = false;
            return true;
        }

        public void ReturnHome()
        {
            Screen = GameScreen.Home;
            CourseId = null;
            Paused = false;
        }
    }
}

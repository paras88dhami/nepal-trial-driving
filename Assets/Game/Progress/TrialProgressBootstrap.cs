using NepalTrial.Flow;

namespace NepalTrial.Progress
{
    /// <summary>These three starter categories are free; later catalog vehicles can be unlockable.</summary>
    public static class TrialProgressBootstrap
    {
        public static LocalProgressData EnsureStarterVehicles(LocalProgressData data)
        {
            if (data == null) data = new LocalProgressData();
            foreach (string vehicle in TrialCatalog.Vehicles)
                ProgressRules.UnlockVehicle(data,vehicle);
            if (!TrialCatalog.IsKnownVehicle(data.selectedVehicleId))
                data.selectedVehicleId = TrialCatalog.StarterCar;
            return data;
        }

        public static LocalProgressData LoadReady()
        {
            var store = new LocalProgressStore();
            var data = store.Load();
            EnsureStarterVehicles(data);
            store.Save(data);
            return data;
        }
    }
}

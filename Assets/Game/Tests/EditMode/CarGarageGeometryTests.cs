using NUnit.Framework;
using NepalTrial.Trial;
using UnityEngine;

namespace NepalTrial.Tests.EditMode
{
    public sealed class CarGarageGeometryTests
    {
        [Test]
        public void RotatedGarageChecksAllVehicleCorners()
        {
            var garage = new GameObject("GarageRegion");
            var vehicle = new GameObject("VehicleBody");
            try
            {
                var parking = garage.AddComponent<BoxCollider>();
                parking.size = new Vector3(5f, 3f, 8f);
                garage.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
                var body = vehicle.AddComponent<BoxCollider>();
                body.size = new Vector3(1.8f, 1.5f, 3.5f);
                vehicle.transform.position = garage.transform.position;
                vehicle.transform.rotation = garage.transform.rotation;
                Assert.IsTrue(CarGarageGeometry.VehicleFullyInside(parking, body));

                vehicle.transform.position = new Vector3(20f, 0f, 0f);
                Assert.IsFalse(CarGarageGeometry.VehicleFullyInside(parking, body));
            }
            finally
            {
                Object.DestroyImmediate(garage);
                Object.DestroyImmediate(vehicle);
            }
        }
    }
}

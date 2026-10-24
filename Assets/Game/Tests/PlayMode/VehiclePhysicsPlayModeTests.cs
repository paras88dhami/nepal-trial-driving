using System.Collections;
using System.Reflection;
using NepalTrial.Vehicles.Car;
using NepalTrial.Vehicles.TwoWheeler;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NepalTrial.Tests.PlayMode
{
    /// <summary>Minimal integration smoke tests. Does not substitute for full handling QA.</summary>
    public sealed class VehiclePhysicsPlayModeTests
    {
        private static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;

        [UnityTest]
        public IEnumerator CarReceivesTorqueAndResetClearsMotion()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_Phase5";
            floor.transform.position = new Vector3(0f,-.5f,0f);
            floor.transform.localScale = new Vector3(80f,1f,80f);
            var car = new GameObject("Car_Phase5");
            car.SetActive(false);
            car.transform.position = new Vector3(0f,1.2f,0f);
            var body = car.AddComponent<Rigidbody>();
            car.AddComponent<BoxCollider>().size = new Vector3(1.8f,.8f,3.4f);
            var ctrl = car.AddComponent<CarDriveController>();
            foreach (var pair in new[] {
                new { field="frontLeft", pos=new Vector3(-.8f,-.3f,1.1f) },
                new { field="frontRight",pos=new Vector3(.8f,-.3f,1.1f) },
                new { field="rearLeft",pos=new Vector3(-.8f,-.3f,-1.1f) },
                new { field="rearRight",pos=new Vector3(.8f,-.3f,-1.1f) } })
            {
                var wheel = new GameObject(pair.field);
                wheel.transform.SetParent(car.transform,false);
                wheel.transform.localPosition = pair.pos;
                var collider = wheel.AddComponent<WheelCollider>();
                collider.radius=.34f;
                collider.suspensionDistance=.25f;
                typeof(CarDriveController).GetField(pair.field,Hidden).SetValue(ctrl,collider);
            }

            try
            {
                car.SetActive(true);
                yield return new WaitForFixedUpdate();
                ctrl.SetCommand(new CarInputCommand {
                    Throttle = .5f, HasRequestedGear = true, RequestedGear = DriveGear.Drive
                });
                yield return new WaitForFixedUpdate();
                var back = car.transform.Find("rearLeft").GetComponent<WheelCollider>();
                Assert.Greater(back.motorTorque, 0f, "Drive must apply nonzero rear-wheel torque.");
                body.linearVelocity=new Vector3(1f,0f,2f);
                ctrl.ResetVehicle();
                Assert.AreEqual(0f,body.linearVelocity.sqrMagnitude,.0001f);
                Assert.AreEqual(0f,body.angularVelocity.sqrMagnitude,.0001f);
            }
            finally
            {
                Object.Destroy(car);
                Object.Destroy(floor);
            }
        }

        [UnityTest]
        public IEnumerator ScooterAutoTransmissionDrivesRearWheel()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0f,-.5f,0f);
            floor.transform.localScale = new Vector3(80f,1f,80f);
            var bike = new GameObject("Scooter_Phase5");
            bike.SetActive(false);
            bike.transform.position = new Vector3(0f,1.0f,0f);
            bike.AddComponent<Rigidbody>();
            bike.AddComponent<BoxCollider>();
            var ctrl = bike.AddComponent<TwoWheelerRigidbodyController>();
            typeof(TwoWheelerRigidbodyController).GetField("kind",Hidden)
                .SetValue(ctrl,TwoWheelerKind.Scooter);
            WheelCollider rear=null;
            foreach (var pair in new[] {
                new { field="frontWheel",z=.7f },new { field="rearWheel",z=-.7f } })
            {
                var wheel = new GameObject(pair.field);
                wheel.transform.SetParent(bike.transform,false);
                wheel.transform.localPosition = new Vector3(0f,-.35f,pair.z);
                var collider = wheel.AddComponent<WheelCollider>();
                collider.radius=.32f;
                if (pair.field=="rearWheel") rear=collider;
                typeof(TwoWheelerRigidbodyController).GetField(pair.field,Hidden)
                    .SetValue(ctrl,collider);
            }
            try
            {
                bike.SetActive(true);
                ctrl.SetControls(new TwoWheelerControlCommand { Throttle = .6f });
                yield return new WaitForFixedUpdate();
                Assert.IsNotNull(rear);
                Assert.AreEqual(TwoWheelerKind.Scooter,ctrl.Kind);
                Assert.Greater(rear.motorTorque,0f,"Scooter should drive without manual clutch.");
                ctrl.ResetVehicle();
                Assert.AreEqual(0f,rear.motorTorque,.001f);
            }
            finally
            {
                Object.Destroy(bike);
                Object.Destroy(floor);
            }
        }
    }
}

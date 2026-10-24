using System;
using NUnit.Framework;
using NepalTrial.Flow;
using NepalTrial.Progress;

namespace NepalTrial.Tests.EditMode
{
    public sealed class Phase4SessionTests
    {
        [Test]
        public void CatalogProvidesAllVehiclesAndExpectedSceneRoutes()
        {
            Assert.AreEqual(3,TrialCatalog.Vehicles.Length);
            Assert.AreEqual(6,TrialCatalog.CoursesFor(TrialCatalog.StarterCar).Length);
            Assert.AreEqual(7,TrialCatalog.CoursesFor(TrialCatalog.StarterMotorcycle).Length);
            Assert.AreEqual(7,TrialCatalog.CoursesFor(TrialCatalog.StarterScooter).Length);
            Assert.AreEqual("CarTrial_garage",TrialCatalog.SceneFor("starter-car","garage"));
            Assert.AreEqual("BikeTrial_scooter_narrow-plank",
                TrialCatalog.SceneFor("starter-scooter","narrow-plank"));
            Assert.IsNull(TrialCatalog.SceneFor("starter-car","narrow-plank"));
            Assert.IsNull(TrialCatalog.SceneFor("unknown","garage"));
            Assert.Throws<ArgumentException>(() => TrialCatalog.CoursesFor("unknown"));

            var copy = TrialCatalog.CoursesFor(TrialCatalog.StarterCar);
            copy[0] = "tampered";
            Assert.AreEqual("figure-eight",TrialCatalog.CoursesFor(TrialCatalog.StarterCar)[0]);
        }

        [Test]
        public void StarterVehiclesAreUnlockedAndPreserveUserProgress()
        {
            var data = new LocalProgressData();
            var record = ProgressRules.RecordAttempt(data,"practice-car-garage",85,true);
            TrialProgressBootstrap.EnsureStarterVehicles(data);
            TrialProgressBootstrap.EnsureStarterVehicles(data);
            Assert.AreEqual(3,data.unlockedVehicleIds.Count);
            Assert.AreEqual(85,record.bestScore);
            Assert.AreEqual(1,record.attempts);
        }

        [Test]
        public void PracticeEndsAfterSingleCourse()
        {
            var session=TrialSessionRules.Create("starter-car",DriveMode.Practice,"garage");
            Assert.AreEqual("garage",session.CurrentCourse);
            Assert.IsTrue(TrialSessionRules.ReportStageResult(session,true,92));
            Assert.IsTrue(session.ended);
            Assert.IsTrue(session.passed);
            Assert.AreEqual(92,session.finalScore);
            Assert.IsFalse(TrialSessionRules.ReportStageResult(session,true,92));
            Assert.IsFalse(TrialSessionRules.Advance(session));
        }

        [Test]
        public void FreeDriveHasNoAdditionalCoursesAndNeverWritesResult()
        {
            var session=TrialSessionRules.Create("starter-scooter",DriveMode.FreeDrive,"figure-eight");
            Assert.AreEqual(1,session.courses.Length);
            Assert.IsFalse(session.ended);
            Assert.IsFalse(TrialSessionRules.RecordIfNeeded(session,new LocalProgressData()));
        }

        [Test]
        public void SevenStageExamPreservesOrderAndAccumulatesPenaltiesOnce()
        {
            var session=TrialSessionRules.Create("starter-motorcycle",DriveMode.Exam);
            Assert.AreEqual("figure-eight",session.CurrentCourse);
            for(int i=0;i<session.courses.Length;i++)
            {
                Assert.IsTrue(TrialSessionRules.ReportStageResult(session,true,97),i.ToString());
                Assert.IsFalse(TrialSessionRules.ReportStageResult(session,true,97));
                if (i<session.courses.Length-1)
                {
                    Assert.IsTrue(session.AwaitingNext);
                    Assert.IsTrue(TrialSessionRules.Advance(session));
                }
            }
            Assert.IsTrue(session.ended);
            Assert.IsTrue(session.passed);
            Assert.AreEqual(79,session.finalScore);
            Assert.IsFalse(TrialSessionRules.Advance(session));
        }

        [Test]
        public void FailedFinalStageMustNeverProducePassedExam()
        {
            var session=TrialSessionRules.Create("starter-car",DriveMode.Exam);
            for(int i=0;i<session.courses.Length-1;i++)
            {
                Assert.IsTrue(TrialSessionRules.ReportStageResult(session,true,100));
                Assert.IsTrue(TrialSessionRules.Advance(session));
            }
            Assert.IsTrue(TrialSessionRules.ReportStageResult(session,false,100));
            Assert.IsTrue(session.ended);
            Assert.IsFalse(session.passed);
        }

        [Test]
        public void AggregatedScoreBelowThresholdFailsBeforeNextStage()
        {
            var session=TrialSessionRules.Create("starter-car",DriveMode.Exam);
            Assert.IsTrue(TrialSessionRules.ReportStageResult(session,true,80));
            Assert.IsTrue(TrialSessionRules.Advance(session));
            Assert.IsTrue(TrialSessionRules.ReportStageResult(session,true,80));
            Assert.IsTrue(session.ended);
            Assert.IsFalse(session.passed);
            Assert.AreEqual(60,session.finalScore);
        }

        [Test]
        public void RetryingUncommittedStageDoesNotDoubleCountPenalty()
        {
            var session=TrialSessionRules.Create("starter-car",DriveMode.Exam);
            TrialSessionRules.ReportStageResult(session,true,80);
            TrialSessionRules.RestartStage(session);
            Assert.IsFalse(session.AwaitingNext);
            TrialSessionRules.ReportStageResult(session,true,95);
            TrialSessionRules.Advance(session);
            Assert.AreEqual(5,session.completedPenaltyPoints);
            Assert.AreEqual(1,session.stageIndex);
        }

        [Test]
        public void SavedExamRecordIsNotDuplicatedAcrossRetryOrRecovery()
        {
            var session=TrialSessionRules.Create("starter-car",DriveMode.Exam);
            session.stageIndex=session.courses.Length-1;
            Assert.IsTrue(TrialSessionRules.ReportStageResult(session,true,91));
            var save=new LocalProgressData();
            Assert.IsTrue(TrialSessionRules.RecordIfNeeded(session,save));
            Assert.IsFalse(TrialSessionRules.RecordIfNeeded(session,save));
            Assert.AreEqual(1,save.records[0].attempts);

            // Simulate a crash after data save but before the session's saved flag.
            session.recordWritten=false;
            Assert.IsFalse(TrialSessionRules.RecordIfNeeded(session,save));
            Assert.IsTrue(session.recordWritten);
            Assert.AreEqual(1,save.records[0].attempts);
        }
    }
}

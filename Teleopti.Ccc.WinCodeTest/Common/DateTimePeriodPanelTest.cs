using System;
using System.Reflection;
using System.Windows;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class DateTimePeriodPanelTest
    {
        private DateTimePeriodPanel target;
        private DateTimePeriod _period;
        private DateTimePeriod _limitPeriod;
        private CrossThreadTestRunner testRunner;

        [SetUp]
        public void Setup()
        {
            _period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 05, 0, 0, 0, DateTimeKind.Utc), 0);
            _limitPeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 07, 0, 0, 0, DateTimeKind.Utc), 0);

            testRunner = new CrossThreadTestRunner();
        }

        [Test]
        public void VerifyProperties()
        {
            testRunner.RunInSTA(
                delegate
                    {
                        target = new DateTimePeriodPanel();
                        Assert.AreEqual(0, target.Overlap);
                        target.Overlap = 1;
                        Assert.AreEqual(1, target.Overlap);
                        Assert.AreEqual(20, target.MinimumItemHeight);

                        DateTimePeriodPanel.SetClipPeriod(target, true);
                        Assert.IsTrue(DateTimePeriodPanel.GetClipPeriod(target));

                        DateTimePeriodPanel.SetDateTimePeriod(target, _period);
                        Assert.AreEqual(_period, DateTimePeriodPanel.GetDateTimePeriod(target));

                        DateTimePeriodPanel.SetLimitDateTimePeriod(target, _limitPeriod);
                        Assert.AreEqual(_limitPeriod, DateTimePeriodPanel.GetLimitDateTimePeriod(target));

                        Assert.AreEqual(new DateTimePeriod(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
                                                           DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc).Add
                                                               (
                                                               TimeSpan.FromMinutes(1))),
                                        DateTimePeriodPanel.GetDateTimePeriod(target));

                        DateTimePeriodPanel.SetClipPeriod(target, false);
                        Assert.AreEqual(_period, DateTimePeriodPanel.GetDateTimePeriod(target));
                    });
        }

        [Test]
        public void VerifyGetUtcDateTimeFromPosition()
        {
            testRunner.RunInSTA(
                delegate
                    {
                        target = new DateTimePeriodPanel();
                        FieldInfo fieldSize = typeof (UIElement).GetField("_size",
                                                                          BindingFlags.Instance | BindingFlags.NonPublic |
                                                                          BindingFlags.FlattenHierarchy);
                        fieldSize.SetValue(target, new Size(10, 2));
                        DateTimePeriodPanel.SetClipPeriod(target, false);
                        DateTimePeriodPanel.SetDateTimePeriod(target, _period);
                        Assert.AreEqual(_period.StartDateTime, target.GetUtcDateTimeFromPosition(0));
                    });
        }

        [Test]
        public void VerifyGetTimeSpanFromHorizontalChange()
        {
            testRunner.RunInSTA(
                delegate
                    {
                        target = new DateTimePeriodPanel();
                        FieldInfo fieldSize = typeof (UIElement).GetField("_size",
                                                                          BindingFlags.Instance | BindingFlags.NonPublic |
                                                                          BindingFlags.FlattenHierarchy);
                        fieldSize.SetValue(target, new Size(10, 2));
                        DateTimePeriodPanel.SetClipPeriod(target, false);
                        DateTimePeriodPanel.SetDateTimePeriod(target, _period);
                        Assert.AreEqual(TimeSpan.FromHours(12),
                                        DateTimePeriodPanel.GetTimeSpanFromHorizontalChange(target, 5, 1));
                        Assert.AreEqual(TimeSpan.Zero,
                                        DateTimePeriodPanel.GetTimeSpanFromHorizontalChange(target, 0.001, 1));
                        Assert.AreEqual(TimeSpan.FromHours(13).Add(TimeSpan.FromMinutes(30)),
                                        DateTimePeriodPanel.GetTimeSpanFromHorizontalChange(target, 5.6, 30));
                    });
        }

        [Test]
        public void VerifyCanMeasure()
        {
            testRunner.RunInSTA(
                delegate
                    {
                        target = new DateTimePeriodPanel();
                        UIElement child = new UIElement();
                        child.SetValue(DateTimePeriodPanel.DateTimePeriodProperty, _period);
                        target.SetValue(DateTimePeriodPanel.DateTimePeriodProperty, _period);
                        target.Children.Add(child);
                        target.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    });
        }

        [Test]
        public void VerifyCanArrange()
        {
            testRunner.RunInSTA(
                delegate
                    {
                        target = new DateTimePeriodPanel();
                        UIElement child = new UIElement();
                        child.SetValue(DateTimePeriodPanel.DateTimePeriodProperty,
                                       _period.ChangeStartTime(TimeSpan.FromHours(-1)));
                        target.SetValue(DateTimePeriodPanel.DateTimePeriodProperty, _period);
                        target.Children.Add(child);
                        target.Arrange(new Rect(0, 0, 10, 2));
                    });
        }

    }
}

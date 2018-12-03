using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
    public class WorkShiftCalculatorTest
    {
        private readonly DateTime _date = new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc);
        private IVisualLayerFactory _layerFactory;
        private NonSecretWorkShiftCalculatorClassic _target;

    	[SetUp]
        public void Setup()
        {
            _layerFactory = new VisualLayerFactory();
			_target = new NonSecretWorkShiftCalculatorClassic();
        }

		[Test]
        public void VerifyCalculate()
        {
			var period1 = new DateTimePeriod(_date, _date.AddMinutes(15));
            DateTimePeriod period2 = new DateTimePeriod(_date.AddMinutes(15), _date.AddMinutes(30));
            DateTimePeriod period3 = new DateTimePeriod(_date.AddMinutes(30), _date.AddMinutes(45));
            DateTimePeriod period4 = new DateTimePeriod(_date.AddMinutes(45), _date.AddMinutes(60));

			var activity1 = ActivityFactory.CreateActivity("ittan");
			activity1.RequiresSkill = false;

			var activity2 = ActivityFactory.CreateActivity("tvåan");
			activity2.RequiresSkill = false;

            IVisualLayer layer1 = _layerFactory.CreateShiftSetupLayer(activity1, period1);
            IVisualLayer layer2 = _layerFactory.CreateShiftSetupLayer(activity2, period2);
            IVisualLayer layer3 = _layerFactory.CreateShiftSetupLayer(activity1, period3);
            IVisualLayer layer4 = _layerFactory.CreateShiftSetupLayer(activity1, period4);


            IList<IVisualLayer> layers = new List<IVisualLayer> { layer1, layer2, layer3, layer4 };
            IVisualLayerCollection layerCollection = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

            SkillStaffPeriodDataInfo dataHolder1 = new SkillStaffPeriodDataInfo(5, 80, period1, 1, 5, 0, null);

            SkillStaffPeriodDataInfo dataHolder3 = new SkillStaffPeriodDataInfo(0, 0, period3, 1, 5, 0, null);
            SkillStaffPeriodDataInfo dataHolder4 = new SkillStaffPeriodDataInfo(5, 5, period4, 1, 5, 0, null);

            Dictionary<DateTime, ISkillStaffPeriodDataHolder> dataHolders = new Dictionary<DateTime, ISkillStaffPeriodDataHolder>();
            dataHolders.Add(period1.StartDateTime, dataHolder1);
            dataHolders.Add(period3.StartDateTime, dataHolder3);
            dataHolders.Add(period4.StartDateTime, dataHolder4);

            Dictionary<DateTime, ISkillStaffPeriodDataHolder> dataHolders2 = new Dictionary<DateTime, ISkillStaffPeriodDataHolder>();
            dataHolders2.Add(period3.StartDateTime, dataHolder3);
            dataHolders2.Add(period4.StartDateTime, dataHolder4);

            Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> skillStaffPeriods = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
            skillStaffPeriods.Add(activity1, dataHolders);
            skillStaffPeriods.Add(activity2, dataHolders2);

            _target.CalculateShiftValue(new WorkShiftCalculatableVisualLayerCollection(layerCollection), new WorkShiftCalculatorSkillStaffPeriodData(skillStaffPeriods), WorkShiftLengthHintOption.Free, true, true);
        }

        [Test]
        public void VerifyThatFindLongerShiftForUnderstaffingSituation()
        {
            int resolution = 15;
            WorkShiftLengthHintOption lengthFactor = WorkShiftLengthHintOption.Long;

            double oldPeriodValue1 = -70d;
            int resourceInMinutes1 = 420;
            double result1 = _target.CalculateShiftValueForPeriod(oldPeriodValue1, resourceInMinutes1, lengthFactor, resolution);

            double oldPeriodValue2 = -75d;
            int resourceInMinutes2 = 480;
            double result2 = _target.CalculateShiftValueForPeriod(oldPeriodValue2, resourceInMinutes2, lengthFactor, resolution);

            double oldPeriodValue3 = -80d;
            int resourceInMinutes3 = 540;
            double result3 = _target.CalculateShiftValueForPeriod(oldPeriodValue3, resourceInMinutes3, lengthFactor, resolution);

            Assert.IsTrue(result2 > result1);
            Assert.IsTrue(result3 > result2);
        }

        [Test]
        public void VerifyCalculateShiftValueCalculationIsExact()
        {
            // normal
            double oldPeriodValue = 600d;
            int resourceInMinutes = 300;
            int resolution = 30;
            WorkShiftLengthHintOption lengthFactor = WorkShiftLengthHintOption.AverageWorkTime;
            double result;
            double expected;

            expected = 600d;
			result = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, lengthFactor, resolution);

            Assert.AreEqual(expected, result, 1d);

            oldPeriodValue = -600d;
            expected = -600d;
			result = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, lengthFactor, resolution);

            Assert.AreEqual(expected, result, 1d);

            oldPeriodValue = 0d;
            expected = 0d;
			result = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, lengthFactor, resolution);
            Assert.AreEqual(expected, result);

            // Long
            lengthFactor = WorkShiftLengthHintOption.Long;

            oldPeriodValue = 600d;
            expected = 2394;
			result = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, lengthFactor, resolution);

            Assert.AreEqual(expected, result, 1d);

            oldPeriodValue = -600d;
            expected = 1194;
			result = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, lengthFactor, resolution);

            Assert.AreEqual(expected, result, 1d);

            oldPeriodValue = 0d;
            expected = 0d;
			result = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, lengthFactor, resolution);
            Assert.AreEqual(expected, result);

            // Long
            lengthFactor = WorkShiftLengthHintOption.Short;

            oldPeriodValue = 600d;
            expected = -1194;
			result = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, lengthFactor, resolution);

            Assert.AreEqual(expected, result, 1d);

            oldPeriodValue = -600d;
            expected = -2394;
			result = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, lengthFactor, resolution);

            Assert.AreEqual(expected, result, 1d);

            oldPeriodValue = 0d;
            expected = 0d;
			result = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, lengthFactor, resolution);
            Assert.AreEqual(expected, result);


        }

        /// <summary>
        /// Verifies the calculate shift value length factor.
        /// </summary>
        /// <remarks>
        /// Basically there are 4 main range:
        /// <list type="bullet">
        /// 	<item>
        /// 		<description>Value is over 1</description>
        /// 	</item>
        /// 	<item>
        /// 		<description>Value is between 0 and 1</description>
        /// 	</item>
        /// 	<item>
        /// 		<description>Value is lower than -1</description>
        /// 	</item>
        /// 	<item>
        /// 		<description>Value is between 0 and -1</description>
        /// 	</item>
        /// </list>
        /// </remarks>
        [Test]
        public void VerifyCalculateShiftValueLengthFactor()
        {
            // case 1: x > 1
            double oldPeriodValue = 600d;
            int resourceInMinutes = 300;
            int resolution = 30;
            double resultLong;
            double resultNone;
            double resultShort;

			resultLong = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Long, resolution);
			resultNone = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.AverageWorkTime, resolution);
			resultShort = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Short, resolution);

            Assert.IsTrue(resultLong > resultNone);
            Assert.IsTrue(resultNone > resultShort);

            // case 2: x = 1
            oldPeriodValue = 1d;
            resourceInMinutes = 100;
            resultLong = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Long, resolution);
            resultNone = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.AverageWorkTime, resolution);
            resultShort = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Short, resolution);

            Assert.IsTrue(resultLong > resultNone);
            Assert.IsTrue(resultNone > resultShort);


            // case 3: 0 < x < 1
            oldPeriodValue = 0.6d;
            resourceInMinutes = 100;
			resultLong = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Long, resolution);
			resultNone = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.AverageWorkTime, resolution);
			resultShort = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Short, resolution);

            Assert.IsTrue(resultLong > resultNone);
            Assert.IsTrue(resultNone > resultShort);

            // case 4: x < -1
            oldPeriodValue = -600d;
            resourceInMinutes = 300;
            resultLong = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Long, resolution);
            resultNone = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.AverageWorkTime, resolution);
            resultShort = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Short, resolution);

            Assert.IsTrue(resultLong > resultNone);
            Assert.IsTrue(resultNone > resultShort);

            // case 5: x = -1
            oldPeriodValue = -1d;
            resourceInMinutes = 10;
			resultLong = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Long, resolution);
			resultNone = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.AverageWorkTime, resolution);
			resultShort = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Short, resolution);

            Assert.IsTrue(resultLong > resultNone);
            Assert.IsTrue(resultNone > resultShort);

            // case 6: 0 > x > -1
            oldPeriodValue = -0.6d;
            resourceInMinutes = 10;
			resultLong = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Long, resolution);
			resultNone = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.AverageWorkTime, resolution);
			resultShort = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Short, resolution);

            Assert.IsTrue(resultLong > resultNone);
            Assert.IsTrue(resultNone > resultShort);
        }

        /// <summary>
        /// Verifies the calculate shift value length factor.
        /// </summary>
        /// <remarks>
        /// Basically there are 4 main range:
        /// <list type="bullet">
        /// 	<item>
        /// 		<description>Value is over 1</description>
        /// 	</item>
        /// 	<item>
        /// 		<description>Value is between 0 and 1</description>
        /// 	</item>
        /// 	<item>
        /// 		<description>Value is lower than -1</description>
        /// 	</item>
        /// 	<item>
        /// 		<description>Value is between 0 and -1</description>
        /// 	</item>
        /// </list>
        /// </remarks>
        [Test]
        public void VerifyCalculateShiftValueHigherResourceGetsHigherValue()
        {
            // case 1: 1.1 > 0.9
            double oldPeriodValue = 1.1d;
            int resourceInMinutes = 1;
            int resolution = 30;
            double resultLongHigher;
            double resultNoneHigher;
            double resultShortHigher;
            double resultLongLower;
            double resultNoneLower;
            double resultShortLower;

			resultLongHigher = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Long, resolution);
			resultNoneHigher = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.AverageWorkTime, resolution);
			resultShortHigher = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Short, resolution);

            oldPeriodValue = 0.9;
            resourceInMinutes = 1;
			resultLongLower = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Long, resolution);
			resultNoneLower = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.AverageWorkTime, resolution);
			resultShortLower = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Short, resolution);

            Assert.IsTrue(resultLongHigher > resultLongLower);
            Assert.IsTrue(resultNoneHigher > resultNoneLower);
            Assert.IsTrue(resultShortHigher > resultShortLower);
        }

        [Test]
        public void VerifyCalculateShiftValuePositiveNegativeSignNeverChange()
        {
            double oldPeriodValue = 0.1;
            int resourceInMinutes = 30;
            int resolution = 30;
            double result;

			result = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Long, resolution);
            Assert.IsTrue(result > 0);

            oldPeriodValue = -0.1;
			result = _target.CalculateShiftValueForPeriod(oldPeriodValue, resourceInMinutes, WorkShiftLengthHintOption.Long, resolution);
            Assert.IsTrue(result < 0);
        }

        [Test]
        public void VerifyCalculateWhenRequiresSkill()
        {
            var period2 = new DateTimePeriod(_date.AddMinutes(15), _date.AddMinutes(30));

			var activity1 = ActivityFactory.CreateActivity("ittan");
            activity1.RequiresSkill = true;

            IVisualLayer layer1 = _layerFactory.CreateShiftSetupLayer(activity1, new DateTimePeriod(_date, _date.AddMinutes(60)));

            IList<IVisualLayer> layers = new List<IVisualLayer> { layer1 };
            IVisualLayerCollection layerCollection = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

            SkillStaffPeriodDataInfo dataHolder1 = new SkillStaffPeriodDataInfo(5, 80, period2, 1, 5, 0, null);

            Dictionary<DateTime, ISkillStaffPeriodDataHolder> dataHolders = new Dictionary<DateTime, ISkillStaffPeriodDataHolder>();
            dataHolders.Add(period2.StartDateTime, dataHolder1);

            var skillStaffPeriods = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
            skillStaffPeriods.Add(activity1, dataHolders);

            _target.CalculateShiftValue(new WorkShiftCalculatableVisualLayerCollection(layerCollection), new WorkShiftCalculatorSkillStaffPeriodData(skillStaffPeriods), WorkShiftLengthHintOption.Free, true, true);

        }

	    [Test]
        public void VerifyCalculateWhenRequiresSkillOnEnd()
        {
            var period1 = new DateTimePeriod(_date, _date.AddMinutes(15));

			var activity1 = ActivityFactory.CreateActivity("ittan");
			var activity2 = ActivityFactory.CreateActivity("tvåan");
            activity1.RequiresSkill = true;

            IVisualLayer layer1 = _layerFactory.CreateShiftSetupLayer(activity1, new DateTimePeriod(_date, _date.AddMinutes(60)));

            IList<IVisualLayer> layers = new List<IVisualLayer> { layer1 };
            IVisualLayerCollection layerCollection = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

            ISkillStaffPeriodDataHolder dataHolder1 = new SkillStaffPeriodDataInfo(5, 80, period1, 1, 5, 0, null);

            var dataHolders = new Dictionary<DateTime, ISkillStaffPeriodDataHolder>();
            dataHolders.Add(period1.StartDateTime, dataHolder1);

            var skillStaffPeriods = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
            skillStaffPeriods.Add(activity2, dataHolders);

            _target.CalculateShiftValue(new WorkShiftCalculatableVisualLayerCollection(layerCollection), new WorkShiftCalculatorSkillStaffPeriodData(skillStaffPeriods), WorkShiftLengthHintOption.Free, true, true);
        }

        [Test]
        public void VerifyCalculateWhenNotRequiresSkillInMiddle()
        {
            var period1 = new DateTimePeriod(_date, _date.AddMinutes(15));
            var period4 = new DateTimePeriod(_date.AddMinutes(45), _date.AddMinutes(60));

			var activity1 = ActivityFactory.CreateActivity("ittan");
            activity1.RequiresSkill = false;


            IVisualLayer layer1 = _layerFactory.CreateShiftSetupLayer(activity1, new DateTimePeriod(_date, _date.AddMinutes(60)));

            IList<IVisualLayer> layers = new List<IVisualLayer> { layer1 };
            IVisualLayerCollection layerCollection = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

            var dataHolder1 = new SkillStaffPeriodDataInfo(5, 80, period1, 1, 5, 0, null);

            var dataHolder4 = new SkillStaffPeriodDataInfo(5, 5, period4, 1, 5, 0, null);

            var dataHolders = new Dictionary<DateTime, ISkillStaffPeriodDataHolder>();
            dataHolders.Add(period1.StartDateTime, dataHolder1);
            //closed
            dataHolders.Add(period4.StartDateTime, dataHolder4);

            var skillStaffPeriods = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
            skillStaffPeriods.Add(activity1, dataHolders);

            _target.CalculateShiftValue(new WorkShiftCalculatableVisualLayerCollection(layerCollection), new WorkShiftCalculatorSkillStaffPeriodData(skillStaffPeriods), WorkShiftLengthHintOption.Free, true, true);
        }

        [Test]
        public void VerifyCalculateWhenPeriodOnLayerNotTheSameAsDataHolder()
        {
            DateTimePeriod period1 = new DateTimePeriod(_date, _date.AddMinutes(15));
            DateTimePeriod oddPeriod1 = new DateTimePeriod(_date.AddMinutes(5), _date.AddMinutes(10));
            DateTimePeriod period3 = new DateTimePeriod(_date.AddMinutes(30), _date.AddMinutes(45));
            DateTimePeriod period4 = new DateTimePeriod(_date.AddMinutes(45), _date.AddMinutes(60));

			var activity1 = ActivityFactory.CreateActivity("ittan");
			activity1.RequiresSkill = false;

            IVisualLayer layer1 = _layerFactory.CreateShiftSetupLayer(activity1, oddPeriod1);

            IList<IVisualLayer> layers = new List<IVisualLayer> { layer1 };
            IVisualLayerCollection layerCollection = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

            SkillStaffPeriodDataInfo dataHolder1 = new SkillStaffPeriodDataInfo(5, 80, period1, 1, 5, 0, null);

            SkillStaffPeriodDataInfo dataHolder3 = new SkillStaffPeriodDataInfo(0, 0, period3, 1, 5, 0, null);
            SkillStaffPeriodDataInfo dataHolder4 = new SkillStaffPeriodDataInfo(5, 5, period4, 1, 5, 0, null);

            var dataHolders = new Dictionary<DateTime, ISkillStaffPeriodDataHolder>();
            dataHolders.Add(period1.StartDateTime, dataHolder1);
            dataHolders.Add(period3.StartDateTime, dataHolder3);
            dataHolders.Add(period4.StartDateTime, dataHolder4);

            var skillStaffPeriods = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
            skillStaffPeriods.Add(activity1, dataHolders);

            _target.CalculateShiftValue(new WorkShiftCalculatableVisualLayerCollection(layerCollection), new WorkShiftCalculatorSkillStaffPeriodData(skillStaffPeriods), WorkShiftLengthHintOption.Free, true, true);

            
        }

        [Test]
        public void VerifyCalculateWhenShortBreakShorterThanPeriod()
        {
            var period1 = new DateTimePeriod(_date, _date.AddMinutes(30));
            var period2 = new DateTimePeriod(_date.AddMinutes(30), _date.AddMinutes(60));
            var period3 = new DateTimePeriod(_date.AddMinutes(60), _date.AddMinutes(90));
            var period4 = new DateTimePeriod(_date.AddMinutes(90), _date.AddMinutes(120));

			var activityPhone = ActivityFactory.CreateActivity("phone");
            activityPhone.RequiresSkill = true;

			var activityBreak = ActivityFactory.CreateActivity("ShortBreak");
            activityBreak.RequiresSkill = false;

            IVisualLayer layerPhone = _layerFactory.CreateShiftSetupLayer(activityPhone, new DateTimePeriod(_date, _date.AddMinutes(120)));
            // only 15 minutes
            IVisualLayer layerBreak = _layerFactory.CreateShiftSetupLayer(activityBreak, new DateTimePeriod(_date.AddMinutes(60), _date.AddMinutes(75)));
            IVisualLayer layerBreak2 = _layerFactory.CreateShiftSetupLayer(activityBreak, new DateTimePeriod(_date.AddMinutes(75), _date.AddMinutes(90)));

            IList<IVisualLayer> layers = new List<IVisualLayer> { layerPhone, layerBreak };
            IVisualLayerCollection layerCollection = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

            var dataHolder1 = new SkillStaffPeriodDataInfo(5, 80, period1, 1, 5, 0, null);
            var dataHolder2 = new SkillStaffPeriodDataInfo(5, 80, period2, 1, 5, 0, null);
            var dataHolder3 = new SkillStaffPeriodDataInfo(5, 80, period3, 1, 5, 0, null);
            var dataHolder4 = new SkillStaffPeriodDataInfo(5, 5, period4, 1, 5, 0, null);

            var dataHolders = new Dictionary<DateTime, ISkillStaffPeriodDataHolder>();
            dataHolders.Add(period1.StartDateTime, dataHolder1);
            dataHolders.Add(period2.StartDateTime, dataHolder2);
            dataHolders.Add(period3.StartDateTime, dataHolder3);
            dataHolders.Add(period4.StartDateTime, dataHolder4);

            var skillStaffPeriods = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
            skillStaffPeriods.Add(activityPhone, dataHolders);

			var result = _target.CalculateShiftValue(new WorkShiftCalculatableVisualLayerCollection(layerCollection),new WorkShiftCalculatorSkillStaffPeriodData(skillStaffPeriods), WorkShiftLengthHintOption.Free, true, true);

            Assert.Greater(result.Value, double.MinValue);

            layers = new List<IVisualLayer> { layerPhone, layerBreak2 };
            layerCollection = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

			var result2 = _target.CalculateShiftValue(new WorkShiftCalculatableVisualLayerCollection(layerCollection), new WorkShiftCalculatorSkillStaffPeriodData(skillStaffPeriods), WorkShiftLengthHintOption.Free, true, true);

            Assert.AreEqual(-3240, result.Value);
            Assert.AreEqual(result.Value, result2.Value);
        }

        [Test]
        public void VerifyOutsideOpenHours()
        {
            var period1 = new DateTimePeriod(_date, _date.AddMinutes(15));
            var period2 = new DateTimePeriod(_date.AddMinutes(15), _date.AddMinutes(30));
            var period3 = new DateTimePeriod(_date.AddMinutes(30), _date.AddMinutes(45));

			var activityPhone = ActivityFactory.CreateActivity("phone");
			activityPhone.RequiresSkill = true;

            IVisualLayer layerPhone = _layerFactory.CreateShiftSetupLayer(activityPhone, new DateTimePeriod(_date, _date.AddMinutes(120)));

            IList<IVisualLayer> layers = new List<IVisualLayer> { layerPhone };
            IVisualLayerCollection layerCollection = new VisualLayerCollection(layers, new ProjectionPayloadMerger());

            var dataHolder1 = new SkillStaffPeriodDataInfo(5, 80, period1, 1, 5, 0, null);
            var dataHolder2 = new SkillStaffPeriodDataInfo(5, 80, period2, 1, 5, 0, null);
            var dataHolder3 = new SkillStaffPeriodDataInfo(5, 80, period3, 1, 5, 0, null);
            var dataHolders = new Dictionary<DateTime, ISkillStaffPeriodDataHolder>
                                                                                {
                                                                                    {period1.StartDateTime, dataHolder1},
                                                                                    {period2.StartDateTime, dataHolder2},
                                                                                    {period3.StartDateTime, dataHolder3}
                                                                                };

            var skillStaffPeriods = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
            skillStaffPeriods.Add(activityPhone, dataHolders);

	        var result = _target.CalculateShiftValue(new WorkShiftCalculatableVisualLayerCollection(layerCollection), new WorkShiftCalculatorSkillStaffPeriodData(skillStaffPeriods), 0, false, false);
            
            Assert.AreEqual(double.MinValue, result.Value);
        }

        [Test]
        public void ShouldBeSameEffectIfShiftValueIsNegativeOrPositive()
        {
            double value1 = _target.CalculateShiftValueForPeriod(130, 480, WorkShiftLengthHintOption.Short, 15);
            double value2 = _target.CalculateShiftValueForPeriod(130, 480, WorkShiftLengthHintOption.Long, 15);
            Assert.IsTrue(value1 < value2);

            double value3 = _target.CalculateShiftValueForPeriod(-130, 480, WorkShiftLengthHintOption.Short, 15);
            double value4 = _target.CalculateShiftValueForPeriod(-130, 480, WorkShiftLengthHintOption.Long, 15);
            Assert.IsTrue(value3 < value4);

            Assert.AreEqual(Math.Abs(value1), Math.Abs(value4));
            Assert.AreEqual(Math.Abs(value2), Math.Abs(value3));
        }

        [Test]
        public void LongShiftShouldHaveHigherValueThanShortIfHintIsLong()
        {
            double value1 = _target.CalculateShiftValueForPeriod(130, 360, WorkShiftLengthHintOption.Long, 15);
            double value2 = _target.CalculateShiftValueForPeriod(130, 480, WorkShiftLengthHintOption.Long, 15);
            Assert.IsTrue(value1 < value2);

            value1 = _target.CalculateShiftValueForPeriod(-130, 360, WorkShiftLengthHintOption.Long, 15);
            value2 = _target.CalculateShiftValueForPeriod(-130, 480, WorkShiftLengthHintOption.Long, 15);
            Assert.IsTrue(value1 < value2);
        }

        [Test]
        public void ShortShiftShouldHaveHigherValueThanLongIfHintIsShort()
        {
            double value1 = _target.CalculateShiftValueForPeriod(130, 480, WorkShiftLengthHintOption.Short, 15);
            double value2 = _target.CalculateShiftValueForPeriod(130, 360, WorkShiftLengthHintOption.Short, 15);
            Assert.IsTrue(value1 < value2);

            value1 = _target.CalculateShiftValueForPeriod(-130, 480, WorkShiftLengthHintOption.Short, 15);
            value2 = _target.CalculateShiftValueForPeriod(-130, 360, WorkShiftLengthHintOption.Short, 15);
            Assert.IsTrue(value1 < value2);
        }
    }
}

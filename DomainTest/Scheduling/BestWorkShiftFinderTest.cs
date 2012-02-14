using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class BestWorkShiftFinderTest
    {
        private MockRepository _mocks;
        private BestWorkShiftFinder _target;
        private IWorkShiftCalculator _workShiftCalculator;
        private IPerson _person;
        private IVisualLayerCollection _layers;
        private IShiftProjectionCache _cashe;
        private List<IShiftProjectionCache> _cashes;
        private IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> _dataHolderDictionary;
        private INonBlendWorkShiftCalculator _nonBlendCalculator;
        private IDictionary<ISkill, ISkillStaffPeriodDictionary> _nonBlendSkillStaffs;
        private IScheduleDictionary _scheduleDic;
        private IFairnessValueCalculator _fairnessCalculator;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDic = _mocks.StrictMock<IScheduleDictionary>();
            _workShiftCalculator = _mocks.StrictMock<IWorkShiftCalculator>();
            _nonBlendCalculator = _mocks.DynamicMock<INonBlendWorkShiftCalculator>();
            _fairnessCalculator = _mocks.StrictMock<IFairnessValueCalculator>();
            _target = new BestWorkShiftFinder(_scheduleDic,_workShiftCalculator, _nonBlendCalculator, _fairnessCalculator);
            _person = _mocks.StrictMock<IPerson>();
            _layers = _mocks.StrictMock<IVisualLayerCollection>();
            _cashe = _mocks.StrictMock<IShiftProjectionCache>();
            _cashes = new List<IShiftProjectionCache> {_cashe};
            _dataHolderDictionary =
                _mocks.StrictMock<IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>>();
            _nonBlendSkillStaffs = _mocks.StrictMock<IDictionary<ISkill, ISkillStaffPeriodDictionary>>();
        }



        

        [Test]
        public void ShouldCallNonBlendCalculatorIsNotEmpty()
        {
            Expect.Call(_dataHolderDictionary.Count).Return(0);
            Expect.Call(_nonBlendSkillStaffs.Count).Return(1);
            Expect.Call(_cashe.MainShiftProjection).Return(_layers);
            Expect.Call(_nonBlendCalculator.CalculateShiftValue(_person, _layers, _nonBlendSkillStaffs, 1, true, true)).Return(5);

            _mocks.ReplayAll();
            var result = _target.FindBestMainShift(_person, _cashes, _dataHolderDictionary, _nonBlendSkillStaffs, 1, true, true);
            Assert.That(result.Value, Is.EqualTo(5));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallWorkShiftCalculatorWhenDictionaryIsNotEmpty()
        {
            Expect.Call(_dataHolderDictionary.Count).Return(1);
            Expect.Call(_cashe.MainShiftProjection).Return(_layers);
            Expect.Call(_workShiftCalculator.CalculateShiftValue(_layers, _dataHolderDictionary, 1,true, true)).Return(5);
            Expect.Call(_nonBlendSkillStaffs.Count).Return(0);
            _mocks.ReplayAll();
            var result = _target.FindBestMainShift(_person, _cashes, _dataHolderDictionary, _nonBlendSkillStaffs, 1, true, true);
            Assert.That(result.Value, Is.EqualTo(5));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldTakeCareOfDoubleMinWhenReturnedFromCalculator()
        {
            Expect.Call(_dataHolderDictionary.Count).Return(1);
            Expect.Call(_cashe.MainShiftProjection).Return(_layers);
            Expect.Call(_workShiftCalculator.CalculateShiftValue(_layers, _dataHolderDictionary, 1, true, true)).Return(double.MinValue);
            Expect.Call(_nonBlendSkillStaffs.Count).Return(0);
            _mocks.ReplayAll();
            var result = _target.FindBestMainShift(_person, _cashes, _dataHolderDictionary, _nonBlendSkillStaffs ,1, true, true);
            Assert.That(result, Is.Null);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallBothCalculatorWhenBothDictionariesFilled()
        {
            Expect.Call(_dataHolderDictionary.Count).Return(1);
            Expect.Call(_cashe.MainShiftProjection).Return(_layers).Repeat.Twice();
            Expect.Call(_workShiftCalculator.CalculateShiftValue(_layers, _dataHolderDictionary, 1, true, true)).Return(5);
            Expect.Call(_nonBlendSkillStaffs.Count).Return(1);
            Expect.Call(_nonBlendCalculator.CalculateShiftValue(_person, _layers, _nonBlendSkillStaffs, 4, true, true)).Return(5);
            _mocks.ReplayAll();
            var result = _target.FindBestMainShift(_person, _cashes, _dataHolderDictionary, _nonBlendSkillStaffs,4, true, true);
            Assert.That(result.Value, Is.EqualTo(10));
            _mocks.VerifyAll();
        }
    }

    public class BestWorkShiftFinder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private IScheduleDictionary _scheduleDictionary;
        private readonly INonBlendWorkShiftCalculator _nonBlendWorkShiftCalculator;
        private readonly IWorkShiftCalculator _workShiftCalculator;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private IFairnessValueCalculator _fairnessValueCalculator;

        public BestWorkShiftFinder(IScheduleDictionary scheduleDictionary,
                                   IWorkShiftCalculator workShiftCalculator,
                                   INonBlendWorkShiftCalculator nonBlendWorkShiftCalculator,
                                   IFairnessValueCalculator fairnessValueCalculator)
        {
            _scheduleDictionary = scheduleDictionary;
            _workShiftCalculator = workShiftCalculator;
            _nonBlendWorkShiftCalculator = nonBlendWorkShiftCalculator;
            _fairnessValueCalculator = fairnessValueCalculator;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
        public IWorkShiftCalculationResultHolder FindBestMainShift(IPerson person,
                                                                   IList<IShiftProjectionCache> shiftProjectionCaches,
                                                                   IDictionary
                                                                       <IActivity,
                                                                       IDictionary
                                                                       <DateTime, ISkillStaffPeriodDataHolder>>
                                                                       dataHolderDictionary,
                                                                   IDictionary<ISkill, ISkillStaffPeriodDictionary>
                                                                       nonBlendSkillStaffs,
            double lengthFactor, bool useMinimumPersons, bool useMaximumPersons)
        {
            IList<IWorkShiftCalculationResultHolder> allValues =
                new List<IWorkShiftCalculationResultHolder>(shiftProjectionCaches.Count);
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            //double? foundValue = null;
            foreach (IShiftProjectionCache shiftProjection in shiftProjectionCaches)
            {
                double? nonBlendValue = null;
                double projValue = double.MinValue;
                double? thisValue = null;

                if (dataHolderDictionary.Count > 0)
                    projValue = _workShiftCalculator.CalculateShiftValue(shiftProjection.MainShiftProjection,
                                                                         dataHolderDictionary, 1, true, true);

                // temp until we change the calculator to return nullable double
                if (projValue > double.MinValue)
                    thisValue = projValue;

                if (nonBlendSkillStaffs.Count > 0)
                    nonBlendValue = _nonBlendWorkShiftCalculator.CalculateShiftValue(person,
                                                                                     shiftProjection.
                                                                                         MainShiftProjection,
                                                                                     nonBlendSkillStaffs, lengthFactor, useMinimumPersons, useMaximumPersons);
                if (nonBlendValue.HasValue)
                {
                    if (!thisValue.HasValue)
                        thisValue = nonBlendValue.Value;
                    else
                    {
                        thisValue += nonBlendValue.Value;
                    }
                }

                if (thisValue.HasValue)
                {
                    var workShiftFinderResultHolder = new WorkShiftCalculationResultHolder { ShiftProjection = shiftProjection, Value = thisValue.Value };
                    allValues.Add(workShiftFinderResultHolder);

                    if (thisValue > maxValue)
                        maxValue = thisValue.Value;
                    if (thisValue < minValue)
                        minValue = thisValue.Value;
                }
         
            }

            if (maxValue == double.MinValue)
                return null;

            return allValues[0];
        }
    }
}
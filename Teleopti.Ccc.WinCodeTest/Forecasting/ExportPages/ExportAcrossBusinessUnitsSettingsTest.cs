using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;


namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
    [TestFixture]
    public class ExportAcrossBusinessUnitsSettingsTest
    {
        private IExportAcrossBusinessUnitsSettings _target;

        [SetUp]
        public void Setup()
        {
            _target = new ExportAcrossBusinessUnitsSettings();
        }

        //[Test]
        //public void ShouldSetMultisiteSkillSelection()
        //{
        //    var multisiteSkill = SkillFactory.CreateMultisiteSkill("TestMulti", SkillTypeFactory.CreateSkillType(), 15);
        //    var multisiteSkillModel = new MultisiteSkillModel(multisiteSkill.Id.GetValueOrDefault());
        //    var childSkillMappingModel = new ChildSkillMappingModel(SkillFactory.CreateChildSkill("Sub 1", multisiteSkill), multisiteSkill);
        //    var multisiteSkillSelectionModel = new MultisiteSkillSelectionModel{MultisiteSkillModel = multisiteSkillModel};
        //    multisiteSkillSelectionModel.ChildSkillMappingModels.Add(childSkillMappingModel);
            
        //    _target.MultisiteSkillSelectionModels = new List<MultisiteSkillSelectionModel>{multisiteSkillSelectionModel};

        //    Assert.That(_target.MultisiteSkillSelectionModels.Count, Is.EqualTo(1));
        //    Assert.That(_target.MultisiteSkillSelectionModels.First().ChildSkillMappingModels.Count, Is.EqualTo(1));
        //}

        //[Test]
        //public void ShouldGetMultisiteSkillSelection()
        //{
        //    var multisiteSkill = SkillFactory.CreateMultisiteSkill("TestMulti", SkillTypeFactory.CreateSkillType(), 15);
        //    var multisiteSkillModel = new MultisiteSkillModel(multisiteSkill.Id.GetValueOrDefault());
        //    var childSkillMappingModel1 = new ChildSkillMappingModel(SkillFactory.CreateChildSkill("Sub 1", multisiteSkill), multisiteSkill);
        //    var childSkillMappingModel2 = new ChildSkillMappingModel(SkillFactory.CreateChildSkill("Sub 2", multisiteSkill), multisiteSkill);
        //    var multisiteSkillSelectionModel = new MultisiteSkillSelectionModel{MultisiteSkillModel = multisiteSkillModel};
        //    multisiteSkillSelectionModel.ChildSkillMappingModels.Add(childSkillMappingModel1);
        //    multisiteSkillSelectionModel.ChildSkillMappingModels.Add(childSkillMappingModel2);

        //    _target.MultisiteSkillSelectionModels = new List<MultisiteSkillSelectionModel>{multisiteSkillSelectionModel};

        //    Assert.That(_target.MultisiteSkillSelectionModels.First(), Is.EqualTo(multisiteSkillSelectionModel));
        //    Assert.That(_target.MultisiteSkillSelectionModels.First().ChildSkillMappingModels.ToList()[0], Is.EqualTo(childSkillMappingModel1));
        //    Assert.That(_target.MultisiteSkillSelectionModels.First().ChildSkillMappingModels.ToList()[1], Is.EqualTo(childSkillMappingModel2));
        //}

        [Test]
        public void ShouldKeepSelectedPeriod()
        {
            var selectedPeriod = new DateOnlyPeriod(2012, 1, 1, 2012, 2, 1);
            
            _target.Period = selectedPeriod;

            Assert.That(_target.Period, Is.EqualTo(selectedPeriod));
        }
    }
}
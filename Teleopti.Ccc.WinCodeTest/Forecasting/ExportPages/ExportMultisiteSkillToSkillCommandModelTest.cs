using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ExportPages
{
    [TestFixture]
    public class ExportMultisiteSkillToSkillCommandModelTest
    {
        private ExportMultisiteSkillToSkillCommandModel target;
        private Guid multisiteSkillGuid;
        private Guid? sourceSkillGuid;
        private Guid? targetSkillGuid;
        private DateOnlyPeriodDto period;


        [SetUp]
        public void Setup()
        {
            target = new ExportMultisiteSkillToSkillCommandModel();
            multisiteSkillGuid = Guid.NewGuid();
            sourceSkillGuid = Guid.NewGuid();
            targetSkillGuid = Guid.NewGuid();
	        period = new DateOnlyPeriodDto
		        {
			        StartDate = new DateOnlyDto {DateTime = new DateTime(2010, 12, 13)},
			        EndDate = new DateOnlyDto {DateTime = new DateTime(2010, 12, 22)}
		        };

            var multisiteSkillSelectionModel = new MultisiteSkillSelectionModel();
            
            multisiteSkillSelectionModel.MultisiteSkillModel = new MultisiteSkillModel(multisiteSkillGuid);
			IChildSkill source = new ChildSkill("source", "desc", Color.Beige, 15, SkillTypeFactory.CreateSkillType());
            source.SetId(sourceSkillGuid);
			ISkill selected = new Skill("source", "desc", Color.Beige, 15, SkillTypeFactory.CreateSkillType());
            selected.SetId(targetSkillGuid);
            var childSkillMappingModel = new ChildSkillMappingModel(sourceSkillGuid.GetValueOrDefault(),
                                                                    selected.Id.GetValueOrDefault(),
                                                                    selected.BusinessUnit.Name, selected.Name);
            
			multisiteSkillSelectionModel.ChildSkillMappingModels.Add(childSkillMappingModel);
            
            target.MultisiteSkillSelectionModels.Add(multisiteSkillSelectionModel);
            target.Period = period;
        }

        [Test]
        public void ShouldTransformToDto()
        {
            var dto = target.TransformToDto();
            Assert.AreEqual(dto.MultisiteSkillSelection.Count, 1);
            Assert.AreEqual(dto.Period, period);
            Assert.AreEqual(dto.MultisiteSkillSelection.First().MultisiteSkill.Id, multisiteSkillGuid);
            Assert.AreEqual(dto.MultisiteSkillSelection.First().ChildSkillMapping.First().SourceSkill.Id, sourceSkillGuid);
            Assert.AreEqual(dto.MultisiteSkillSelection.First().ChildSkillMapping.First().TargetSkill.Id, targetSkillGuid);
        }

        [Test]
        public void ShouldHaveChildSkillMappings()
        {
            target.HasChildSkillMappings.Should().Be.True();

            target.MultisiteSkillSelectionModels.Clear();

            target.HasChildSkillMappings.Should().Be.False();
        }
    }
}

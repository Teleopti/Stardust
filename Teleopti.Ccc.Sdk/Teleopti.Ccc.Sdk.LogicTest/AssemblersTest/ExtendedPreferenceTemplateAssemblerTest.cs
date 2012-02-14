using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ExtendedPreferenceTemplateAssemblerTest
    {
        private ExtendedPreferenceTemplateAssembler target;
        private IPerson person;
        private MockRepository mocks;
        private IAssembler<IPreferenceRestrictionTemplate, ExtendedPreferenceTemplateDto> restrictionAssembler;
       // private IExtendedPreferenceTemplateRepository extendedPreferenceTemplateRepository; ehh dunno?

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            restrictionAssembler = mocks.StrictMock<IAssembler<IPreferenceRestrictionTemplate, ExtendedPreferenceTemplateDto>>();
           // extendedPreferenceTemplateRepository = mocks.StrictMock<IExtendedPreferenceTemplateRepository>();
            person = mocks.StrictMock<IPerson>(); 
            target = new ExtendedPreferenceTemplateAssembler(person,restrictionAssembler);
        }

        [Test]
        public void VerifyDomainEntityAndDto()
        {
            var template = new ExtendedPreferenceTemplate(person, new PreferenceRestrictionTemplate(), "temptest", Color.SteelBlue);
            using (mocks.Record())
            {
                Expect.Call(restrictionAssembler.DomainEntityToDto(template.Restriction)).Return(
                    new ExtendedPreferenceTemplateDto());
            }
            using (mocks.Playback())
            {
                var dto = target.DomainEntityToDto(template);

                Assert.AreEqual(dto.Name, template.Name);

                Assert.AreEqual(dto.DisplayColor.Blue, template.DisplayColor.B);
                Assert.AreEqual(dto.DisplayColor.Red, template.DisplayColor.R);
                Assert.AreEqual(dto.DisplayColor.Green, template.DisplayColor.G);
                Assert.AreEqual(dto.DisplayColor.Alpha, template.DisplayColor.A);
            }
        }

        [Test]
        public void VerifyDtoToDomainEntity()
        {
            var templateDto = new ExtendedPreferenceTemplateDto();
            var templateRestriction = new PreferenceRestrictionTemplate();
            using (mocks.Record())
            {
                Expect.Call(restrictionAssembler.DtoToDomainEntity(templateDto)).Return(templateRestriction);
            }
            using (mocks.Playback())
            {
                templateDto.DisplayColor = new ColorDto(Color.DodgerBlue);
                templateDto.Name = "Kanin";
                var template = target.DtoToDomainEntity(templateDto);

                Assert.AreEqual("Kanin", template.Name);
                Assert.AreEqual(Color.DodgerBlue.B, template.DisplayColor.B);
                Assert.AreEqual(person, template.Person);
            }
        }
    }
}

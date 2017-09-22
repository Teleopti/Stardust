using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Forecasting
{
    [TestFixture]
    public class CopyToSkillPresenterTest
    {
        private MockRepository mocks;
        private ICopyToSkillView view;
        private ISkill skill;
        private IWorkload workload;
        private CopyToSkillModel model;
        private CopyToSkillPresenter target;
        private ISkillRepository repository;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private ICopyToSkillCommand command;

        [SetUp]
        public void Setup()
        {
            SetupWorkload();

            mocks = new MockRepository();
            view = mocks.StrictMock<ICopyToSkillView>();
            repository = mocks.StrictMock<ISkillRepository>();
            unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            command = mocks.StrictMock<ICopyToSkillCommand>();
            model = new CopyToSkillModel(workload);
            target = new CopyToSkillPresenter(view, model, command, repository, unitOfWorkFactory);
        }

        private void SetupWorkload()
        {
            skill = SkillFactory.CreateSkill("test");
            workload = WorkloadFactory.CreateWorkload(skill);
            workload.SetId(Guid.NewGuid());
        }

        [Test]
        public void ShouldInitializeView()
        {
            var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            using (mocks.Record())
            {
                Expect.Call(() => view.ToggleIncludeQueues(model.IncludeQueues));
                Expect.Call(() => view.ToggleIncludeTemplates(model.IncludeTemplates));
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => view.SetCopyFromText(model.SourceWorkloadName));
                Expect.Call(() => view.AddSkillToList(skill.Name,skill));
                Expect.Call(repository.FindAllWithWorkloadAndQueues()).Return(new[] {skill});
            }

            using (mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void ShouldInitializeViewWithMatchingSkillsOnly()
        {
            var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            var newSkill = SkillFactory.CreateSkill("test2", skill.SkillType, skill.DefaultResolution*2);
            using (mocks.Record())
            {
                Expect.Call(() => view.ToggleIncludeQueues(model.IncludeQueues));
                Expect.Call(() => view.ToggleIncludeTemplates(model.IncludeTemplates));
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => view.SetCopyFromText(model.SourceWorkloadName));
                Expect.Call(() => view.AddSkillToList(skill.Name, skill));
                Expect.Call(repository.FindAllWithWorkloadAndQueues()).Return(new[] { skill, newSkill });
            }

            using (mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void ShouldCloseWhenNoMatchingSkillsAreFound()
        {
            var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            using (mocks.Record())
            {
                Expect.Call(() => view.ToggleIncludeQueues(model.IncludeQueues));
                Expect.Call(() => view.ToggleIncludeTemplates(model.IncludeTemplates));
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => view.SetCopyFromText(model.SourceWorkloadName));
                Expect.Call(() => view.NoMatchingSkillsAvailable());
                Expect.Call(() => view.Close());
                Expect.Call(repository.FindAllWithWorkloadAndQueues()).Return(new ISkill[] {});
            }

            using (mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void ShouldPerformCopy()
        {
            using (mocks.Record())
            {
                command.Execute();
            }

            using (mocks.Playback())
            {
                target.Copy();
            }
        }

        [Test]
        public void ShouldSetValuesFromView()
        {
            var newSkill = SkillFactory.CreateSkill("test2", skill.SkillType, skill.DefaultResolution * 2);
            
            target.SetTargetSkill(newSkill);
            target.ToggleIncludeQueues(!model.IncludeQueues);
            target.ToggleIncludeTemplates(!model.IncludeTemplates);

            model.TargetSkill.Should().Be.SameInstanceAs(newSkill);
            model.IncludeQueues.Should().Be.True();
            model.IncludeTemplates.Should().Be.False();
        }
    }
}

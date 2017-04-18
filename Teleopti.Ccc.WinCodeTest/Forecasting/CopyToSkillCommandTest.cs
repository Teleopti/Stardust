using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Forecasting
{
    [TestFixture]
    public class CopyToSkillCommandTest
    {
        private MockRepository mocks;
        private ICopyToSkillView view;
        private ISkill skill;
        private IWorkload workload;
        private CopyToSkillModel model;
        private ICopyToSkillCommand target;
        private IWorkloadRepository repository;
        private IUnitOfWorkFactory unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            SetupWorkload();

            mocks = new MockRepository();
            view = mocks.StrictMock<ICopyToSkillView>();
            repository = mocks.StrictMock<IWorkloadRepository>();
            unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            model = new CopyToSkillModel(workload);
            target = new CopyToSkillCommand(view, model, repository, unitOfWorkFactory);
        }

        private void SetupWorkload()
        {
            skill = SkillFactory.CreateSkill("test");
            workload = WorkloadFactory.CreateWorkload(skill);
            workload.SetId(Guid.NewGuid());
            workload.AddQueueSource(QueueSourceFactory.CreateQueueSource());

            var customTemplate = workload.TemplateWeekCollection[0].NoneEntityClone();
            customTemplate.Name = "Custom";
            workload.AddTemplate(customTemplate);
        }

        [Test]
        public void ShouldCopyWithQueuesOnly()
        {
            var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            var changes = new List<IRootChangeInfo>();
            using (mocks.Record())
            {
                Expect.Call(() => view.TriggerEntitiesNeedRefresh(changes));
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(unitOfWork.PersistAll()).Return(changes);
                Expect.Call(() => repository.Add(null)).Constraints(
                    Rhino.Mocks.Constraints.Is.Matching(
                        new Predicate<IWorkload>(
                            w => !w.Id.HasValue && w.Name != workload.Name && w.Name.EndsWith(workload.Name) && w.QueueSourceCollection.Count>0 && w.TemplateWeekCollection.Count==7)));
            }

            using (mocks.Playback())
            {
                model.TargetSkill = skill;
                model.IncludeQueues = true;
                model.IncludeTemplates = false;

                target.Execute();
            }
        }

        [Test]
        public void ShouldCopyWithTemplatesOnly()
        {
            var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            var changes = new List<IRootChangeInfo>();
            
            using (mocks.Record())
            {
                Expect.Call(() => view.TriggerEntitiesNeedRefresh(changes));
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(unitOfWork.PersistAll()).Return(changes);
                Expect.Call(() => repository.Add(null)).Constraints(
                    Rhino.Mocks.Constraints.Is.Matching(
                        new Predicate<IWorkload>(
                            w => !w.Id.HasValue && w.Name != workload.Name && w.Name.EndsWith(workload.Name) && w.QueueSourceCollection.Count == 0 && w.TemplateWeekCollection.Count == 8)));
            }

            using (mocks.Playback())
            {
                model.TargetSkill = skill;
                model.IncludeQueues = false;
                model.IncludeTemplates = true;

                target.Execute();
            }
        }

        [Test]
        public void ShouldCopyWithTemplatesAndHandleMergedPeriods()
        {
            var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            var changes = new List<IRootChangeInfo>();

            workload.TemplateWeekCollection[7].MakeOpen24Hours();
            workload.TemplateWeekCollection[7].MergeTemplateTaskPeriods(workload.TemplateWeekCollection[7].TaskPeriodList);
            using (mocks.Record())
            {
                Expect.Call(() => view.TriggerEntitiesNeedRefresh(changes));
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(unitOfWork.PersistAll()).Return(changes);
                Expect.Call(() => repository.Add(null)).Constraints(
                    Rhino.Mocks.Constraints.Is.Matching(
                        new Predicate<IWorkload>(w =>
                            w.TemplateWeekCollection.Count == 8 && w.TemplateWeekCollection[7].TaskPeriodList.Count == 1)));
            }

            using (mocks.Playback())
            {
                model.TargetSkill = skill;
                model.IncludeQueues = false;
                model.IncludeTemplates = true;

                target.Execute();
            }
        }
    }
}

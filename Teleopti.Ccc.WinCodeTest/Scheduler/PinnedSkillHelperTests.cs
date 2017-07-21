using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class PinnedSkillHelperTests :IDisposable
    {
        private PinnedSkillHelper _target;
        private MockRepository _mocks;
        private ISchedulingScreenSettings _currentSchedulingScreenSettings;
        private TabControlAdv _tabControlAdv;
        private ISkill _skill1;
        private TabPageAdv _tab1;
        private TabPageAdv _tab2;
        private TabPageAdv _tab3;
        private SkillTypePhone _skillTypePhone;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _currentSchedulingScreenSettings = _mocks.StrictMock<ISchedulingScreenSettings>();
            _tabControlAdv = new TabControlAdv();
            _skill1 = _mocks.StrictMock<ISkill>();
            _target = new PinnedSkillHelper();
            _tab1 = new TabPageAdv("A First ordinary skill");
            _tab1.Tag = _skill1;
            _tabControlAdv.TabPages.Add(_tab1);
            _skillTypePhone = new SkillTypePhone(new Description("type"), ForecastSource.InboundTelephony);
        }

        
        [Test]
        public void ShouldCheckSettingsForPinned()
        {
            Expect.Call(_currentSchedulingScreenSettings.PinnedSkillTabs).Return(new List<Guid>()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.IsVirtual).Return(false).Repeat.AtLeastOnce();
            Expect.Call(_skill1.SkillType).Return(_skillTypePhone).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Name).Return("skill1").Repeat.AtLeastOnce();
            _mocks.ReplayAll();
            _target.InitialSetup(_tabControlAdv, _currentSchedulingScreenSettings);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldPinSavedSkillFirst()
        {
           var skill2 = _mocks.StrictMock<ISkill>();
            var skill2Id = Guid.NewGuid();
 
            _tab2 = new TabPageAdv("B Second ordinary skill");
            _tab2.Tag = skill2;
            _tabControlAdv.TabPages.Add(_tab2);

            Expect.Call(_currentSchedulingScreenSettings.PinnedSkillTabs).Return(new List<Guid> { skill2Id }).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.IsVirtual).Return(false).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Name).Return("Skill1").Repeat.AtLeastOnce();
            Expect.Call(_skill1.SkillType).Return(_skillTypePhone).Repeat.AtLeastOnce();

            Expect.Call(skill2.Id).Return(skill2Id).Repeat.AtLeastOnce();
            Expect.Call(skill2.IsVirtual).Return(false).Repeat.AtLeastOnce();
            Expect.Call(skill2.SkillType).Return(_skillTypePhone).Repeat.AtLeastOnce();
            Expect.Call(skill2.Name).Return("Skill2").Repeat.AtLeastOnce();
            
            _mocks.ReplayAll();
            Assert.That(_tabControlAdv.TabPages[0].Text, Is.EqualTo("A First ordinary skill"));
            _target.InitialSetup(_tabControlAdv, _currentSchedulingScreenSettings);
            Assert.That(_tabControlAdv.TabPages[0].Text, Is.EqualTo("B Second ordinary skill"));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSortPinnedVirtualFirst()
        {
            var skill2 = _mocks.StrictMock<ISkill>();
            var skill3 = _mocks.StrictMock<ISkill>();
            var skillTypePhone = new SkillTypePhone(new Description("type"), ForecastSource.InboundTelephony);

            _tab2 = new TabPageAdv("B Second ordinary skill");
            _tab2.Tag = skill2;
            _tabControlAdv.TabPages.Add(_tab2);

            _tab3 = new TabPageAdv("Virtual");
            _tab3.Tag = skill3;
            _tabControlAdv.TabPages.Add(_tab3);

            Expect.Call(_currentSchedulingScreenSettings.PinnedSkillTabs).Return(new List<Guid>()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.IsVirtual).Return(false).Repeat.AtLeastOnce();
            Expect.Call(_skill1.SkillType).Return(skillTypePhone).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Name).Return("Skill1").Repeat.AtLeastOnce();

            Expect.Call(skill2.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(skill2.IsVirtual).Return(false).Repeat.AtLeastOnce();
            Expect.Call(skill2.SkillType).Return(skillTypePhone).Repeat.AtLeastOnce();
            Expect.Call(skill2.Name).Return("Skill2").Repeat.AtLeastOnce();

            Expect.Call(skill3.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(skill3.IsVirtual).Return(true).Repeat.AtLeastOnce();
            Expect.Call(skill3.Name).Return("Skill3 virtual").Repeat.AtLeastOnce();

            _mocks.ReplayAll();
            Assert.That(_tabControlAdv.TabPages[0].Text, Is.EqualTo("A First ordinary skill"));
            _target.InitialSetup(_tabControlAdv, _currentSchedulingScreenSettings);
            // virtual sorted first when allunpinned
            Assert.That(_tabControlAdv.TabPages[0].Text, Is.EqualTo("Virtual"));
            _target.PinSlashUnpinTab(_tabControlAdv.TabPages[2]);
            Assert.That(_tabControlAdv.TabPages[0].Text, Is.EqualTo("B Second ordinary skill"));
            _target.PinSlashUnpinTab(_tabControlAdv.TabPages[1]);
            // now virtual first again
            Assert.That(_tabControlAdv.TabPages[0].Text, Is.EqualTo("Virtual"));
           // unpin virtual
            _target.PinSlashUnpinTab(_tabControlAdv.TabPages[0]);
            // should be second now
            Assert.That(_tabControlAdv.TabPages[1].Text, Is.EqualTo("Virtual"));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldRemoveVirtualWhenDeleted()
        {
            Expect.Call(_currentSchedulingScreenSettings.PinnedSkillTabs).Return(new List<Guid>()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.IsVirtual).Return(false).Repeat.AtLeastOnce();
            Expect.Call(_skill1.SkillType).Return(_skillTypePhone).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Name).Return("skill1").Repeat.AtLeastOnce();
            _mocks.ReplayAll();
            _target.InitialSetup(_tabControlAdv, _currentSchedulingScreenSettings);
            Assert.That(_target.NotPinnedSkills.Count, Is.EqualTo(1));
            _target.RemoveVirtualSkill(_skill1);
            Assert.That(_target.NotPinnedSkills.Count, Is.EqualTo(0));
            _mocks.VerifyAll();
            
        }

        [Test]
        public void ShouldAddToUnpinnedWhenNew()
        {
            var skill2 = _mocks.StrictMock<ISkill>();
            Expect.Call(_currentSchedulingScreenSettings.PinnedSkillTabs).Return(new List<Guid>()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.IsVirtual).Return(false).Repeat.AtLeastOnce();
            Expect.Call(_skill1.SkillType).Return(_skillTypePhone).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Name).Return("skill1").Repeat.AtLeastOnce();
            _mocks.ReplayAll();
            _target.InitialSetup(_tabControlAdv, _currentSchedulingScreenSettings);
            Assert.That(_target.NotPinnedSkills.Count, Is.EqualTo(1));
            _target.AddVirtualSkill(skill2);
            Assert.That(_target.NotPinnedSkills.Count, Is.EqualTo(2));
            _mocks.VerifyAll();

        }

        [Test]
        public void ShouldSwitchSkillWhenRenamed()
        {
            var skill2 = _mocks.StrictMock<ISkill>();
            Expect.Call(_currentSchedulingScreenSettings.PinnedSkillTabs).Return(new List<Guid>()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.IsVirtual).Return(false).Repeat.AtLeastOnce();
            Expect.Call(_skill1.SkillType).Return(_skillTypePhone).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Name).Return("skill1").Repeat.AtLeastOnce();
            _mocks.ReplayAll();
            _target.InitialSetup(_tabControlAdv, _currentSchedulingScreenSettings);
            Assert.That(_target.NotPinnedSkills.Contains(_skill1), Is.True);
            _target.ReplaceOldWithNew(skill2, _skill1);
            Assert.That(_target.NotPinnedSkills.Contains(_skill1), Is.False);
            Assert.That(_target.NotPinnedSkills.Contains(skill2), Is.True);
           
            _mocks.VerifyAll();

        }

        [Test]
        public void ShouldSwitchPinnedSkillWhenRenamed()
        {
            var skill2 = _mocks.StrictMock<ISkill>();
            Expect.Call(_currentSchedulingScreenSettings.PinnedSkillTabs).Return(new List<Guid>()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.IsVirtual).Return(false).Repeat.AtLeastOnce();
            Expect.Call(_skill1.SkillType).Return(_skillTypePhone).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            Expect.Call(_skill1.Name).Return("skill1").Repeat.AtLeastOnce();
            Expect.Call(skill2.Id).Return(Guid.NewGuid()).Repeat.AtLeastOnce();
            _mocks.ReplayAll();
            _target.InitialSetup(_tabControlAdv, _currentSchedulingScreenSettings);
            _target.PinSlashUnpinTab(_tabControlAdv.TabPages[0]);
            Assert.That(_target.PinnedSkills.Contains(_skill1), Is.True);
            _target.ReplaceOldWithNew(skill2, _skill1);
            Assert.That(_target.PinnedSkills.Contains(_skill1), Is.False);
            Assert.That(_target.PinnedSkills.Contains(skill2), Is.True);

            _mocks.VerifyAll();

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_tab1 != null)
                {
                    _tab1.Dispose();
                    _tab1 = null;
                }
                if (_tab2 != null)
                {
                    _tab2.Dispose();
                    _tab2 = null;
                }
                if (_tab3 != null)
                {
                    _tab3.Dispose();
                    _tab3 = null;
                }

                if (_tabControlAdv != null)
                {
                    _tabControlAdv.Dispose();
                    _tabControlAdv = null;
                }
                
            }

        }
    }

    
}
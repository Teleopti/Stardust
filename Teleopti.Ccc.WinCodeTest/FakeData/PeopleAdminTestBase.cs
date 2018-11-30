using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.FakeData
{
   public class PeopleAdminTestBase
   {

       [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "BLUETEAM")]
        public const string BLUETEAM = "Bona Team";
       [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "REDTEAM")]
        public const string REDTEAM = "Nana Team";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "GREENTEAM")]
        public const string GREENTEAM = "Kana Team";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "BLUESITE")]
        public const string BLUESITE = "Bona Site";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "REDSITE")]
        public const string REDSITE = "Nana Site";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "GREENSITE")]
        public const string GREENSITE = "Kana Site";
        
        public SiteTeamModel SiteTeam1
        {
            get;
            set;
        }
        public SiteTeamModel SiteTeam2
        {
            get;
            set;
        }
        public SiteTeamModel SiteTeam3
        {
            get;
            set;
        }

        public ITeam TeamBlue
        {
            get;
            set;
        }

        public ITeam TeamRed
        {
            get;
            set;
        }

        public ITeam TeamGreen
        {
            get;
            set;
        }


        public IPersonSkill PersonSkill1
        {
            get;
            set;
        }

        public IPersonSkill PersonSkill2
        {
            get;
            set;
        }

        public IPersonSkill PersonSkill3
        {
            get;
            set;
        }

       private IList<SiteTeamModel> _siteTeamAdapterCollection = new List<SiteTeamModel>();
       private IList<IExternalLogOn> _externalLogOnCollection = new List<IExternalLogOn>();
       private IList<IPersonSkill> _personSkillCollection = new List<IPersonSkill>();
       
       public ISkill Skill1
       {
           get;
           set;
       }

       public ISkill Skill2
       {
           get;
           set;
       }

       public ISkill Skill3
       {
           get;
           set;
       }

       public IExternalLogOn ExternalLogOn1
       {
           get;
           set;
       }

       public IExternalLogOn ExternalLogOn2
       {
           get;
           set;
       }

       public IExternalLogOn ExternalLogOn3
       {
           get;
           set;
       }

       public IList<SiteTeamModel> SiteTeamAdapterCollection
       {
           get { return _siteTeamAdapterCollection; }
       }

       public IList<IExternalLogOn> ExternalLogOnCollection
       {
           get { return _externalLogOnCollection; }
       }

       public IList<IPersonSkill> PersonSkillCollection
       {
           get { return _personSkillCollection; }
       }

       public void CreateSiteTeamCollection()
       {
           TeamBlue = TeamFactory.CreateTeam(BLUETEAM, BLUESITE);
           TeamRed = TeamFactory.CreateTeam(REDTEAM, REDSITE);
           TeamGreen = TeamFactory.CreateTeam(GREENTEAM, GREENSITE);

           SiteTeam1 = EntityConverter.ConvertToOther<ITeam, SiteTeamModel>(TeamBlue);
           SiteTeam2 = EntityConverter.ConvertToOther<ITeam, SiteTeamModel>(TeamRed);
           SiteTeam3 = EntityConverter.ConvertToOther<ITeam, SiteTeamModel>(TeamGreen);

           _siteTeamAdapterCollection.Add(SiteTeam1);
           _siteTeamAdapterCollection.Add(SiteTeam2);
           _siteTeamAdapterCollection.Add(SiteTeam3);
       }

       public void CreateExternalLogOnCollection()
       {
           ExternalLogOn1 = ExternalLogOnFactory.CreateExternalLogOn();
           ExternalLogOn2 = ExternalLogOnFactory.CreateExternalLogOn();
           ExternalLogOn3 = ExternalLogOnFactory.CreateExternalLogOn();

           ExternalLogOn1.DataSourceId = 1;
           ExternalLogOn2.DataSourceId = 2;
           ExternalLogOn3.DataSourceId = 3;

           _externalLogOnCollection.Clear();
           _externalLogOnCollection.Add(ExternalLogOn1);
           _externalLogOnCollection.Add(ExternalLogOn2);
           _externalLogOnCollection.Add(ExternalLogOn3);
       }

       public void CreatePersonSkillCollection()
       {
           PersonSkill1 = PersonSkillFactory.CreatePersonSkill("_skill1", 1);
           PersonSkill2 = PersonSkillFactory.CreatePersonSkill("_skill2", 1);
           PersonSkill3 = PersonSkillFactory.CreatePersonSkill("_skill3", 1);

           _personSkillCollection.Add(PersonSkill1);
           _personSkillCollection.Add(PersonSkill2);
           _personSkillCollection.Add(PersonSkill3);
       }

       public void CreateSkills()
       {
           Skill1 = SkillFactory.CreateSkill("_skill1");
           Skill2 = SkillFactory.CreateSkill("_skill2");
           Skill3 = SkillFactory.CreateSkill("_skill3");
       }

       public static DateOnly DateOnly1
       {
           get { return new DateOnly(2000, 1, 1); } 
       }

       public static DateOnly DateOnly2
       {
           get { return new DateOnly(2005, 1, 1); }
       }

       public static DateOnly DateOnly3
       {
           get { return new DateOnly(2008, 1, 1); }
       }
    }
}

﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting
{

    /// <summary>
    /// Handles word replacement for non Telephony skills
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-08-22
    /// </remarks>
    public class TextManager
    {
        private readonly IDictionary<string, string> _wordDictionary = new Dictionary<string, string>();

        public TextManager(ISkillType skillType)
        {
            GetWords(skillType);
        }

        public IDictionary<string, string> WordDictionary
        {
            get { return _wordDictionary; }
        }
        private void GetWords(ISkillType skillType)
        {
            BaseSetUp();

            switch (skillType.ForecastSource)
            {
                case ForecastSource.Email:
                    EmailSetUp();
                    break;
                case ForecastSource.Facsimile:
                    FacsimileSetUp();
                    break;
                case ForecastSource.Backoffice:
                    BackofficeSetUp();
                    break;
                case ForecastSource.Time:
                    BackofficeSetUp();
                    break;
                default:
                    PhoneSetUp();
                    break;
            }
        }

        private void BaseSetUp()
        {
            _wordDictionary.Add("TotalAverageTaskTime", UserTexts.Resources.TotalHandlingTime);
            _wordDictionary.Add("AverageTaskTime", UserTexts.Resources.HandlingTime);
            _wordDictionary.Add("ForecastedIncomingDemand", UserTexts.Resources.HoursInc);
            _wordDictionary.Add("ForecastedDistributedDemand", UserTexts.Resources.Hours);
            _wordDictionary.Add("ForecastedDistributedDemandWithShrinkage", UserTexts.Resources.HoursIncShrinkage);
            _wordDictionary.Add("ForecastedIncomingDemandWithShrinkage", UserTexts.Resources.HoursIncShrinkage);
            _wordDictionary.Add("HandledWithin", UserTexts.Resources.HandledWithin);
            _wordDictionary.Add("AgentsInc", UserTexts.Resources.AgentsInc);
            _wordDictionary.Add("AgentsIncWithShrinkage", UserTexts.Resources.AgentsIncWithShrinkage);
            _wordDictionary.Add("OriginalAverageTaskTime", UserTexts.Resources.OriginalHandlingTime);
            _wordDictionary.Add("AverageTalkTime", UserTexts.Resources.AverageHandlingTime);
            _wordDictionary.Add("ValidatedAverageTaskTime", UserTexts.Resources.ValidatedAverageHandlingTime);
            _wordDictionary.Add("TotalStatisticAverageTaskTime", UserTexts.Resources.HandlingTime);
            _wordDictionary.Add("ForecastedHandlingTime", UserTexts.Resources.ForecastedHandlingTime);//
            _wordDictionary.Add("ActualHandlingTime", UserTexts.Resources.ActualHandlingTime);//
        }

        private void EmailSetUp()
        {
            _wordDictionary.Add("Tasks", UserTexts.Resources.Emails);
            _wordDictionary.Add("TotalTasks", UserTexts.Resources.TotalEmails);
            _wordDictionary.Add("TotalAverageAfterTaskTime", UserTexts.Resources.TotalAEW);
            _wordDictionary.Add("AverageAfterTaskTime", UserTexts.Resources.AEW);
            _wordDictionary.Add("CampaignTaskTime", UserTexts.Resources.CampaignEmailsHandlingtimePercent);
            _wordDictionary.Add("CampaignAfterTaskTime", UserTexts.Resources.CampaignAEWPercent);
            _wordDictionary.Add("CampaignTasks", UserTexts.Resources.CampaignEmailsPercent);
            _wordDictionary.Add("TotalStatisticCalculatedTasks", UserTexts.Resources.CalculatedEmails);
            _wordDictionary.Add("TotalStatisticAbandonedTasks", UserTexts.Resources.AbandonedEmails);
            _wordDictionary.Add("TotalStatisticAnsweredTasks", UserTexts.Resources.AnsweredEmails);
            _wordDictionary.Add("TotalStatisticAverageAfterTaskTime", UserTexts.Resources.AEW);
            _wordDictionary.Add("TaskIndex", UserTexts.Resources.IndexEmails);
            _wordDictionary.Add("TalkTimeIndex", UserTexts.Resources.IndexHandlingTime);
            _wordDictionary.Add("AfterTalkTimeIndex", UserTexts.Resources.IndexAEW);
            _wordDictionary.Add("OriginalTasks", UserTexts.Resources.OriginalOfferedEmails);
            _wordDictionary.Add("AverageTasks", UserTexts.Resources.AverageEmails);
            _wordDictionary.Add("OriginalAverageAfterTaskTime", UserTexts.Resources.OriginalAEW);
            _wordDictionary.Add("AverageAfterWorkTime", UserTexts.Resources.AverageAEW);
            _wordDictionary.Add("ValidatedAverageAfterTaskTime", UserTexts.Resources.ValidatedAEW);
            _wordDictionary.Add("ValidatedTasks", UserTexts.Resources.ValidatedEmails);
            _wordDictionary.Add("ForecastedTasks", UserTexts.Resources.ForecastedEmails);
            _wordDictionary.Add("OfferedTasks", UserTexts.Resources.OfferedEmails);
            _wordDictionary.Add("DeviationCallsColon", UserTexts.Resources.DeviationEmailsColon);
            _wordDictionary.Add("DeviationTalkTimeColon", UserTexts.Resources.DeviationHandlingTimeColon);
            _wordDictionary.Add("DeviationACWColon", UserTexts.Resources.DeviationAEWColon);

        }
        private void BackofficeSetUp()
        {
            _wordDictionary.Add("Tasks", UserTexts.Resources.Tasks);
            _wordDictionary.Add("TotalTasks", UserTexts.Resources.TotalTasks);
            _wordDictionary.Add("TotalAverageAfterTaskTime", UserTexts.Resources.TotalATW);
            _wordDictionary.Add("AverageAfterTaskTime", UserTexts.Resources.ATW);
            _wordDictionary.Add("CampaignTaskTime", UserTexts.Resources.CampaignTaskPercent);
            _wordDictionary.Add("CampaignAfterTaskTime", UserTexts.Resources.CampaignATWPercent);
            _wordDictionary.Add("CampaignTasks", UserTexts.Resources.CampaignTaskPercent);
            _wordDictionary.Add("TotalStatisticCalculatedTasks", UserTexts.Resources.CalculatedTasks);
            _wordDictionary.Add("TotalStatisticAbandonedTasks", UserTexts.Resources.AbandonedTasks);
            _wordDictionary.Add("TotalStatisticAnsweredTasks", UserTexts.Resources.AnsweredTasks);
            _wordDictionary.Add("TotalStatisticAverageAfterTaskTime", UserTexts.Resources.ATW);
            _wordDictionary.Add("TaskIndex", UserTexts.Resources.IndexTasks);
            _wordDictionary.Add("TalkTimeIndex", UserTexts.Resources.IndexHandlingTime);
            _wordDictionary.Add("AfterTalkTimeIndex", UserTexts.Resources.IndexATW);
            _wordDictionary.Add("OriginalTasks", UserTexts.Resources.OriginalOfferedTasks);
            _wordDictionary.Add("AverageTasks", UserTexts.Resources.AverageTasks);
            _wordDictionary.Add("OriginalAverageAfterTaskTime", UserTexts.Resources.OriginalATW);
            _wordDictionary.Add("AverageAfterWorkTime", UserTexts.Resources.AverageATW);
            _wordDictionary.Add("ValidatedAverageAfterTaskTime", UserTexts.Resources.ValidatedATW);
            _wordDictionary.Add("ValidatedTasks", UserTexts.Resources.ValidatedTasks);
            _wordDictionary.Add("ForecastedTasks", UserTexts.Resources.ForecastedTasks);
            _wordDictionary.Add("OfferedTasks", UserTexts.Resources.OfferedTasks);
            _wordDictionary.Add("DeviationCallsColon", UserTexts.Resources.DeviationTasksColon);
            _wordDictionary.Add("DeviationTalkTimeColon", UserTexts.Resources.DeviationHandlingTimeColon);
            _wordDictionary.Add("DeviationACWColon", UserTexts.Resources.DeviationATWColon);
        }
        private void FacsimileSetUp()
        {
            _wordDictionary.Add("Tasks", UserTexts.Resources.Facsimiles);
            _wordDictionary.Add("TotalTasks", UserTexts.Resources.TotalFacsimiles);
            _wordDictionary.Add("TotalAverageAfterTaskTime", UserTexts.Resources.TotalAFW);
            _wordDictionary.Add("AverageAfterTaskTime", UserTexts.Resources.AFW);
            _wordDictionary.Add("CampaignTaskTime", UserTexts.Resources.CampaignFacsimilesHandlingtime);
            _wordDictionary.Add("CampaignAfterTaskTime", UserTexts.Resources.CampaignAFWPercent);
            _wordDictionary.Add("CampaignTasks", UserTexts.Resources.CampaignFacsimilesPercent);
            _wordDictionary.Add("TotalStatisticCalculatedTasks", UserTexts.Resources.CalculatedFacsimiles);
            _wordDictionary.Add("TotalStatisticAbandonedTasks", UserTexts.Resources.AbandonedFacsimiles);
            _wordDictionary.Add("TotalStatisticAnsweredTasks", UserTexts.Resources.AnsweredFacsimiles);
            _wordDictionary.Add("TotalStatisticAverageAfterTaskTime", UserTexts.Resources.AFW);
            _wordDictionary.Add("TaskIndex", UserTexts.Resources.IndexFacsimiles);
            _wordDictionary.Add("TalkTimeIndex", UserTexts.Resources.IndexHandlingTime);
            _wordDictionary.Add("AfterTalkTimeIndex", UserTexts.Resources.IndexAFW);
            _wordDictionary.Add("OriginalTasks", UserTexts.Resources.OriginalOfferedFacsimiles);
            _wordDictionary.Add("AverageTasks", UserTexts.Resources.AverageFacsimiles);
            _wordDictionary.Add("OriginalAverageAfterTaskTime", UserTexts.Resources.OriginalAFW);
            _wordDictionary.Add("AverageAfterWorkTime", UserTexts.Resources.AverageAFW);
            _wordDictionary.Add("ValidatedAverageAfterTaskTime", UserTexts.Resources.ValidatedAFW);
            _wordDictionary.Add("ValidatedTasks", UserTexts.Resources.ValidatedFacsimiles);
            _wordDictionary.Add("ForecastedTasks", UserTexts.Resources.ForecastedFacsimiles);
            _wordDictionary.Add("OfferedTasks", UserTexts.Resources.OfferedFacsimiles);
            _wordDictionary.Add("DeviationCallsColon", UserTexts.Resources.DeviationFacsimilesColon);
            _wordDictionary.Add("DeviationTalkTimeColon", UserTexts.Resources.DeviationHandlingTimeColon);
            _wordDictionary.Add("DeviationACWColon", UserTexts.Resources.DeviationAFWColon);
        }
        private void PhoneSetUp()
        {
            _wordDictionary.Add("Tasks", UserTexts.Resources.Calls);
            _wordDictionary.Add("TotalTasks", UserTexts.Resources.TotalCalls);
            _wordDictionary.Add("TotalAverageAfterTaskTime", UserTexts.Resources.TotalACW);
            _wordDictionary.Add("AverageAfterTaskTime", UserTexts.Resources.ACW);
            _wordDictionary.Add("CampaignTaskTime", UserTexts.Resources.CampaignTalkTimePercentSign);
            _wordDictionary.Add("CampaignAfterTaskTime", UserTexts.Resources.CampaignACWPercentSign);
            _wordDictionary.Add("CampaignTasks", UserTexts.Resources.CampaignCallsPercentSign);
            _wordDictionary.Add("TotalStatisticCalculatedTasks", UserTexts.Resources.CalculatedCalls);
            _wordDictionary.Add("TotalStatisticAbandonedTasks", UserTexts.Resources.AbandonedCalls);
            _wordDictionary.Add("TotalStatisticAnsweredTasks", UserTexts.Resources.AnsweredCalls);
            _wordDictionary.Add("TotalStatisticAverageAfterTaskTime", UserTexts.Resources.ACW);
            _wordDictionary.Add("TaskIndex", UserTexts.Resources.IndexCalls);
            _wordDictionary.Add("TalkTimeIndex", UserTexts.Resources.IndexHandlingTime);
            _wordDictionary.Add("AfterTalkTimeIndex", UserTexts.Resources.IndexACW);
            _wordDictionary.Add("OriginalTasks", UserTexts.Resources.OriginalOfferedCalls);
            _wordDictionary.Add("AverageTasks", UserTexts.Resources.AverageCalls);
            _wordDictionary.Add("OriginalAverageAfterTaskTime", UserTexts.Resources.OriginalACW);
            _wordDictionary.Add("AverageAfterWorkTime", UserTexts.Resources.AverageACW);
            _wordDictionary.Add("ValidatedAverageAfterTaskTime", UserTexts.Resources.ValidatedACW);
            _wordDictionary.Add("ValidatedTasks", UserTexts.Resources.ValidatedCalls);
            _wordDictionary.Add("ForecastedTasks", UserTexts.Resources.ForecastedCalls);
            _wordDictionary.Add("OfferedTasks", UserTexts.Resources.OfferedCalls);
            _wordDictionary.Add("DeviationCallsColon", UserTexts.Resources.DeviationCallsColon);
            _wordDictionary.Add("DeviationTalkTimeColon", UserTexts.Resources.DeviationHandlingTimeColon);
            _wordDictionary.Add("DeviationACWColon", UserTexts.Resources.DeviationACWColon);
        }

    
    }
}
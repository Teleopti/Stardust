﻿using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Forecasting.Export;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ImportForecastFromFileHandler : IHandle<ImportForecastsFileToSkillEvent>
	{
		private readonly IComponentContext _componentContext;
		private readonly IJobResultFeedback _feedback;
		private Action<string> _sendProgress;

		public ImportForecastFromFileHandler(IComponentContext componentContext,IJobResultFeedback feedback)
		{
			_componentContext = componentContext;
			_feedback = feedback;
		}

		private void feedbackFeedbackChanged(object sender, FeedbackEventArgs e)
		{
			if(!string.IsNullOrEmpty(e.JobResultDetail.Message))
				_sendProgress(e.JobResultDetail.Message);
		}

		public void Handle(ImportForecastsFileToSkillEvent parameters, 
			CancellationTokenSource cancellationTokenSource, 
			Action<string> sendProgress, 
			ref IEnumerable<object> returnObjects)
		{
			_feedback.FeedbackChanged += feedbackFeedbackChanged;
			_sendProgress = sendProgress;
			var theRealOne = _componentContext.Resolve<IHandleEvent<ImportForecastsFileToSkillEvent>>();
			theRealOne.Handle(parameters);
			_feedback.FeedbackChanged -= feedbackFeedbackChanged;
		}
	}
}
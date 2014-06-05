
define([
		'knockout',
		'progressitem-count',
		'addfulldayabsence/iteration',
		'result',
		'messagebroker'
	], function(
		ko,
		ProgressItemCountViewModel,
		Iteration,
		ResultViewModel,
		messagebroker
	) {
		return function (readModelName, isNotificationApplicable, text) {

			var self = this;
			var startPromise = messagebroker.start();
			var readModelSubscription;
			var result;
			
			this.Name = "Add full day absence -> " + readModelName;
			this.Text = text;

			this.Configuration = ko.observable();

			this.ConfigurationObject = ko.computed(function() {
				try {
					return JSON.parse(self.Configuration());
				} catch(e) {
					return undefined;
				}
			});
			
			this.Iterations = ko.computed(function() {

				var configuration = self.ConfigurationObject();
				if (!configuration)
					return;
				
				var startDate = moment(configuration.DateRange.From);
				var endDate = moment(configuration.DateRange.To);
				var numberOfDays = endDate.diff(startDate, 'days') + 1;
				var personIds = configuration.PersonIds;

				var iterations = [];

				for (var i = 0; i < personIds.length; i++) {
					var personId = personIds[i];
					var date = startDate.clone().subtract('days', 1);

					for (var j = 0; j < numberOfDays; j++) {
						date.add('days', 1);

						iterations.push(new Iteration({
							AbsenceId: configuration.AbsenceId,
							PersonId: personId,
							Date: date.clone(),
							ReadModelUpdated: function() {
								progressItemReadModel.Success();
								calculateRunDone();
							},
							ReadModelUpdateFailed: function() {
								progressItemReadModel.Failure();
								calculateRunDone();
							},
							IsApplicableNotification: isNotificationApplicable
						}));

						if (iterations.length > 2000)
							return undefined;
					}
				}

				return iterations;
			});

			this.IterationsExpected = ko.computed(function () {
				var iterations = self.Iterations();
				if (iterations)
					return iterations.length;
				else
					return 0;
			});

			var progressItemReadModel = new ProgressItemCountViewModel(
				readModelName,
				ko.computed(function() {
					return self.IterationsExpected();
				})
			);
			
			this.ProgressItems = [
				progressItemReadModel
			];
			
			this.ConfigurationError = ko.computed(function() {
				if (!self.ConfigurationObject())
					return "Could not parse configuration";
				if (!self.Iterations())
					return "Too many combinations found";
				if (self.Iterations().length == 0)
					return "No combinations found";
				return undefined;
			});
			
			var loadDefaultConfiguration = function () {
				var absenceId;
				var personId;

				var promise1 = $.ajax({
					url: 'Configuration/GetAAbsenceId',
					success: function (data, textStatus, jqXHR) {
						absenceId = data;
					}
				});

				var promise2 = $.ajax({
					url: 'Configuration/GetAPersonId',
					success: function (data, textStatus, jqXHR) {
						personId = data;
					}
				});

				$.when(promise1, promise2).done(function () {
					var date = moment().format('YYYY-MM-DD');
					var configurationObject = {
						AbsenceId: absenceId,
						PersonIds: [
							personId
						],
						DateRange: {
							From: date,
							To: date
						}
					};
					var configuration = JSON.stringify(configurationObject, null, 4);
					self.Configuration(configuration);
				});
			};
			loadDefaultConfiguration();
			
			var calculateRunDone = function () {
				var calculatedInterationsDone = progressItemReadModel.Count();
				if (calculatedInterationsDone > result.IterationsDone()) {
					result.IterationsDone(calculatedInterationsDone);
					if (result.IterationsDone() >= self.IterationsExpected()) {
						result.RunDone(true);
						result = null;
						messagebroker.unsubscribe(readModelSubscription);
						readModelSubscription = null;
					}
				}
			};
			
			this.Run = function () {

				var iterations = self.Iterations();
				
				progressItemReadModel.Reset();
				result = new ResultViewModel();

				startPromise.done(function () {
					
					readModelSubscription = messagebroker.subscribe({
						domainType: 'I' + readModelName,
						callback: function (notification) {
							$.each(iterations, function (i, e) {
								if (e.NotifyReadModelChanged(notification))
									return false;
							});
						}
					});
					
					$.when(
						readModelSubscription.promise
					).done(function() {

						$.each(iterations, function(i, e) {
							e.Start();
						});

						var commandsSentPromises = $.map(iterations, function (e) {
							return e.AllCommandsCompletedPromise;
						});
						$.when.apply($, commandsSentPromises).then(function () {
							result.CommandsDone(true);
						});

					});

				});

				return result;
			};
		};
	});
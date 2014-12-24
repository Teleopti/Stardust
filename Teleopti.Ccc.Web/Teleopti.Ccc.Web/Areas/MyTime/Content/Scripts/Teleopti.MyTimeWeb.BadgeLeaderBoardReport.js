Teleopti.MyTimeWeb.BadgeLeaderBoardReport = (function () {
	var vm;
	function BadgeLeaderBoardReportViewModel() {
		var self = this;
		self.currentDate = ko.observable(getDate());
		self.agentBadges = ko.observableArray();
		self.availableOptions = ko.observableArray();
		self.selectedOption = ko.observable();
		self.showOptions = ko.observable(false);

		self.loadData = function() {

			$.ajax({
				url: 'MyTime/BadgeLeaderBoardReport/Overview',
				dataType: 'json',
				data: { date: self.currentDate().clone().utc().toDate().toJSON(), selectedOption: self.selectedOption() },
				success: function (data) {
					$.each(data.Agents, function (index, item) {
						var badgeViewModel = new BadgeViewModel(index, item);
						self.agentBadges.push(badgeViewModel);
					});
				}
			});
		}

		self.loadOptions = function() {
			$.ajax({
				url: 'MyTime/Team/OptionsForLeaderboard',
				dataType: 'json',
				data: { date: self.currentDate().clone().utc().toDate().toJSON() },
				success: function (data) {
					if (data.length == 0) {
						self.showOptions(false);
					} else {
						self.showOptions(true);
						self.availableOptions(data);
						self.selectedOption(data[0].id);
						self.loadData();
					}
				}
			});
		}


	}

	function BadgeViewModel(index, data) {
		var self = this;

		self.name = data.AgentName;
		self.gold = data.Gold;
		self.silver = data.Silver;
		self.bronze = data.Bronze;
		self.rank = ++index;
	}

	function bindData() {
		vm = new BadgeLeaderBoardReportViewModel();
		var elementToBind = $('.BadgeLeaderBoardReport')[0];
		ko.applyBindings(vm, elementToBind);
	}
	
	function getDate() {
		var date = Teleopti.MyTimeWeb.Portal.ParseHash().dateHash;
		if (date != '') {
			return moment(date, "YYYYMMDD");
		} else {
			return moment(new Date(new Date().getTeleoptiTime())).add('days', -1).startOf('day');
		}
	}

	return {
		Init: function () {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('BadgeLeaderBoardReport/Index',
									Teleopti.MyTimeWeb.BadgeLeaderBoardReport.BadgeLeaderBoardReportPartialInit, Teleopti.MyTimeWeb.BadgeLeaderBoardReport.BadgeLeaderBoardReportPartialDispose);
		},
		BadgeLeaderBoardReportPartialInit: function () {			
			if (!$('.BadgeLeaderBoardReport').length) {
				return;
			}
			bindData();
			vm.loadOptions();
			
		},
		BadgeLeaderBoardReportPartialDispose: function () {			

		}
	};
})(jQuery);
Teleopti.MyTimeWeb.BadgeLeaderBoardReport = (function () {
	var vm;
	function BadgeLeaderBoardReportViewModel(loadDataMethod, date) {
		var self = this;

		self.agentBadges = ko.observableArray();

		loadDataMethod(date);
	}

	function BadgeViewModel(data) {
		var self = this;

		self.name = data.AgentName;
		self.gold = data.Gold;
		self.silver = data.Silver;
		self.bronze = data.Bronze;
	}

	function loadData(date) {
		
		$.ajax({
			url: 'MyTime/BadgeLeaderBoardReport/Overview',
			dataType: 'json',
			data: { date: date.clone().utc().toDate().toJSON() },
			success: function (data) {
				$.each(data.Agents, function (index, item) {
					var badgeViewModel = new BadgeViewModel(item);
					vm.agentBadges.push(badgeViewModel);
				});
			}

		});
	}

	function bindData() {
		vm = new BadgeLeaderBoardReportViewModel(loadData, getDate());
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
		},
		BadgeLeaderBoardReportPartialDispose: function () {			
		},
		ForDay: function(date) {
			loadData(date);
		}		
	};
})(jQuery);
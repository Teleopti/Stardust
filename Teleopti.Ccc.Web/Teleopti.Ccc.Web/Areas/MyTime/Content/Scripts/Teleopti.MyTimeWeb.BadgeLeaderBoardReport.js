Teleopti.MyTimeWeb.BadgeLeaderBoardReport = (function () {
	var vm;
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var tempBadgeComparator = { Gold: '0', Silver: '0', Bronze: '0' };

	function BadgeLeaderBoardReportViewModel() {
		var self = this;
		
		self.currentDate = ko.observable(getDate());
		self.agentBadges = ko.observableArray();
		self.availableOptions = ko.observableArray();
		self.selectedOptionId = ko.observable();
		self.selectedOptionType = -1;
		self.showOptions = ko.observable(false);
		self.isLoading = ko.observable(false);
		
		self.loadData = function() {
			self.agentBadges([]);
			self.isLoading(true);
			ajax.Ajax({
				url: 'BadgeLeaderBoardReport/Overview',
				dataType: 'json',
				data: {
					Date: self.currentDate().clone().utc().toDate().toJSON(),
					Type: self.selectedOptionType,
					SelectedId: self.selectedOptionId()
				},
				success: function (data) {
					self.isLoading(false);
					if(data.Agents[0])
						UpdateBadgeComparator(data.Agents[0]);
					$.each(data.Agents, function (index, item) {
						var badgeViewModel = new BadgeViewModel(index, item);
						self.agentBadges.push(badgeViewModel);
					});
				}
			});
		}

		self.loadOptions = function() {
			self.isLoading(true);
			ajax.Ajax({
				url: 'Team/OptionsForLeaderboard',
				dataType: 'json',
				data: { date: self.currentDate().clone().utc().toDate().toJSON() },
				success: function (data) {
					self.isLoading(false);
					if (data.length === 1) {
						//MyOwn
						self.showOptions(false);
						self.selectedOptionType = 0;
						self.loadData();
					} else {
						self.showOptions(true);
						self.availableOptions(data);
						self.selectedOptionId(data[0].id);
					}
				}
			});
		}

		self.selectedOptionId.subscribe(function (value) {
			ko.utils.arrayForEach(self.availableOptions(), function(option) {
				if (option.id === value)
					self.selectedOptionType = option.type;
			});
			if (value)
				self.loadData();
		});

	}

	function BadgeComparator(data) {

		if (data.Gold == tempBadgeComparator.Gold)
			if (data.Silver == tempBadgeComparator.Silver)
				if (data.Bronze == tempBadgeComparator.Bronze) {
					return 0;
				} else {
					UpdateBadgeComparator(data);
					return 1;
			} else {
				UpdateBadgeComparator(data);
				return 1;
		} else {
			UpdateBadgeComparator(data);
			return 1;
		}
	}

	function UpdateBadgeComparator(data) {
		tempBadgeComparator.Gold = data.Gold;
		tempBadgeComparator.Silver = data.Silver;
		tempBadgeComparator.Bronze = data.Bronze;
	}

	function BadgeViewModel(index, data) {
		var self = this;

		self.name = data.AgentName;
		self.gold = data.Gold;
		self.silver = data.Silver;
		self.bronze = data.Bronze;
		self.rank = index == 0 ? 1 : BadgeComparator(data) + index;
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

	function featureCheck() {
		var toggleEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_OrganisationalBasedLeaderboard_31184");
		vm.showOptions(toggleEnabled);
		if (toggleEnabled) {
			vm.loadOptions();
		} else {
			vm.selectedOptionType = 3;
			vm.loadData();
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
			featureCheck();

		},
		BadgeLeaderBoardReportPartialDispose: function () {
			ajax.AbortAll();

		}
	};
})(jQuery);
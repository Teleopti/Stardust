Teleopti.MyTimeWeb.BadgeLeaderBoardReport = (function () {
	var vm;
	var ajax = new Teleopti.MyTimeWeb.Ajax();

	function BadgeLeaderBoardReportViewModel(weekStart) {
		var self = this;

		self.currentDate = ko.observable(getDate());
		self.agentBadges = ko.observableArray();
		self.availableOptions = ko.observableArray();
		self.selectedOptionId = ko.observable();
		self.selectedOptionType = -1;

		self.showOptions = ko.observable(false);
		self.isLoading = ko.observable(false);
		self.showCalendar = false;
		
		self.dateFormat = $('.format-field')[0].value;
		self.rollingPeriod = $('.rolling-type-field')[0].value
		self.showCalendar = self.rollingPeriod && self.rollingPeriod !== '0';
		self.selectedDate = ko.observable(moment().startOf("day"));

		self.displayDate = ko.computed(function () {
			if (self.rollingPeriod === '0' || !self.rollingPeriod) return;

			return Teleopti.MyTimeWeb.Common.FormatDatePeriod(
				getStartDate(),
				getEndDate());
		}, self);

		self.previous = function () {
			increasePeriod(-1);
		}

		self.next = function () {
			increasePeriod(1);
		}

		self.selectedDate.subscribe(function (value) {
			if (!self.oldValue) {
				if (value) {
					self.loadData();
					self.oldValue = value;
				}
			} else if (self.oldValue.format('YYMMDD') != value.format('YYMMDD')) {
				self.loadData();
				self.oldValue = value;
			}
		});

		function increasePeriod(num) {
			if (self.rollingPeriod === '1') {
				self.selectedDate(self.selectedDate().clone().add(num, 'week'));
			} else if (self.rollingPeriod === '2') {
				self.selectedDate(self.selectedDate().clone().add(num, 'month'));
			}
		}

		function getWeekStartDate(current) {
			var currentDayOfWeek = current.day();
			var differenceAsDays = currentDayOfWeek - parseInt(weekStart);
			if (differenceAsDays < 0) {
				differenceAsDays += 7;
			}

			return current.add('days', -differenceAsDays);
		}

		function getStartDate() {

			if (self.rollingPeriod === '1') {
				var currentDate = self.selectedDate().clone();
				return getWeekStartDate(currentDate);
			}

			if (self.rollingPeriod === '2') {
				return self.selectedDate().clone().startOf('month');
			}
		}

		function getEndDate() {
			
			if (self.rollingPeriod === '1') {
				return getStartDate(self.selectedDate().clone()).add('days', 6);
			}

			if (self.rollingPeriod === '2') {
				return self.selectedDate().clone().endOf('month');
			}
		}

		self.loadData = function () {
			self.agentBadges([]);
			self.isLoading(true);
			var data = {
				Date: self.currentDate().clone().utc().toDate().toJSON(),
				Type: self.selectedOptionType,
				SelectedId: self.selectedOptionId()
			};

			if (self.rollingPeriod !== '0') {
				data.StartDate = getStartDate().utc().toDate().toJSON();
				data.EndDate = getEndDate().utc().toDate().toJSON();
			}

			ajax.Ajax({
				url: 'BadgeLeaderBoardReport/Overview',
				dataType: 'json',
				data: data,
				success: function(data) {
					self.isLoading(false);

					var previousItem;
					var rankPosition = {
						rank: 1,
						index: 0
					};
					var newItems = ko.utils.arrayMap(data.Agents, function (item) {
						if (rankPosition.index > 0) {
							rankPosition.rank = haveSameRank(previousItem, item) ? rankPosition.rank : rankPosition.index + 1;
						}
						previousItem = item;
						var model = new BadgeViewModel(rankPosition, item);
						
						rankPosition.index++;
						return model;
					});
					self.agentBadges.push.apply(self.agentBadges, newItems);
				}
			});
		}

		

		self.loadOptions = function() {
			self.isLoading(true);
			ajax.Ajax({
				url: 'BadgeLeaderBoardReport/OptionsForLeaderboard',
				dataType: 'json',
				success: function (data) {
					self.isLoading(false);
					if (data.options.length === 1) {
						//MyOwn
						self.showOptions(false);
						self.selectedOptionType = 0;
						self.loadData();
					} else {
						self.showOptions(true);
						self.availableOptions(data.options);
						self.selectedOptionId(data.defaultOptionId);
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


	function haveSameRank(item1, item2) {
		return (item1.Gold == item2.Gold) && (item1.Silver == item2.Silver) && (item1.Bronze == item2.Bronze);
	}

	function BadgeViewModel(rankPosition, data) {
		var self = this;

		self.name = data.AgentName;
		self.gold = data.Gold;
		self.silver = data.Silver;
		self.bronze = data.Bronze;
		self.rank = rankPosition.rank;
	}

	function bindData() {
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function (data) {
			vm = new BadgeLeaderBoardReportViewModel(data.WeekStart);
			var elementToBind = $('.BadgeLeaderBoardReport')[0];
			$(".moment-datepicker").attr("data-bind", "datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart:" + data.WeekStart + "}");

			ko.applyBindings(vm, elementToBind);
		});
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
			ajax.AbortAll();

		}
	};
})(jQuery);
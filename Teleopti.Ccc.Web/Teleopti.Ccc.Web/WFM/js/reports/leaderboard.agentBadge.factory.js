(function() {
	'use strict';

	angular.module('wfm.reports').factory("LeaderBoardViewModelFactory", LeaderBoardViewModel)

	function LeaderBoardViewModel() {
		var leaderboardViewModelFactory = {
			Create: create
		};

		function create(agentBadges) {
			var previousItem;
			var rank = 1;
			return agentBadges.map(function(item, index) {

				if (index > 0) {
					rank = haveSameRank(previousItem, item) ? rank : index + 1;
				}
				previousItem = item;
				var model = badgeViewModel(rank, item);
				return model;
			});

		}

		function badgeViewModel(rank, data) {
			return {
				name: data.AgentName,
				gold: data.Gold,
				silver: data.Silver,
				bronze: data.Bronze,
				rank: rank
			};
		}
		function haveSameRank(item1, item2) {
			return (item1.Gold == item2.Gold) && (item1.Silver == item2.Silver) && (item1.Bronze == item2.Bronze);
		}

		return leaderboardViewModelFactory;
	}
})();
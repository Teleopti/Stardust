define(['buster', 'views/realtimeadherenceteams/team','navigation'], function (buster, team,navigation) {
	return function () {
		
		buster.testCase("real time adherence team", {
			"should be able to drill down to specified team when allowed": function () {

				var hasNeverBeenCalled = true;
				navigation.GotoRealTimeAdherenceTeamDetails = function () {
					hasNeverBeenCalled = false;
				};

				var teamviewmodel = team();
				teamviewmodel.openTeam();

				assert.isFalse(hasNeverBeenCalled);
			}

		});		
	};
});
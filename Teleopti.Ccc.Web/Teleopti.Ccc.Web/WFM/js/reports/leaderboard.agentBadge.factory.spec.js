(function(){
	"use strict";

	describe("leader board factory tests",function(){
		var target;

		beforeEach(function(){
			module("wfm.reports");
		});

		beforeEach(inject(function (LeaderBoardViewModelFactory) {
			target = LeaderBoardViewModelFactory;
		}));

		it("should calculate the correct badge rank order",function(){
			var agentBadges = [
								{
									"AgentName": "Carlos Oliveira", 
									"Gold": 3, 
									"Silver": 0, 
									"Bronze": 0
						        }, 
						        {
									"AgentName": "Kari Nies", 
									"Gold": 3, 
									"Silver": 0, 
									"Bronze": 0
						        },
						        {
									"AgentName": "Daniel Billsus", 
									"Gold": 1, 
									"Silver": 50, 
									"Bronze": 0
						        }, 
						        {
									"AgentName": "Michael Kantor", 
									"Gold": 1, 
									"Silver": 10, 
									"Bronze": 0
						        }, 
						        {
									"AgentName": "Bill Gates", 
									"Gold": 1, 
									"Silver": 10, 
									"Bronze": 0
						        }, 
						        {
									"AgentName": "Candy Mamer", 
									"Gold": 0, 
									"Silver": 50, 
									"Bronze": 0
						        }, 
						        {
									"AgentName": "Tim McMahon", 
									"Gold": 0, 
									"Silver": 50, 
									"Bronze": 0
						        }, 
						        {
									"AgentName": "Sharad Mehrotra", 
									"Gold": 0, 
									"Silver": 30, 
									"Bronze": 1
						        },
						         {
									"AgentName": "Steve Novack", 
									"Gold": 0, 
									"Silver": 30, 
									"Bronze": 0
								}
						      ];

			var tableList = target.Create(agentBadges);
			
			expect(tableList[0].rank).toEqual(1);
			expect(tableList[1].rank).toEqual(1);
			expect(tableList[2].rank).toEqual(3);
			expect(tableList[3].rank).toEqual(4);
			expect(tableList[4].rank).toEqual(4);
			expect(tableList[5].rank).toEqual(6);
			expect(tableList[6].rank).toEqual(6);
			expect(tableList[7].rank).toEqual(8);
			expect(tableList[8].rank).toEqual(9);
		});
	});

})();
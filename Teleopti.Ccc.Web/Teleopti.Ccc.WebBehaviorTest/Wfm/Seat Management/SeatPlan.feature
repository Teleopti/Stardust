@WFM
Feature: SeatPlan
	As a seat planner
	I want to be able to allocate seats to agents

Background: 
	Given I have a role with
		| Field                 | Value            |
		| Name                  | Seat Planner     |
		| Access to seatplanner | True             |
		| Name                  | Resource Planner |
		| Access to permissions | True             |
	And there is a planning period with
		| Field		| Value			|
		| Date		| 2015-04-10	|
	And there is a seatplan with 
		| Field		| Value		|
		| Date		| 2015-05-01	|
		| Status	| 2			| 
	And there is a seatplan with 
		| Field		| Value			|
		| Date		| 2015-05-02	|
		| Status	| 0				| 
	And there is a location with 
		| Field		| Value			|
		| Name		| Location1	|
	And I am englishspeaking swede

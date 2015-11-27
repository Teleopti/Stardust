Feature: SeatPlanReport
	As a seat planner
	I want to be able to view the result of SeatPlan in SeatPlanReport

Background: 
	Given I have a role with
		| Field                 | Value            |
		| Name                  | Seat Planner     |
		| Access to seatplanner | True             |
		| Name                  | Resource Planner |
		| Access to permissions | True             |
	And I am an agent in a team
	And I have a schedule period with 
		| Field      | Value      |
		| Start date | 2014-01-01 |
		| Type       | Week       |
		| Length     | 1          |
	And I have a person period with 
		| Field      | Value      |
		| Start date | 2014-01-01 |
	And there are shift categories
		| Name  |
		| Night |
		| Day   |
		| Late  |
	And there are activities
		| Name           | Color  |
		| Lunch          | Yellow |
		| White activity | White  |
		| Black activity | Black  |
	And I have a shift with
		| Field          | Value            |
		| StartTime      | 2015-01-01 8:00  |
		| EndTime        | 2015-01-01 17:00 |
		| Shift category | Day              |


Scenario: Should be able to view the report
	When I view Seat plan on "2015-01-01"
	And I click SeatPlanReport button
	Then I should be able to view the report content

Scenario: Should not showing paging bar when there is only one page
	When I view Seat plan on "2015-01-01"
	And I click SeatPlanReport button
	Then I should be able to view the report content
	And paging bar should be hide

Scenario: Should filter report by date when we enter report from planning period
	Given I have a shift with
		| Field          | Value            |
		| StartTime      | 2015-05-01 8:00  |
		| EndTime        | 2015-05-01 17:00 |
		| Shift category | Day |
	Given I have a shift with
		| Field          | Value            |
		| StartTime      | 2015-05-31 8:00  |
		| EndTime        | 2015-05-31 17:00 |
		| Shift category | Day              |
	And there is a planning period with
		| Field		| Value			|
		| Date		| 2015-04-01	|
	When I view Seat plan on "2015-05-01"
	And I click SeatPlanReport button from planning period
	Then I should be able to view the report from "2015-05-01" to "2015-05-31"

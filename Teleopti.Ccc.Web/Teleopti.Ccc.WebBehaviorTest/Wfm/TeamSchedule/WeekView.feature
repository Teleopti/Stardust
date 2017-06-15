@WFM
@OnlyRunIfEnabled('WfmTeamSchedule_WeekView_39870')
Feature: WeekView
	As a team leader
	I want to view agent's activities weekly

Background:
	Given I am american
	And there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And I have a role with
	| Field                         | Value          |
	| Name                          | Wfm Team Green |
	| Access to everyone            | True           |
	| Access to Wfm MyTeam Schedule | true           |
	| Add Activity                  | true           |
	| Add Personal Activity         | true           |
	| Remove Activity               | true           |
	| Move Activity                 | true           |
	And there is a shift category named 'Day'
	And there are activities
	| Name     | Color    | Allow meeting |
	| Phone    | Green    | true          |
	| Lunch    | Yellow   | false         |
	| Sales    | Red      | true          |
	| Training | Training | true          |
	And there is a contract named 'A contract'
	And there is a contract schedule named 'A contract schedule'
	And there is a part time percentage named 'Part time percentage'
	And there is a rule set with
	| Field          | Value       |
	| Name           | A rule set  |
	| Activity       | Phone       |
	| Shift category | Day         |	
	And there is a shift bag named 'A shift bag' with rule set 'A rule set'
	And there is a skill named 'A skill' with activity 'Phone'
	And 'John Smith' has a workflow control set publishing schedules until '2016-12-01'
	And 'John Smith' has a person period with
	| Field                | Value                |
	| Shift bag            | A shift bag          |
	| Skill                | A skill              |
	| Team                 | Team green           |
	| Start date           | 2016-01-01           |
	| Contract             | A contract           |
	| Contract schedule    | A contract schedule  |
	| Part time percentage | Part time percentage |
	And 'John Smith' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-10-10 09:00 |
	| EndTime          | 2016-10-10 17:00 |

Scenario: Should be able to see week view toggle button in day view
	When I view wfm team schedules
	Then I should be able to see week view toggle button

Scenario: Should be able to see day view toggle button in week view
	When I view wfm team schedules
	Then I should be able to see day view toggle button

@ignore
@OnlyRunIfDisabled('WfmTeamSchedule_DisplayScheduleOnBusinessHierachy_41260')
@OnlyRunIfDisabled('WfmTeamSchedule_DisplayWeekScheduleOnBusinessHierachy_42252')
Scenario: Should be able to toggle week view
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I select a site "The site"
	And I searched schedule with keyword 'Team green'
	And I toggle "WEEK" view
	Then I should see week view schedule table

@ignore
Scenario: Should be able to navigate to next week in week view
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I select a site "The site"
	And I searched schedule with keyword 'Team green'
	And I toggle "WEEK" view
	And I navigate to next week in week view
	Then The display date should be "2016-10-16"
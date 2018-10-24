@WFM
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


Scenario: should keep the selection of group pages when back to day view if the selection is cleared and changed in week view
	Given today is '2018-10-22'
	And there is a site named 'Yellow site'
	And there is a team named 'Team yellow' on 'Yellow site'
	And John has a person period with
	| Field      | Value      |
	| Start date | 2016-01-01 |
	| Team       | Team yellow |
	| Skill      | A skill    |
	And there is a site named 'Red site'
	And there is a team named 'Team red' on 'Red site'
	And Ashley has a person period with
	| Field      | Value      |
	| Start date | 2016-01-01 |
	| Team       | Team red |
	When I view wfm team schedules
	And I open group pages picker
	Then I select group 'Yellow site' on group page picker
	Then I close group pages picker
	And I click button to search for schedules
	When I toggle week view
	And I open group pages picker
	Then I click clear button in group pages picker
	And I select group 'Red site' on group page picker
	And I close group pages picker
	When I toggle day view
	Then I should see agent 'Ashley' in the table


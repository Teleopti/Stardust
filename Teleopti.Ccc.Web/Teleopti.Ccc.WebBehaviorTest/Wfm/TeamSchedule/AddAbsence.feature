@WFM
Feature: AddAbsence
	As a team leader
	I want to add absences to agents in different time zones

Background: 
	Given I am american
	And there is a site named 'The site'
	And there is a team named 'Team green' on site 'The site'
	And I have a person period with
	| Field      | Value      |
	| Start date | 2017-01-01 |
	| Team       | Team green |
	And I have a role with
	| Field                         | Value          |
	| Name                          | Wfm Team Green |
	| Access to everyone            | True           |
	| Access to Wfm MyTeam Schedule | true           |
	| Add Absence                   | true           |
	And there are activities
	| Name     | Color    |
	| Phone    | Green    |
	| Lunch    | Yellow   |
	| Sales    | Red      |
	And there are absences
	| Name     | Color |
	| Vacation | Pink  |
	| Illness  | Red   |
	And there is a shift category named 'Day'
	And there is a contract named 'A contract'
	And there is a contract schedule named 'A contract schedule'
	And there is a part time percentage named 'Part time percentage'
	And there is a rule set with
	| Field          | Value      |
	| Name           | A rule set |
	| Activity       | Phone      |
	| Shift category | Day        |
	And there is a shift bag named 'A shift bag' with rule set 'A rule set'
	And there is a skill named 'A skill' with activity 'Phone'
	And 'John Smith' has a workflow control set publishing schedules until '2017-12-31'
	And 'Bill Gates' has a workflow control set publishing schedules until '2017-12-31'
	And 'Bill Gates' is in Hawaii time zone
	And 'John Smith' has a person period with
	| Field                | Value                |
	| Shift bag            | A shift bag          |
	| Skill                | A skill              |
	| Team                 | Team green           |
	| Start date           | 2017-01-01           |
	| Contract             | A contract           |
	| Contract schedule    | A contract schedule  |
	| Part time percentage | Part time percentage |
	And 'Bill Gates' has a person period with
	| Field                | Value                |
	| Shift bag            | A shift bag          |
	| Skill                | A skill              |
	| Team                 | Team green           |
	| Start date           | 2017-01-01           |
	| Contract             | A contract           |
	| Contract schedule    | A contract schedule  |
	| Part time percentage | Part time percentage |

@ignore
Scenario: Can add intraday absence to agent in another time zone
	Given 'John Smith' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| StartTime      | 2017-04-17 09:00 |
	| EndTime        | 2017-04-17 17:00 |
	And 'Bill Gates' has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2017-04-17 10:00 |
	| EndTime          | 2017-04-17 17:00 |
	When I view wfm team schedules
	And I set schedule date to '2017-04-17'
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I selected agent 'Bill Gates'
	And I open menu in team schedule
	And I click menu item 'AddAbsence' in team schedule
	And I set a new absence as
	| Field        | Value            |
	| Absence      | Vacation         |
	| SelectedDate | 2017-04-17       |
	| StartTime    | 2017-04-17 21:00 |
	| EndTime      | 2017-04-17 22:00 |
	| FullDay      | False            |
	Then I should be able to apply my new absence
	When I apply the new absence
	Then I should see a successful notice

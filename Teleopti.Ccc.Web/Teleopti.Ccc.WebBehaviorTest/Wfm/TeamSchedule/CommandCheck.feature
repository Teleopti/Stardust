@WFM
Feature: CommandCheck
	There should be command check after user have add activity over NOT ALLOW MEETING activities

Background:
	Given I am american
	And there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And I have a person period with
	| Field      | Value      |
	| Start date | 2016-01-01 |
	| Team       | Team green |
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
		| Name           | Color  | Allow meeting |
		| Phone          | Green  | true          |
		| Lunch          | Yellow | false         |
		| Sales          | Red    | true          |
		| Training       | Blue   | true          |
		| PersonActivity | Black  | true          |
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
		| Field                  | Value            |
		| Shift category         | Day              |
		| Activity               | Phone            |
		| Start time             | 2016-10-10 09:00 |
		| End time               | 2016-10-10 17:00 |
		| Lunch Activity         | Lunch            |
		| Lunch start time       | 2016-10-10 12:00 |
		| Lunch end time         | 2016-10-10 13:00 |
		| ThirdActivity          | Training         |
		| ThirdActivityStartTime | 2016-10-10 09:00 |
		| ThirdActivityEndTime   | 2016-10-10 10:00 |

Scenario: Should show command check after adding activity on top of NOT ALLOW MEETING acitvity
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I open menu in team schedule
	And I click menu item 'AddActivity' in team schedule
	And I set new activity as
		| Field         | Value            |
		| Activity      | Training         |
		| Selected date | 2016-10-10       |
		| StartTime     | 2016-10-10 12:00 |
		| EndTime       | 2016-10-10 13:00 |
		| Is next day   | false            |
	Then I should be able to apply my new activity
	When I apply my new activity
	Then I should be able to see command check

	
Scenario: Should show command check after adding person activity on top of NOT ALLOW MEETING acitvity
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I open menu in team schedule
	And I click menu item 'AddPersonalActivity' in team schedule
	And I set new activity as
		| Field         | Value            |
		| Activity      | PersonActivity   |
		| Selected date | 2016-10-10       |
		| StartTime     | 2016-10-10 12:00 |
		| EndTime       | 2016-10-10 13:00 |
		| Is next day   | false            |
	When I apply my new activity
	Then I should be able to see command check


Scenario: Should show command check after moving activity on top of NOT ALLOW MEETING acitvity
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I searched schedule with keyword 'John'
	And I click button to search for schedules
	And I selected activity 'Training'
	And I move activity to '2016-10-10 12:00' with next day being 'false'
	Then I should be able to see command check
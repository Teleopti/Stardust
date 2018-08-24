@WFM
@OnlyRunIfEnabled('Wfm_WebPlan_Pilot_46815')
Feature: Planning Period
	As a resource planner
	I want to work on planning periods

Scenario: The first planning period suggestion should be the next upcoming schedule period
	Given the time is '2016-06-07'
	And I am swedish
	And I have a role with full access
	And there is a site named 'Site 1'
	And there is a team named 'Team 1' on 'Site 1'
	And Ashley Andeen has a person period with
		| Field      | Value      |
		| Team       | Team 1     |
		| Start Date | 2016-06-05 |
	And Ashley Andeen has a schedule period with
		| Field      | Value      |
		| Start date | 2016-06-05 |
		| Type       | Week       |
		| Length     | 1          |
	And there is an planning group with
		| Field               | Value           |
		| Planning group name | PlanningGroup 1 |
		| Team                | Team 1          |
	When I view planning periods for planning group 'PlanningGroup 1'
	And I click create planning period
	Then I should see planning period suggestions
	And I select the first suggestion
	When I click apply planning period
	Then I should see a planning period between '2016-06-12' and '2016-06-18'

Scenario: Creating next planning period should generate a period with the same type as the previous one 
	Given the time is '2016-06-07'
	And I am swedish
	And I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to resource planner            | True              |
	And there is a site named 'Site 1'
	And there is a team named 'Team 1' on 'Site 1'
	And there is an planning group with
		| Field               | Value           |
		| Planning group name | PlanningGroup 1 |
		| Team                | Team 1          |
	And there is a planning period with
		| Field               | Value           |
		| Date                | 2016-06-01      |
		| Planning group name | PlanningGroup 1 |
	When I view planning periods for planning group 'PlanningGroup 1'
	And I click create next planning period
	Then I should see a planning period between '2016-07-01' and '2016-07-31'

@RunningStardust 
Scenario: Schedule a planning period
	Given there is a dayoff named 'Day Off'
	And there is a scenario
	  | Field          | Value        |
	  | Name           | To           |
	  | Business Unit  | BusinessUnit |
	  | Extra scenario | true         |
  	And There is a skill to monitor called 'Skill 1' with queue id '9' and queue name 'queue1' and activity 'Phone'
  	And there is queue statistics for the skill 'Skill 1' up until '2016-06-08 17:00'
  	And there is forecast data for skill 'Skill 1' for 1 week starting from  '2016-06-08'
	And there is a shift category named 'Day'
	And there is a rule set with
	  | Field          | Value       |
	  | Name           | Common      |
	  | Activity       | Phone       |
	  | Shift category | Day         |
	  | Start boundry  | 8:00-10:00  |
	  | End boundry    | 16:00-18:00 |
	And there is a shift bag with
	  | Field    | Value  |
	  | Name     | Common |
	  | Rule set | Common |
	And there is a site named 'Site 1'
	And there is a team named 'Team 1' on 'Site 1'
  	And I am englishspeaking swede
  	And I have a role with full access
	And there is a contract schedule with
	  | Field              | Value     |
	  | Name               | Full week |
	  | Monday work day    | true      |
	  | Tuesday work day   | true      |
	  | Wednesday work day | true      |
	  | Thursday work day  | true      |
	  | Friday work day    | true      |
	  | Saturday work day  | true      |
	  | Sunday work day    | true      |
	And Ashley Andeen has a person period with
	  | Field      | Value      |
	  | Team       | Team 1     |
	  | Start Date | 2015-01-1  |
	  | Skill      | Skill 1    |
	  | Shift bag  | Common     |
	  | Contract Schedule|Full week|
	And Ashley Andeen has a schedule period with
	  | Field      | Value      |
	  | Start date | 2016-06-01 |
	  | Type       | Week       |
	  | Length     | 1          |
	And there is an planning group with
	  | Field               | Value           |
	  | Planning group name | PlanningGroup 1 |
	  | Team                | Team 1          |
	And there is a planning period with
	  | Field               | Value           |
	  | Date                | 2016-06-01      |
	  | Planning group name | PlanningGroup 1 |
	  | Type       			| Week            |
	When I view planning periods for planning group 'PlanningGroup 1'
	And I open planning period
	And I click schedule
	Then Planning period should have been scheduled

@RunningStardust
  Scenario: Intraday optimization a planning period
	Given there is a dayoff named 'Day Off'
	And there is a scenario
	  | Field          | Value        |
	  | Name           | To           |
	  | Business Unit  | BusinessUnit |
	  | Extra scenario | true         |
	And There is a skill to monitor called 'Skill 1' with queue id '9' and queue name 'queue1' and activity 'Phone'
	And there is queue statistics for the skill 'Skill 1' up until '2016-06-08 17:00'
	And there is forecast data for skill 'Skill 1' for 1 week starting from  '2016-06-08'
	And there is a shift category named 'Day'
	And there is a rule set with
	  | Field          | Value       |
	  | Name           | Common      |
	  | Activity       | Phone       |
	  | Shift category | Day         |
	  | Start boundry  | 8:00-10:00  |
	  | End boundry    | 16:00-18:00 |
	And there is a shift bag with
	  | Field    | Value  |
	  | Name     | Common |
	  | Rule set | Common |
	And there is a site named 'Site 1'
	And there is a team named 'Team 1' on 'Site 1'
	And I am englishspeaking swede
	And I have a role with full access
	And there is a contract schedule with
	  | Field              | Value     |
	  | Name               | Full week |
	  | Monday work day    | true      |
	  | Tuesday work day   | true      |
	  | Wednesday work day | true      |
	  | Thursday work day  | true      |
	  | Friday work day    | true      |
	  | Saturday work day  | true      |
	  | Sunday work day    | true      |
	And Ashley Andeen has a person period with
	  | Field      | Value      |
	  | Team       | Team 1     |
	  | Start Date | 2015-01-1  |
	  | Skill      | Skill 1    |
	  | Shift bag  | Common     |
	  | Contract Schedule|Full week|
	And Ashley Andeen has a schedule period with
	  | Field      | Value      |
	  | Start date | 2016-06-01 |
	  | Type       | Week       |
	  | Length     | 1          |
	And there is an planning group with
	  | Field               | Value           |
	  | Planning group name | PlanningGroup 1 |
	  | Team                | Team 1          |
	And there is a planning period with
	  | Field               | Value           |
	  | Date                | 2016-06-01      |
	  | Planning group name | PlanningGroup 1 |
	  | Type       			| Week            |
	  | Has scheduled       | True            |
	When I view planning periods for planning group 'PlanningGroup 1'
	And I open planning period
	When I click optimize intraday
	Then Planning period should have been intraday optimized
@RTA
Feature: Rta tracer
  In order to manually verify the rta works
  As a troubleshooter
  I want to see that states are received, and what the rta does with the states

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2017-01-01 |
	And there is a state with name 'Ready'

  Scenario: See process trace
	Given the time is '2017-09-25 08:00'
	And I view real time adherence trace for 'Pierre Baldi'
	When 'Pierre Baldi' sets his phone state to 'Ready'
	Then I should see process tracing 'Pierre Baldi'
	Then I should see process received data at '08:00'

  Scenario: See state trace
	Given the time is '2017-09-25 08:00'
	And I view real time adherence trace for 'Pierre Baldi'
	When 'Pierre Baldi' sets his phone state to 'Ready'
	Then I should see trace of state 'Ready' being 'Received'
	And I should see trace of state 'Ready' being 'Processing'
	And I should see trace of state 'Ready' being 'Processed'
	And I should see trace of state 'Ready' being 'PersonStateChangedEvent'

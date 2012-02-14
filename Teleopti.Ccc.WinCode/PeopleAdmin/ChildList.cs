using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
        /// <summary>
        /// Child List
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-11
        /// </remarks>
        public class ChildList
        {
            private readonly PersonPeriod _currentPeriod;
            private DateTime? _periodDate;
            private Guid? _currentTeamIdentifier;
            private Guid? _currentPersonContractIndentifier;
            private Guid? _currentContractScheduleIdentifier;
            private Guid? _currentPartTimePercentageIdentifier;


            /// <summary>
            /// Initializes a new instance of the <see cref="ChildList"/> class.
            /// </summary>
            /// <param name="personPeriod">The person period.</param>
            /// <remarks>
            /// Created by: Dinesh Ranasinghe
            /// Created date: 2008-03-13
            /// </remarks>
            public ChildList(PersonPeriod personPeriod)
            {
                _currentPeriod = personPeriod;
            }

            /// <summary>
            /// Gets the current team.
            /// </summary>
            /// <value>The current team.</value>
            /// <remarks>
            /// Created by: Dinesh Ranasinghe
            /// Created date: 2008-03-13
            /// </remarks>
            public Team CurrentTeam
            {
                get
                {
                    if (CurrentPeriod != null)
                    {
                        return CurrentPeriod.Team;
                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    if (CurrentPeriod != null)
                    {
                        CurrentPeriod.Team = value;
                    }
                }
            }


            /// <summary>
            /// Gets the skill.
            /// </summary>
            /// <value>The skill.</value>
            /// <remarks>
            /// Created by: Dinesh Ranasinghe
            /// Created date: 2008-03-13
            /// </remarks>
            public string Skill
            {
                get
                {
                    String skill = string.Empty;
                    if (CurrentPeriod != null)
                    {
                        foreach (PersonSkill personSkill in CurrentPeriod.PersonSkillCollection)
                        {
                            skill += personSkill.Skill.Name + ";";
                        }

                        return skill;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            /// <summary>
            /// Gets the current period.
            /// </summary>
            /// <value>The current period.</value>
            /// <remarks>
            /// Created by: Dinesh Ranasinghe
            /// Created date: 2008-03-13
            /// </remarks>
            public PersonPeriod CurrentPeriod
            {
                get
                {
                    return _currentPeriod;
                }

            }

            /// <summary>
            /// Gets the current person contract.
            /// </summary>
            /// <value>The current person contract.</value>
            /// <remarks>
            /// Created by: Dinesh Ranasinghe
            /// Created date: 2008-03-13
            /// </remarks>
            public PersonContract CurrentPersonContract
            {
                get
                {
                    if (CurrentPeriod != null)
                    {
                        return CurrentPeriod.PersonContract;
                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    if (CurrentPeriod != null)
                    {
                        CurrentPeriod.PersonContract = value;
                    }
                }
            }

            /// <summary>
            /// Gets or sets the date.
            /// </summary>
            /// <value>The date.</value>
            /// <remarks>
            /// Created by: Dinesh Ranasinghe
            /// Created date: 2008-03-11
            /// </remarks>
            public DateTime? PeriodDate
            {
                get
                {
                    if (CurrentPeriod != null)
                    {
                        _periodDate = CurrentPeriod.StartDate;
                        return _periodDate;
                    }
                    else return null;
                }
                set
                {
                    _periodDate = value;

                    if (CurrentPeriod != null)
                        //Todo: This will need to implement is null scenario
                        CurrentPeriod.StartDate = (DateTime)_periodDate;
                }
            }


            /// <summary>
            /// Gets the current team ID.
            /// </summary>
            /// <value>The current team ID.</value>
            /// <remarks>
            /// Created by: Dinesh Ranasinghe
            /// Created date: 2008-03-16
            /// </remarks>
            public Guid? CurrentTeamIdentifier
            {
                get
                {
                    if (CurrentTeam != null) return CurrentTeam.Id;
                    else
                    {
                        return _currentTeamIdentifier;
                    }
                }
                set
                {
                    _currentTeamIdentifier = value;
                }
            }

            /// <summary>
            /// Gets the current person contract ID.
            /// </summary>
            /// <value>The current person contract ID.</value>
            /// <remarks>
            /// Created by: Dinesh Ranasinghe
            /// Created date: 2008-03-16
            /// </remarks>
            public Guid? CurrentPersonContractIdentifier
            {
                get
                {
                    if (CurrentPersonContract != null)
                        return CurrentPersonContract.Contract.Id;
                    else
                    {
                        return _currentPersonContractIndentifier;
                    }
                }
                set
                {
                    _currentPersonContractIndentifier = value;
                }
            }

            /// <summary>
            /// Currents the contract schedule.
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// Created by:SanjayaI
            /// Created date: 3/27/2008
            /// </remarks>
            public ContractSchedule CurrentContractSchedule
            {
                get
                {

                    if (CurrentPeriod != null && CurrentPeriod.PersonContract != null && CurrentPeriod.PersonContract.ContractSchedule != null)
                    {
                        return CurrentPeriod.PersonContract.ContractSchedule;

                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    if (CurrentPeriod.PersonContract.ContractSchedule != null)
                    {
                        CurrentPeriod.PersonContract.ContractSchedule = value;
                    }
                }
            }

            /// <summary>
            /// Gets or sets the current part time percentage.
            /// </summary>
            /// <value>The current part time percentage.</value>
            /// <remarks>
            /// Created by:SanjayaI
            /// Created date: 3/27/2008
            /// </remarks>
            public PartTimePercentage CurrentPartTimePercentage
            {

                get
                {

                    if (CurrentPeriod != null && CurrentPeriod.PersonContract != null && CurrentPeriod.PersonContract.PartTimePercentage != null)
                    {
                        return CurrentPeriod.PersonContract.PartTimePercentage;

                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    if (CurrentPeriod != null && CurrentPeriod.PersonContract != null && CurrentPeriod.PersonContract.PartTimePercentage != null)
                    {
                        CurrentPeriod.PersonContract.PartTimePercentage = value;
                    }
                }


            }

            /// <summary>
            /// Gets or sets the current part time percentage identifier.
            /// </summary>
            /// <value>The current part time percentage identifier.</value>
            /// <remarks>
            /// Created by:SanjayaI
            /// Created date: 3/27/2008
            /// </remarks>
            public Guid? CurrentPartTimePercentageIdentifier
            {
                get
                {
                    if (CurrentPartTimePercentage != null) return CurrentPartTimePercentage.Id;
                    else
                    {
                        return _currentPartTimePercentageIdentifier;
                    }
                }
                set
                {
                    _currentPartTimePercentageIdentifier = value;
                }

            }

            /// <summary>
            /// Gets or sets the current contract schedule identifier.
            /// </summary>
            /// <value>The current contract schedule identifier.</value>
            /// <remarks>
            /// Created by:SanjayaI
            /// Created date: 3/27/2008
            /// </remarks>
            public Guid? CurrentContractScheduleIdentifier
            {
                get
                {
                    if (CurrentContractSchedule != null) return CurrentContractSchedule.Id;
                    else
                    {
                        return _currentContractScheduleIdentifier;
                    }
                }
                set
                {
                    _currentContractScheduleIdentifier = value;
                }
            }

        }
    }


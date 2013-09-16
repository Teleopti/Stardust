﻿using System;

namespace Teleopti.Interfaces.Domain
{

	/// <summary>
	/// Responsible to update all the person absence accounts
	/// </summary>
	public interface IPersonAccountUpdater
	{
		/// <summary>
		/// Updates the person absence accounts on activation / on termination
		/// </summary>
		void Update(IPerson person);
	}
}
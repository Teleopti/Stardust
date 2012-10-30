using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// 
	/// </summary>
	public interface IChangePasswordResultInfo
	{
		/// <summary>
		/// 
		/// </summary>
		bool IsSuccessful { get; set; }
		/// <summary>
		/// 
		/// </summary>
		bool IsAuthenticationSuccessful { get; set; }
	}
}

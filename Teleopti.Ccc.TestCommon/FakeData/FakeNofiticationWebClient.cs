using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeNofiticationWebClient : INotificationClient
	{
		private bool _requestFaild = false;
		private const string _errorResponse = @"<clickAPI>
							<sendMsgResp>
								<fault>001, Authentication failed</fault>
								<to>13640632742</to>
								<sequence_no></sequence_no>
							</sendMsgResp>
						</clickAPI>";

		private const string _successResponse = @"<clickAPI>
							<sendMsgResp>
								<success>true</success>
								<to>13640632742</to>
								<sequence_no></sequence_no>
							</sendMsgResp>
						</clickAPI>";

		public FakeNofiticationWebClient()
		{
			SentMessages = new List<string>();
		}
		public List<string> SentMessages { get; private set; }

		public string MakeRequest(string queryStringData)
		{
			SentMessages.Add(queryStringData);
			if (_requestFaild) {
				return _errorResponse;
			}
			return _successResponse;
		}

		public void MakeRequestFaild()
		{
			_requestFaild = true;
		}
		public void Dispose()
		{

		}
	}


}

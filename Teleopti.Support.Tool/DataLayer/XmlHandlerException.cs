using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Support.Tool.DataLayer
{
  /// <summary>
  /// Contains a list of errors and messages that was recieved during an execut, could be used when you don't want to throw an Exception.
  /// </summary>
  
public class XmlHandlerMessages
    {
        private List<string> errors;
        private List<string> messages;
        public XmlHandlerMessages()
        {

            errors = new List<string>();
            messages = new List<string>();
        }

        public void AddError(string message)
        {
            errors.Add(message);

        }
        public string GetError(int index)
        {
            return errors[index];
        }

        public IList<string> Errors
        {
            get { return errors; }

        }

        public void AddMessage(string message)
        {

            messages.Add(message);
        }
        public IList<string> Messages
        {
            get { return messages; }
        }
        public string GetMessage(int index)
        {
            return messages[index];
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Teleopti.Ccc.Sdk.ClientProxies
{
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://customer.teleopti.com/ws/", ConfigurationName="SingleSignOn.SingleSignOnServiceSoap")]
    public interface ISingleSignOnServiceSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://customer.teleopti.com/ws/InitializeSSO", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string InitializeSSO(string indata);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://customer.teleopti.com/ws/InitializeSSOArray", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        InitializeSSOArrayResponse InitializeSSOArray(InitializeSSOArrayRequest request);
    }
    
    [System.ServiceModel.MessageContractAttribute(WrapperName="InitializeSSOArray", WrapperNamespace="http://customer.teleopti.com/ws/", IsWrapped=true)]
    public class InitializeSSOArrayRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://customer.teleopti.com/ws/", Order=0)]
        [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")]
        public byte[] indata;
        
        public InitializeSSOArrayRequest() {
        }
        
        public InitializeSSOArrayRequest(byte[] indata) {
            this.indata = indata;
        }
    }
    
    [System.ServiceModel.MessageContractAttribute(WrapperName="InitializeSSOArrayResponse", WrapperNamespace="http://customer.teleopti.com/ws/", IsWrapped=true)]
    public class InitializeSSOArrayResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://customer.teleopti.com/ws/", Order=0)]
        public SSOResponse InitializeSSOArrayResult;
        
        public InitializeSSOArrayResponse() {
        }
        
        public InitializeSSOArrayResponse(SSOResponse InitializeSSOArrayResult) {
            this.InitializeSSOArrayResult = InitializeSSOArrayResult;
        }
    }

    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://customer.teleopti.com/ws/")]
    public class SSOResponse : object, System.ComponentModel.INotifyPropertyChanged {
        
        private int statusField;
        
        private string messageField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public int Status {
            get {
                return this.statusField;
            }
            set {
                this.statusField = value;
                this.RaisePropertyChanged("Status");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string Message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
                this.RaisePropertyChanged("Message");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }


    public class CustomerWebProxy : ClientBase<ISingleSignOnServiceSoap>
    {
        public InitializeSSOArrayResponse InitializeSSOArray(InitializeSSOArrayRequest request)
        {
            return Channel.InitializeSSOArray(request);
        }
    }
}

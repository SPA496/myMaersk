﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myDamco.DocumentManagement {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://www.bea.com/servers/wls/samples/net/mlog/edoc/webservice/edocwebservice/wi" +
        "dget", ConfigurationName="DocumentManagement.RecentPouchWidgetWSSoapBinding")]
    public interface RecentPouchWidgetWSSoapBinding {
        
        // CODEGEN: Generating message contract since element name UserLogin from namespace http://www.bea.com/servers/wls/samples/net/mlog/edoc/webservice/edocwebservice/widget is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        myDamco.DocumentManagement.getReleasedPouchDataResponse getReleasedPouchData(myDamco.DocumentManagement.getReleasedPouchDataRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class getReleasedPouchDataRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="getReleasedPouchData", Namespace="http://www.bea.com/servers/wls/samples/net/mlog/edoc/webservice/edocwebservice/wi" +
            "dget", Order=0)]
        public myDamco.DocumentManagement.getReleasedPouchDataRequestBody Body;
        
        public getReleasedPouchDataRequest() {
        }
        
        public getReleasedPouchDataRequest(myDamco.DocumentManagement.getReleasedPouchDataRequestBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://www.bea.com/servers/wls/samples/net/mlog/edoc/webservice/edocwebservice/wi" +
        "dget")]
    public partial class getReleasedPouchDataRequestBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string UserLogin;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string CacheKey;
        
        public getReleasedPouchDataRequestBody() {
        }
        
        public getReleasedPouchDataRequestBody(string UserLogin, string CacheKey) {
            this.UserLogin = UserLogin;
            this.CacheKey = CacheKey;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class getReleasedPouchDataResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="getReleasedPouchDataResponse", Namespace="http://www.bea.com/servers/wls/samples/net/mlog/edoc/webservice/edocwebservice/wi" +
            "dget", Order=0)]
        public myDamco.DocumentManagement.getReleasedPouchDataResponseBody Body;
        
        public getReleasedPouchDataResponse() {
        }
        
        public getReleasedPouchDataResponse(myDamco.DocumentManagement.getReleasedPouchDataResponseBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://www.bea.com/servers/wls/samples/net/mlog/edoc/webservice/edocwebservice/wi" +
        "dget")]
    public partial class getReleasedPouchDataResponseBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string WidgetResponse;
        
        public getReleasedPouchDataResponseBody() {
        }
        
        public getReleasedPouchDataResponseBody(string WidgetResponse) {
            this.WidgetResponse = WidgetResponse;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface RecentPouchWidgetWSSoapBindingChannel : myDamco.DocumentManagement.RecentPouchWidgetWSSoapBinding, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class RecentPouchWidgetWSSoapBindingClient : System.ServiceModel.ClientBase<myDamco.DocumentManagement.RecentPouchWidgetWSSoapBinding>, myDamco.DocumentManagement.RecentPouchWidgetWSSoapBinding {
        
        public RecentPouchWidgetWSSoapBindingClient() {
        }
        
        public RecentPouchWidgetWSSoapBindingClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public RecentPouchWidgetWSSoapBindingClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RecentPouchWidgetWSSoapBindingClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RecentPouchWidgetWSSoapBindingClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        myDamco.DocumentManagement.getReleasedPouchDataResponse myDamco.DocumentManagement.RecentPouchWidgetWSSoapBinding.getReleasedPouchData(myDamco.DocumentManagement.getReleasedPouchDataRequest request) {
            return base.Channel.getReleasedPouchData(request);
        }
        
        public string getReleasedPouchData(string UserLogin, string CacheKey) {
            myDamco.DocumentManagement.getReleasedPouchDataRequest inValue = new myDamco.DocumentManagement.getReleasedPouchDataRequest();
            inValue.Body = new myDamco.DocumentManagement.getReleasedPouchDataRequestBody();
            inValue.Body.UserLogin = UserLogin;
            inValue.Body.CacheKey = CacheKey;
            myDamco.DocumentManagement.getReleasedPouchDataResponse retVal = ((myDamco.DocumentManagement.RecentPouchWidgetWSSoapBinding)(this)).getReleasedPouchData(inValue);
            return retVal.Body.WidgetResponse;
        }
    }
}

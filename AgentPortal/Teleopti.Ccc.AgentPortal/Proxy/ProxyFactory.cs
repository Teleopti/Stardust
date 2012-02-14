#region Imports

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;
using System.Xml.Schema;
using Microsoft.CSharp;

#endregion

namespace Teleopti.Ccc.AgentPortal.Proxy
{

    /// <summary>
    /// Represents a factory class for the web service proxy
    /// </summary>
    public class ProxyFactory<T>
    {
        #region Fields - Instance Member

        private const string ProxyNameSpace = "Contract";
        private const string ListFullClassName = "System.Collections.Generic.List<{0}>";
        private const string ProxyInvocationHelperType = "DTODefinition.ProxyInvocationHelper";
        private const string SetPropertyDefaultArgumentName = "value";
        private const string CopyArrayMethodName = "CopyArray";
        private const string TwoPointOInterfaceAssemblyName = "DTODefinition";

        private bool _useGenericList = false;
        private readonly Uri _wsdlLocation;
        private readonly Protocols _protocolName = Protocols.Soap;
        private readonly Type _serviceContract;
        private readonly static string DoConvertMethodName = "DoConvert";

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - ProxyFactory Members

        /// <summary>
        /// Gets or sets a value indicating whether [apply generic list support].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [apply generic list support]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        /// <remarks>
        /// Set this property to <c>true</c> if you want to use the System.Collections.Generic.List<{0}>
        /// </remarks>
        public bool ApplyGenericListSupport
        {
            get
            {
                return _useGenericList;
            }
            set
            {
                _useGenericList = value;
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyFactory"/> class.
        /// </summary>
        /// <param name="wsdlLocation">The WSDL location.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        public ProxyFactory(string wsdlLocation)
            : this(wsdlLocation, Protocols.Soap)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyFactory"/> class.
        /// </summary>
        /// <param name="wsdlLocation">The WSDL location.</param>
        /// <param name="protocolName">Name of the protocol.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        public ProxyFactory(string wsdlLocation, Protocols protocolName)
        {
            _wsdlLocation = new Uri(wsdlLocation);
            _protocolName = protocolName;
            _serviceContract = typeof(T);
        }

        #endregion

        #region Events - Instance Member

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - ProxyFactory Members

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns>Created instance of the proxy</returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        public T Build()
        {
            ServiceDescriptionImporter serviceDescriptionImporter = new ServiceDescriptionImporter();
            serviceDescriptionImporter.ProtocolName = _protocolName.ToString();

            DiscoverService(serviceDescriptionImporter);

            Assembly assembly;
            using (CSharpCodeProvider codeProvider = new CSharpCodeProvider())
            {
                assembly = GenerateProxyAssembly(codeProvider, GenerateProxySourceCode(serviceDescriptionImporter, codeProvider));
            }

            Type serviceProxyType = assembly.GetTypes()[0];
            T proxyInstance = (T)Activator.CreateInstance(serviceProxyType);

            return proxyInstance;
        }

        /// <summary>
        /// Discovers the service.
        /// </summary>
        /// <param name="importer">The importer.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        private void DiscoverService(ServiceDescriptionImporter importer)
        {
            using (DiscoveryClientProtocol discoClient = new DiscoveryClientProtocol())
            {
                discoClient.DiscoverAny(_wsdlLocation.ToString());
                discoClient.ResolveAll();

                foreach (object value in discoClient.Documents.Values)
                {
                    ServiceDescription serviceDescription = value as ServiceDescription;
                    if (serviceDescription != null)
                    {
                        importer.AddServiceDescription(serviceDescription, null, null);
                    }
                    else
                    {
                        XmlSchema xmlSchema = value as XmlSchema;
                        if (xmlSchema != null)
                            importer.Schemas.Add(xmlSchema);

                    }
                }
            }
        }

        /// <summary>
        /// Generates the proxy source code.
        /// </summary>
        /// <param name="importer">The importer.</param>
        /// <param name="codeProvider">The code provider.</param>
        /// <returns>Source code for the proxy</returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        private string GenerateProxySourceCode(ServiceDescriptionImporter importer, 
                                               CodeDomProvider codeProvider)
        {
            CodeNamespace mainNamespace = new CodeNamespace(ProxyNameSpace);
            importer.Import(mainNamespace, null);

            if (_useGenericList)
                ChangeArrayToGenericList(mainNamespace);

            AddImports(mainNamespace);

            CodeTypeDeclaration[] codeTypeDeclarations = new CodeTypeDeclaration[mainNamespace.Types.Count];
            mainNamespace.Types.CopyTo(codeTypeDeclarations, 0);

            string assemblyName = "DTODefinition";
            Assembly definitionAssembly = AppDomain.CurrentDomain.Load(assemblyName);

            foreach (CodeTypeDeclaration declarationType in codeTypeDeclarations)
            {
                if (!((declarationType.BaseTypes.Count > 0)
                    && (declarationType.BaseTypes[0].BaseType == typeof(SoapHttpClientProtocol).FullName)))
                {
                    Type interfaceType = definitionAssembly.GetType(assemblyName + ".I" + declarationType.Name);
                    declarationType.BaseTypes.Add(interfaceType);

                    for (int i = (declarationType.Members.Count - 1); i >= 0; i--)
                    {
                        if (declarationType.Members[i] is CodeMemberField)
                        {
                            CodeMemberField field = (CodeMemberField)declarationType.Members[i];
                            string fieldName = field.Name;

                            PropertyInfo property = interfaceType.GetProperty(fieldName);
                            if (property != null)
                            {
                                CodeMemberProperty memberProperty = new CodeMemberProperty();
                                memberProperty.Name = interfaceType.Name + "." + fieldName;
                                memberProperty.HasGet = property.CanRead ? true : false;
                                memberProperty.HasSet = property.CanWrite ? true : false;
                                memberProperty.Attributes = 0;

                                memberProperty.Type = new CodeTypeReference(property.PropertyType);
                                declarationType.Members.Add(memberProperty);
                                AddArrayTypes(field, fieldName, property, memberProperty);//To add the aray type properties
                                AddGenericArrayTypes(field, fieldName, property, memberProperty); //To add the collection based properties
                            }
                        }
                    }
                }
            }

            ImplementServiceInterface(mainNamespace);

            return BuildSourceCode(mainNamespace, codeProvider);
        }

        /// <summary>
        /// Adds the generic array types.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="property">The property.</param>
        /// <param name="memberProperty">The member property.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        private static void AddGenericArrayTypes(CodeMemberField field, 
                                                 string fieldName, 
                                                 PropertyInfo property, 
                                                 CodeMemberProperty memberProperty)
        {
            if ((field.Type.ArrayElementType != null) &&
                (property.PropertyType.IsGenericType))
            {

                if (memberProperty.HasGet)
                {
                    memberProperty.GetStatements.Clear();

                    memberProperty.GetStatements.Add(new CodeMethodReturnStatement
                    (
                        new CodeMethodInvokeExpression
                        (
                            new CodeMethodReferenceExpression
                            (
                                new CodeTypeReferenceExpression(ProxyInvocationHelperType),
                                DoConvertMethodName,
                                new CodeTypeReference[] 
                                {
                                    new CodeTypeReference(property.PropertyType.GetGenericArguments()[0]),
                                    new CodeTypeReference(typeof(object)),
                                }),
                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)
                            )
                        )
                    );
                }

                if (memberProperty.HasSet)
                {
                    memberProperty.SetStatements.Clear();
                    memberProperty.SetStatements.Add
                    (
                        new CodeAssignStatement
                        (
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), property.Name),
                            new CodeMethodInvokeExpression
                            (
                                new CodeMethodReferenceExpression
                                    (
                                        new CodeTypeReferenceExpression(ProxyInvocationHelperType),
                                        DoConvertMethodName,
                                        new CodeTypeReference[] 
                                        {
                                            new CodeTypeReference(typeof(object)),
                                            new CodeTypeReference(property.PropertyType.GetGenericArguments()[0]),
                                        }
                                    ),
                                new CodeVariableReferenceExpression(SetPropertyDefaultArgumentName)
                            )
                        )
                    );
                }
            }
        }

        /// <summary>
        /// Adds the array types.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="property">The property.</param>
        /// <param name="memberProperty">The member property.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        private static void AddArrayTypes(CodeMemberField field, 
                                          string fieldName, 
                                          PropertyInfo property, 
                                          CodeMemberProperty memberProperty)
        {
            if (memberProperty.HasGet)
            {
                if (field.Type.ArrayElementType != null && !property.PropertyType.IsGenericType)
                {
                    memberProperty.GetStatements.Add
                    (
                        new CodeMethodReturnStatement
                        (
                            new CodeMethodInvokeExpression
                            (
                                new CodeMethodReferenceExpression
                                (
                                    new CodeTypeReferenceExpression(ProxyInvocationHelperType),
                                    CopyArrayMethodName,
                                    new CodeTypeReference[] 
                                    {
                                        new CodeTypeReference(property.PropertyType.FullName.Replace("[]", "")),
                                    }
                                ),
                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)
                            )
                        )
                    );
                }
                else
                {
                    memberProperty.GetStatements.Add
                    (
                        new CodeMethodReturnStatement
                        (
                            new CodeCastExpression(memberProperty.Type, new CodeVariableReferenceExpression(property.Name))
                        )
                    );
                }
            }

            if (memberProperty.HasSet)
            {
                memberProperty.SetStatements.Add
                (
                    new CodeAssignStatement
                    (
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), property.Name),
                        new CodeCastExpression(field.Type, new CodeVariableReferenceExpression(SetPropertyDefaultArgumentName))
                    )
                );
            }
        }

        /// <summary>
        /// Builds the source code.
        /// </summary>
        /// <param name="mainNamespace">The main namespace.</param>
        /// <param name="codeProvider">The code provider.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        private static string BuildSourceCode(CodeNamespace mainNamespace, 
                                              CodeDomProvider codeProvider)
        {
            StringBuilder proxySourceCode = new StringBuilder();
            string code;
            using (StringWriter sw = new StringWriter(proxySourceCode, CultureInfo.CurrentCulture))
            {
                codeProvider.GenerateCodeFromNamespace(mainNamespace, sw, null);
                code = proxySourceCode.ToString();
                sw.Close();
            }

            return code;
        }

        /// <summary>
        /// Implements the service interface.
        /// </summary>
        /// <param name="mainNamespace">The main namespace.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        private static void ImplementServiceInterface(CodeNamespace mainNamespace)
        {
            Type proxyType = typeof (T);
            mainNamespace.Types[0].BaseTypes.Add(new CodeTypeReference(proxyType));

            MethodInfo[] serviceMethods = proxyType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (MethodInfo method in serviceMethods)
            {
                CodeMemberMethod serviceMethod = new CodeMemberMethod();
                serviceMethod.Name = string.Concat(proxyType.Name, ".", method.Name);
                serviceMethod.ReturnType = new CodeTypeReference(method.ReturnType);
                serviceMethod.Attributes = 0;

                ParameterInfo[] methodParameters = method.GetParameters();
                CodeTypeReference[] parameters = new CodeTypeReference[methodParameters.Length];

                int index = 0;
                foreach (ParameterInfo parameter in methodParameters)
                {
                    parameters[index] = new CodeTypeReference(parameter.ParameterType);
                    index++;
                }

                CodeMethodReferenceExpression codeRefExpr = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), 
                                                                                              method.Name, 
                                                                                              parameters);
                CodeMethodInvokeExpression codeMethodInvokeExpr = new CodeMethodInvokeExpression(codeRefExpr);
                serviceMethod.Statements.Add(new CodeMethodReturnStatement(codeMethodInvokeExpr));
                mainNamespace.Types[0].Members.Add(serviceMethod);
            }
        }

        /// <summary>
        /// Changes the array to generic list.
        /// </summary>
        /// <param name="codeNamespace">The code namespace.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        private static void ChangeArrayToGenericList(CodeNamespace codeNamespace)
        {
            CodeTypeDeclaration[] typeDeclarations = new CodeTypeDeclaration[codeNamespace.Types.Count];
            codeNamespace.Types.CopyTo(typeDeclarations, 0);

            foreach (CodeTypeDeclaration type in typeDeclarations)
            {
                foreach (CodeTypeMember member in type.Members)
                {
                    CodeMemberMethod methodMember = member as CodeMemberMethod;
                    if (methodMember != null)
                    {
                        foreach (CodeParameterDeclarationExpression paremeter in methodMember.Parameters)
                        {
                            if (paremeter.Type.ArrayElementType != null)
                                paremeter.Type = new CodeTypeReference(String.Format(CultureInfo.InvariantCulture,
                                                                       ListFullClassName,
                                                                       paremeter.Type.ArrayElementType.BaseType));
                        }

                        if (methodMember.ReturnType.ArrayElementType != null)
                        {
                            string listGenericType = String.Format(CultureInfo.InvariantCulture,
                                                                   ListFullClassName,
                                                                   methodMember.ReturnType.ArrayElementType.BaseType);

                            methodMember.ReturnType = new CodeTypeReference(listGenericType);

                            CodeMethodReturnStatement returnStatement = null;
                            foreach (CodeStatement code in methodMember.Statements)
                            {
                                returnStatement = code as CodeMethodReturnStatement;
                                if (returnStatement != null)
                                    break;
                            }

                            if (returnStatement != null)
                            {
                                methodMember.Statements.Remove(returnStatement);
                                string format = String.Format(CultureInfo.InvariantCulture, "({0})results[0]", listGenericType);
                                CodeArgumentReferenceExpression codeArgRefExpr = new CodeArgumentReferenceExpression(format);
                                methodMember.Statements.Add(new CodeMethodReturnStatement(codeArgRefExpr));
                            }
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Adds the imports.
        /// </summary>
        /// <param name="codeNamespace">The code namespace.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        private static void AddImports(CodeNamespace codeNamespace)
        {
            CodeNamespaceImport cnm = new CodeNamespaceImport(TwoPointOInterfaceAssemblyName);
            codeNamespace.Imports.Add(cnm);
        }

        /// <summary>
        /// Generates the proxy assembly.
        /// </summary>
        /// <param name="csharpCodeProvider">The csharp code provider.</param>
        /// <param name="proxyCode">The proxy code.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        private Assembly GenerateProxyAssembly(CodeDomProvider csharpCodeProvider, 
                                               string proxyCode)
        {
            CompilerParameters parameters = new CompilerParameters();

            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.ReferencedAssemblies.Add("System.Web.dll");
            parameters.ReferencedAssemblies.Add("System.Web.Services.dll");
            parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

            GetReferencedAssemblies(_serviceContract.Assembly, parameters);

            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = false;
            parameters.IncludeDebugInformation = false;
            parameters.TempFiles = new TempFileCollection(Path.GetTempPath());

            CompilerResults compilerResults = csharpCodeProvider.CompileAssemblyFromSource(parameters, proxyCode);

            if (compilerResults.Errors.Count > 0)
                return null;

            Assembly compiledAssembly = compilerResults.CompiledAssembly;
            return compiledAssembly;
        }

        /// <summary>
        /// Gets the referenced assemblies.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="parameters">The parameters.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/27/2008
        /// </remarks>
        private static void GetReferencedAssemblies(Assembly assembly, 
                                                    CompilerParameters parameters)
        {
            if (!parameters.ReferencedAssemblies.Contains(assembly.Location))
            {
                string location = Path.GetFileName(assembly.Location);
                if (!parameters.ReferencedAssemblies.Contains(location))
                {
                    parameters.ReferencedAssemblies.Add(assembly.Location);
                    foreach (AssemblyName referencedAssembly in assembly.GetReferencedAssemblies())
                        GetReferencedAssemblies(Assembly.Load(referencedAssembly.FullName), parameters);
                }
            }
        }

        #endregion

        #endregion

    }

}

using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using NJsonSchema;
using NSwag;
using NSwag.SwaggerGeneration;

namespace Teleopti.Wfm.Api
{
	public class SwaggerHandler : OwinMiddleware
	{
		readonly CommandDtoProvider commandProvider;
		readonly QueryHandlerProvider queryProvider;

		private const string url = "/swagger/v1/swagger.json";

		public SwaggerHandler(OwinMiddleware next, CommandDtoProvider commandProvider, QueryHandlerProvider queryProvider) : base(next)
		{
			this.queryProvider = queryProvider;
			this.commandProvider = commandProvider;
		}

		private string generateDoc(IOwinContext context)
		{
			var settings = new NJsonSchema.Generation.JsonSchemaGeneratorSettings
			{
				SchemaType = SchemaType.Swagger2,
				DefaultPropertyNameHandling = PropertyNameHandling.CamelCase
			};

			var generator = new SwaggerJsonSchemaGenerator(settings);
			var doc = new SwaggerDocument();
			var resolver = new SwaggerSchemaResolver(doc, settings);

			doc.Info.Title = "Teleopti WFM api";
			doc.Info.Description = "The api to use for integrations with Teleopti WFM. Please specify an api key to use to run the samples.";

			doc.Host = context.Request.Host.Value ?? "";
			doc.Schemes.Add(context.Request.Scheme == "http" ? SwaggerSchema.Http : SwaggerSchema.Https);
			doc.BasePath = context.Request.PathBase.Value?.Substring(0, context.Request.PathBase.Value.Length) ?? "";

			var list = new System.Collections.Generic.List<SwaggerSecurityRequirement> { new SwaggerSecurityRequirement { { "Authorization", new[] { "Authorization" } } } };
			var swaggerOperations = new SwaggerOperations {{SwaggerOperationMethod.Get, new SwaggerOperation {Security = list}}};
			doc.Paths.Add("/api/wfm/command", swaggerOperations);
			foreach (var cmd in commandProvider.AllowedCommandTypes())
			{
				var opname = cmd.Name.Substring(0, cmd.Name.LastIndexOf("Dto", StringComparison.InvariantCultureIgnoreCase));
				var operations = new SwaggerOperations();
				var swaggerOperation = new SwaggerOperation
				{
					OperationId = "POST_command_" + opname,
					Security = list
				};
				var swaggerResponse = new SwaggerResponse
				{
					Schema = generator.GenerateWithReferenceAndNullability<JsonSchema4>(typeof(ResultDto), null, false, resolver)
						.Result
				};
				swaggerOperation.Responses.Add("200", swaggerResponse);

				var param = new SwaggerParameter
				{
					Description = "Command arguments for " + opname,
					Kind = SwaggerParameterKind.Body,
					Schema = generator.GenerateWithReferenceAndNullability<JsonSchema4>(cmd, null, false, resolver).Result
				};
				swaggerOperation.Parameters.Add(param);

				operations.Add(SwaggerOperationMethod.Post, swaggerOperation);
				doc.Paths.Add("/api/wfm/command/" + opname, operations);

			}

			swaggerOperations = new SwaggerOperations {{SwaggerOperationMethod.Get, new SwaggerOperation {Security = list}}};
			doc.Paths.Add("/api/wfm/query", swaggerOperations);
			foreach (var query in queryProvider.AllowedQueryTypes())
			{
				var opname = query.Item2.Name.Substring(0, query.Item2.Name.LastIndexOf("Dto", StringComparison.InvariantCultureIgnoreCase));
				var typename = query.Item3.Name.Substring(0, query.Item3.Name.LastIndexOf("Dto", StringComparison.InvariantCultureIgnoreCase));

				var operations = new SwaggerOperations();
				var swaggerOperation = new SwaggerOperation
				{
					Security = list,
					OperationId = "POST_query_" + opname
				};
				var swaggerResponse = new SwaggerResponse
				{
					Schema = generator.GenerateWithReferenceAndNullability<JsonSchema4>(query.Item3, null, false, resolver).Result
				};
				swaggerOperation.Responses.Add("200", swaggerResponse);

				var param = new SwaggerParameter
				{
					Description = "Query arguments for " + opname,
					Kind = SwaggerParameterKind.Body,
					Schema = generator.GenerateWithReferenceAndNullability<JsonSchema4>(query.Item2, null, false, resolver).Result
				};
				swaggerOperation.Parameters.Add(param);

				operations.Add(SwaggerOperationMethod.Post, swaggerOperation);
				doc.Paths.Add("/api/wfm/query/" + typename + "/" + opname, operations);

			}

			doc.SecurityDefinitions.Add("Authorization", new SwaggerSecurityScheme { Name = "Authorization", Description = "Bearer token", Type = SwaggerSecuritySchemeType.ApiKey, In = SwaggerSecurityApiKeyLocation.Header });
			doc.Consumes = new System.Collections.Generic.List<string> { "application/json" };
			doc.Produces = new System.Collections.Generic.List<string> { "application/json" };

			return doc.ToJson();
		}

		public override Task Invoke(IOwinContext context)
		{
			if (context.Request.Path.HasValue && string.Equals(context.Request.Path.Value.Trim('/'), url.Trim('/'), StringComparison.OrdinalIgnoreCase))
			{
				context.Response.StatusCode = 200;
				context.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
				return context.Response.WriteAsync(generateDoc(context));
			}

			// Call the next delegate/middleware in the pipeline
			return Next.Invoke(context);
		}
	}
}
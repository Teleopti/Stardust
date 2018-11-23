using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;

namespace Teleopti.Wfm.Api
{
	public class SwaggerHandler : OwinMiddleware
	{
		private readonly CommandDtoProvider commandProvider;
		private readonly QueryHandlerProvider queryProvider;
		private readonly JsonSchemaGeneratorSettings settings = new JsonSchemaGeneratorSettings
		{
			SchemaType = SchemaType.Swagger2,
			DefaultPropertyNameHandling = PropertyNameHandling.CamelCase,
			AllowReferencesWithProperties = true
		};

		private const string url = "/swagger/v1/swagger.json";

		public SwaggerHandler(OwinMiddleware next, CommandDtoProvider commandProvider, QueryHandlerProvider queryProvider) : base(next)
		{
			this.queryProvider = queryProvider;
			this.commandProvider = commandProvider;
		}

		private string generateDoc(IOwinContext context)
		{
			var doc = new SwaggerDocument
			{
				Info =
				{
					Title = "Teleopti WFM api",
					Description =
						"The api to use for integrations with Teleopti WFM. Please specify an api key to use to run the samples."
				},
				Host = context.Request.Host.Value ?? "",
			};

			var schemaResolver = new SwaggerSchemaResolver(doc, settings);
			var generator = new JsonSchemaGenerator(settings);
			
			doc.Schemes.Add(context.Request.Scheme == "http" ? SwaggerSchema.Http : SwaggerSchema.Https);
			doc.BasePath = context.Request.PathBase.Value?.Substring(0, context.Request.PathBase.Value.Length) ?? "";

			var list = new List<SwaggerSecurityRequirement> { new SwaggerSecurityRequirement { { "Authorization", new[] { "Authorization" } } } };
			var swaggerOperations = new SwaggerOperations {{SwaggerOperationMethod.Get, new SwaggerOperation {Security = list}}};
			doc.Paths.Add("/command", swaggerOperations);
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
					Schema = generator.GenerateWithReferenceAndNullability<JsonSchema4>(typeof(ResultDto), null, false, schemaResolver, null).GetAwaiter().GetResult()
				};
				swaggerOperation.Responses.Add("200", swaggerResponse);

				var param = new SwaggerParameter
				{
					Description = "Command arguments for " + opname,
					Kind = SwaggerParameterKind.Body,
					Schema = generator.GenerateWithReferenceAndNullability<JsonSchema4>(cmd, null, false, schemaResolver, null).GetAwaiter().GetResult()
				};
				swaggerOperation.Parameters.Add(param);

				operations.Add(SwaggerOperationMethod.Post, swaggerOperation);
				doc.Paths.Add("/command/" + opname, operations);
			}
			
			swaggerOperations = new SwaggerOperations {{SwaggerOperationMethod.Get, new SwaggerOperation {Security = list}}};
			doc.Paths.Add("/query", swaggerOperations);
			
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

				operations.Add(SwaggerOperationMethod.Post, swaggerOperation);
				doc.Paths.Add($"/query/{typename}/{opname}", operations);
				
				var swaggerResponse = new SwaggerResponse
				{
					Schema = generator.GenerateWithReferenceAndNullability<JsonSchema4>(typeof(QueryResultDto<>).MakeGenericType(query.Item3), null, false, schemaResolver, null).GetAwaiter().GetResult()
				};
				swaggerOperation.Responses.Add("200", swaggerResponse);
				
				var param = new SwaggerParameter
				{
					Description = "Query arguments for " + opname,
					Kind = SwaggerParameterKind.Body,
					Schema = generator.GenerateWithReferenceAndNullability<JsonSchema4>(query.Item2, null, false, schemaResolver, null).GetAwaiter().GetResult()
				};
				swaggerOperation.Parameters.Add(param);
			}
			
			doc.SecurityDefinitions.Add("Authorization", new SwaggerSecurityScheme { Name = "Authorization", Description = "Bearer token", Type = SwaggerSecuritySchemeType.ApiKey, In = SwaggerSecurityApiKeyLocation.Header });
			doc.Consumes = new List<string> { "application/json" };
			doc.Produces = new List<string> { "application/json" };

			return doc.ToJson();
		}

		public override Task Invoke(IOwinContext context)
		{
			if (context.Request.Path.HasValue && string.Equals(context.Request.Path.Value.Trim('/'), url.Trim('/'), StringComparison.OrdinalIgnoreCase))
			{
				context.Response.StatusCode = 200;
				context.Response.Headers["Content-Type"] = "application/json; charset=utf-8";

				var doc = generateDoc(context);
				return context.Response.WriteAsync(doc);
			}

			// Call the next delegate/middleware in the pipeline
			return Next.Invoke(context);
		}
	}
}
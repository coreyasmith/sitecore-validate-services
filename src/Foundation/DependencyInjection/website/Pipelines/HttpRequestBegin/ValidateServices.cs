using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Pipelines.HttpRequest;

namespace CoreySmith.Foundation.DependencyInjection.Pipelines.HttpRequestBegin
{
    public class ValidateServices : HttpRequestProcessor
    {
	    private static readonly Lazy<IServiceProvider> ServiceProviderFactory = new Lazy<IServiceProvider>(() =>
	    {
		    var serviceProviderBuilder = new ValidatingServiceProviderBuilder();
		    return serviceProviderBuilder.Build();
	    });

        protected List<string> AssemblyPrefixes { get; } = new List<string>();

        public override void Process(HttpRequestArgs args)
        {
            IEnumerable<Exception> exceptions;
            var validatingServiceProvider = ServiceProviderFactory.Value;
            using (var scope = validatingServiceProvider.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var serviceCollection = serviceProvider.GetRequiredService<IServiceCollection>();
                exceptions = ValidateServiceCollection(serviceProvider, serviceCollection).ToList();
            }

            if (!exceptions.Any())
            {
                return;
            }

            RenderExceptions(exceptions, args.HttpContext);
            args.AbortPipeline();
        }

        private IEnumerable<Exception> ValidateServiceCollection(
            IServiceProvider serviceProvider,
            IServiceCollection serviceCollection)
        {
            var exceptions = new List<Exception>();
            var validatableServices = serviceCollection.Where(IsValidatableService);
            foreach (var service in validatableServices)
            {
                try
                {
                    serviceProvider.GetRequiredService(service.ServiceType);
                }
                catch (Exception ex)
                {
                    exceptions.Add(new Exception($"{service.ServiceType}", ex));
                }
            }
            return exceptions;
        }

        private bool IsValidatableService(ServiceDescriptor descriptor)
        {
            return (!descriptor.ServiceType.IsGenericType || descriptor.ServiceType.IsConstructedGenericType)
                   && AssemblyPrefixes.Any(prefix => descriptor.ServiceType.Assembly.FullName.StartsWith(prefix)
                                                     || (descriptor.ImplementationType?.Assembly.FullName.StartsWith(prefix) ?? false)
                                                     || (descriptor.ImplementationInstance?.GetType().Assembly.FullName.StartsWith(prefix) ?? false));
        }

        private static void RenderExceptions(IEnumerable<Exception> exceptions, HttpContextBase httpContext)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body>");
            sb.Append("<h1>Some services failed validation.</h1>");
            sb.Append("<table><tr><th>Service</th><th>Issue</th></tr>");
            foreach (var exception in exceptions)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{exception.Message}</td>");
                if (exception.InnerException != null)
                {
                    sb.Append($"<td>{exception.InnerException.Message}</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            sb.Append("</body></html>");
            httpContext.Response.Write(sb.ToString());
            httpContext.Response.ContentType = "text/html";
            httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            httpContext.Response.End();
        }
    }
}

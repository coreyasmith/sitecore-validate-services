using System;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace CoreySmith.Foundation.DependencyInjection
{
    public class ValidatingServiceProviderBuilder : DefaultServiceProviderBuilder
    {
        protected override IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(serviceCollection);
            return serviceCollection.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true
            });
        }
    }
}

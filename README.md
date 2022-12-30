# ✅ Validate Sitecore Service Registrations

This repository demonstrates how to validate services registered with the
Microsoft Dependency Injection framework in Sitecore [as demonstrated in my blog
post][1].

The code in this repository is a pared-down fork of the `custom-images` in
[Sitecore's `docker-examples` repository][2]. That repository is an excellent
reference for getting started with Sitecore and Docker.

## 🏗️ Setup

1. Run [`init.ps1`][3] to initialize the repository.
2. Run `docker-compose up -d --build` from the root of the repository.
3. Once Sitecore is running, deploy the solution from Visual Studio with the
   [`DockerDeploy`][4] publish profile.
4. Navigate to the site at <https://cm.validateservices.localhost>.

## 🚀 Usage

The [`ValidateServices`][5] pipeline processor executes on every request. To
trigger the validate services page, add an invalid service registration to the
container. The easiest way is to create a scoped and singleton service and
add them to the `serviceCollection` in the `BuildServiceProvider` method of
[`ValidatingServiceProviderBuilder`][6] like this:

```csharp
public class ScopedService
{
}
```

```csharp
public class SingletonService
{
  public SingletonService(ScopedService service)
  {
  }
}
```

```csharp
public class ValidatingServiceProviderBuilder : DefaultServiceProviderBuilder
{
  protected override IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection)
  {
    serviceCollection.AddScoped<ScopedService>();
    serviceCollection.AddSingleton<SingletonService>();

    serviceCollection.AddSingleton(serviceCollection);
    return serviceCollection.BuildServiceProvider(new ServiceProviderOptions
    {
      ValidateScopes = true
    });
  }
}
```

Redeploy and the validate services page will be displayed next time you navigate
to the site.

[1]: https://www.coreysmith.co/sitecore-dependency-injection-validate-services/
[2]: https://github.com/Sitecore/docker-examples/
[3]: init.ps1
[4]: src/Foundation/DependencyInjection/website/Properties/PublishProfiles/DockerDeploy.pubxml
[5]: src/Foundation/DependencyInjection/website/Pipelines/HttpRequestBegin/ValidateServices.cs
[6]: src/Foundation/DependencyInjection/website/ValidatingServiceProviderBuilder.cs

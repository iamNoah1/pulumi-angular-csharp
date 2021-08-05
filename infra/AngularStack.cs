using System.Linq;
using System;
using System.IO;
using Pulumi;
using Cdn = Pulumi.AzureNative.Cdn;
using Resources = Pulumi.AzureNative.Resources;
using Storage = Pulumi.AzureNative.Storage;

class AngularStack : Stack
{
    [Output("staticEndpoint")]
    public Output<string> StaticEndpoint { get; set; }

    [Output("cdnEndpoint")]

    public Output<string> CdnEndpoint { get; set; }
    public AngularStack()
    {

        var resourceGroup = new Resources.ResourceGroup("resourceGroup");

        var storageAccount = new Storage.StorageAccount("storageaccount", new Storage.StorageAccountArgs
        {
            Kind = Storage.Kind.StorageV2,
            ResourceGroupName = resourceGroup.Name,
            Sku = new Storage.Inputs.SkuArgs
            {
                Name = Storage.SkuName.Standard_LRS,
            },
        });

        // Enable static website support
        var staticWebsite = new Storage.StorageAccountStaticWebsite("staticWebsite", new Storage.StorageAccountStaticWebsiteArgs
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            IndexDocument = "index.html",
            Error404Document = "404.html",
        });

        var files = Directory.EnumerateFiles("../webapp/dist/webapp").Select(Path.GetFileName).ToArray();
        Array.ForEach(files, (filename) =>
        {
            new Storage.Blob(filename, new Storage.BlobArgs
            {
                ResourceGroupName = resourceGroup.Name,
                AccountName = storageAccount.Name,
                ContainerName = staticWebsite.ContainerName,
                Source = new FileAsset($"../webapp/dist/webapp/{filename}"),
                ContentType = "text/html",
            });
        });

        // Web endpoint to the website
        this.StaticEndpoint = storageAccount.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web);

        // (Optional) Add a CDN in front of the storage account.
        var profile = new Cdn.Profile("profile", new Cdn.ProfileArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Location = "global",
            Sku = new Cdn.Inputs.SkuArgs
            {
                Name = Cdn.SkuName.Standard_Microsoft,
            },
        });

        var endpointOrigin = storageAccount.PrimaryEndpoints.Apply(pe => pe.Web.Replace("https://", "").Replace("/", ""));

        var endpoint = new Cdn.Endpoint("endpoint", new Cdn.EndpointArgs
        {
            EndpointName = storageAccount.Name.Apply(sa => $"cdn-endpnt-{sa}"),
            IsHttpAllowed = false,
            IsHttpsAllowed = true,
            OriginHostHeader = endpointOrigin,
            Origins =
            {
                new Cdn.Inputs.DeepCreatedOriginArgs
                {
                    HostName = endpointOrigin,
                    HttpsPort = 443,
                    Name = "origin-storage-account",
                },
            },
            ProfileName = profile.Name,
            QueryStringCachingBehavior = Cdn.QueryStringCachingBehavior.NotSet,
            ResourceGroupName = resourceGroup.Name,
        });

        // CDN endpoint to the website.
        // Allow it some time after the deployment to get ready.
        this.CdnEndpoint = endpoint.HostName.Apply(hostName => $"https://{hostName}");
    }
}

using Pulumi.Azure.Core;
using Pulumi.Azure.AppService;
using CosmosDB = Pulumi.Azure.CosmosDB;
using Storage = Pulumi.Azure.Storage;
using System.Collections.Generic;
using Pulumi.Azure.AppService.Inputs;
using Pulumi;

namespace pulumi_cosmos_functions
{
  internal class MyCosmosFunctionStack : Stack
  {
    public MyCosmosFunctionStack() 
    {
      var config = new Config();
      var location = config.Require("Location");

      var rg = new ResourceGroup("mjgpulumiexample-rg", new ResourceGroupArgs { Location = location});

      // CosmosDB
      var comsos = new CosmosDB.Account("mjgpulumidb", new CosmosDB.AccountArgs
      {
        ResourceGroupName = rg.Name,
        ConsistencyPolicy = new CosmosDB.Inputs.AccountConsistencyPolicyArgs {
          ConsistencyLevel = "Session"
        },
        GeoLocations = new List<CosmosDB.Inputs.AccountGeoLocationArgs> {
          new CosmosDB.Inputs.AccountGeoLocationArgs {
            Location = location,
            FailoverPriority = 0
          }
        },
        OfferType = "Standard",
        Capabilities = new List<CosmosDB.Inputs.AccountCapabilityArgs> {
          new CosmosDB.Inputs.AccountCapabilityArgs {
            Name = "EnableServerless"
          }
        }
      });

      var database = new CosmosDB.SqlDatabase("pulumi-db", new CosmosDB.SqlDatabaseArgs {
        Name = "pulumi-db",
        ResourceGroupName = rg.Name,
        AccountName = comsos.Name
      });

      var collection = new CosmosDB.SqlContainer("functiondocs", new CosmosDB.SqlContainerArgs {
        Name = "functiondocs",
        ResourceGroupName = rg.Name,
        AccountName = comsos.Name,
        DatabaseName = database.Name
      });

      // Storage
      var storage = new Storage.Account("mjgpulumifuncsa", new Storage.AccountArgs {
        ResourceGroupName = rg.Name,
        AccountReplicationType = "LRS",
        AccountTier = "Standard"
      });

      // Function App
      var asp = new Plan("mjgpulumiexamplefunction-asp", new PlanArgs {
        ResourceGroupName = rg.Name,
        Kind = "Linux",
        Sku = new PlanSkuArgs
        {
          Tier = "PremiumV2",
          Size = "P1v2"
        },
        Reserved = true
      });

      var func = new FunctionApp("mjgpulumiexample-fn", new FunctionAppArgs {
        ResourceGroupName = rg.Name,
        AppServicePlanId = asp.Id,
        AppSettings = {
          {"CosmosDBConnection", comsos.ConnectionStrings.First()}
        },
        StorageAccountName = storage.Name,
        StorageAccountAccessKey = storage.PrimaryAccessKey,
        Version = "~3",
        OsType = "linux",
        SiteConfig = new FunctionAppSiteConfigArgs
        {
          AlwaysOn = true
        }
      });
    }
  }
}
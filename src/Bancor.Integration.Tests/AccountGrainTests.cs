﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Bancor.Core.Grains;
using Bancor.Core.Grains.Interfaces.Grains;
using Bancor.Core.Grains.Interfaces.Repository;
using Bancor.Infrastructure;
using Bancor.Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NPoco;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Orleans.TestingHost;
using Shouldly;
using Xunit;

namespace Bancor.Integration.Tests
{
    public class AccountGrainTests
    {
        [Fact]
        public async void Should_Store_One_Account()
        {
            // Arrange

            var dbName = Guid.NewGuid().ToString();
            var db = BankorDbFactory.Create(dbName);
            BankorDbFactory.Upgrade(dbName);
            SiloConfigurator.Db = db;

            var builder = new TestClusterBuilder(1);

            builder.AddSiloBuilderConfigurator<SiloConfigurator>();

            var testCluster = builder.Build();

            await testCluster.DeployAsync();

            var grain = testCluster.GrainFactory.GetGrain<IAccountGrain>(2000);

            // Act
            await grain.HasNewName("Savings account");
            var name = await grain.GetName();

            // Assert
            name.ShouldBe("Savings account");
        }
    }

    public class SiloConfigurator : ISiloBuilderConfigurator
    {
        public static IDatabase Db { get; set; }
        public void Configure(ISiloHostBuilder hostBuilder)
        {

            hostBuilder
                .ConfigureServices(s => s.TryAddSingleton<IGrainStorage, CustomerStorageProvider>())
                .ConfigureServices(s => s.TryAddTransient<ICustomerRepository, CustomerRepository>())
                .ConfigureServices(s => s.TryAddSingleton<IDatabase>(Db))
                .ConfigureServices(s =>
                    s.AddSingletonNamedService<IGrainStorage>("CustomerStorageProvider",
                        (x, y) => new CustomerStorageProvider(Db,
                            (IGrainFactory)x.GetService(typeof(IGrainFactory)))))
                .ConfigureServices(s =>
                    s.AddSingletonNamedService<IGrainStorage>("AccountsStorageProvider",
                        (x, y) => new AccountsStorageProvider(Db)))
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .UseTransactions();
        }
    }
}

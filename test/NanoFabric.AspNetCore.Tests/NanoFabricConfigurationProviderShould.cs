﻿using NanoFabric.Core;
using NanoFabric.RegistryHost.ConsulRegistry;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NanoFabric.AspNetCore.Tests
{
    public class NanoFabricConfigurationProviderShould
    {
        private readonly Configuration.NanoFabricConfigurationProvider _inMemoryProvider;
        private readonly Configuration.NanoFabricConfigurationProvider _consulProvider;

        public NanoFabricConfigurationProviderShould()
        {
            _inMemoryProvider = new Configuration.NanoFabricConfigurationProvider(() => GetInMemoryRegistryHostAsync().Result);
            _consulProvider = new Configuration.NanoFabricConfigurationProvider(() => GetConsulRegistryHostAsync().Result);
        }

        private async Task<IRegistryHost> GetInMemoryRegistryHostAsync()
        {
            var registryHost = new InMemoryRegistryHost();
            await registryHost.KeyValuePutAsync("key1", "value1");
            await registryHost.KeyValuePutAsync("key2", "value2");
            await registryHost.KeyValuePutAsync("folder/key3", "value3");
            await registryHost.KeyValuePutAsync("folder/key4", "value4");

            return registryHost;
        }

        private async Task<IRegistryHost> GetConsulRegistryHostAsync()
        {
            var configuration = new ConsulRegistryHostConfiguration() { HostName = "localhost" };

            var registryHost = new ConsulRegistryHost(configuration);

            await registryHost.KeyValuePutAsync("key1", "value1");
            await registryHost.KeyValuePutAsync("key2", "value2");
            await registryHost.KeyValuePutAsync("folder/key3", "value3");
            await registryHost.KeyValuePutAsync("folder/key4", "value4");

            return registryHost;
        }

        private void TryGetTest(IConfigurationProvider provider)
        {
            string value;
            Assert.True(provider.TryGet("key1", out value));
            Assert.Equal("value1", value);

            Assert.True(provider.TryGet("key2", out value));
            Assert.Equal("value2", value);
        }

        [Fact]
        public void TryGet()
        {
            TryGetTest(_inMemoryProvider);
            TryGetTest(_consulProvider);
        }

        private void SetTest(IConfigurationProvider provider)
        {
            string key = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            string expectedValue = nameof(NanoFabricConfigurationProviderShould.Set);

            provider.Set(key, expectedValue);
            string value;
            Assert.True(provider.TryGet(key, out value));
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void Set()
        {
            SetTest(_inMemoryProvider);
            SetTest(_consulProvider);
        }

        private void GetChildKeysTest(IConfigurationProvider provider)
        {
            var result = provider.GetChildKeys(Enumerable.Empty<string>(), "folder/");
            Assert.Equal(new[] { "folder/key3", "folder/key4" }, result);
        }

        [Fact]
        public void GetChildKeys()
        {
            GetChildKeysTest(_inMemoryProvider);
            GetChildKeysTest(_consulProvider);
        }
    }
}

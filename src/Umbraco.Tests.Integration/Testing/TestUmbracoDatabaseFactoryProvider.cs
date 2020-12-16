using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Tests.Integration.Testing
{
    /// <summary>
    /// I want to be able to create a database for integration testsing without setting the connection string on the
    /// singleton database factory forever.
    /// </summary>
    public class TestUmbracoDatabaseFactoryProvider
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptions<GlobalSettings> _globalSettings;
        private readonly IOptions<ConnectionStrings> _connectionStrings;
        private readonly Lazy<IMapperCollection> _mappers;
        private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;

        public TestUmbracoDatabaseFactoryProvider(
            ILoggerFactory loggerFactory,
            IOptions<GlobalSettings> globalSettings,
            IOptions<ConnectionStrings> connectionStrings,
            Lazy<IMapperCollection> mappers,
            IDbProviderFactoryCreator dbProviderFactoryCreator)
        {
            _loggerFactory = loggerFactory;
            _globalSettings = globalSettings;
            _connectionStrings = connectionStrings;
            _mappers = mappers;
            _dbProviderFactoryCreator = dbProviderFactoryCreator;
        }

        public IUmbracoDatabaseFactory Create()
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            return new UmbracoDatabaseFactory(
                _loggerFactory.CreateLogger<UmbracoDatabaseFactory>(),
                _loggerFactory,
                _globalSettings.Value,
                _connectionStrings.Value,
                _mappers,
                _dbProviderFactoryCreator);
        }
    }
}

using bvmfscrapper.helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace bvmfscrapper
{
    public static class DbContextExtensions
    {
        public static DbContext UseLog4NetAsLogger(this DbContext context)
        {
            var serviceProvider = context.GetInfrastructure<IServiceProvider>();
            var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
            loggerFactory.AddProvider(Log4NetProvider.Instance);
            return context;
        }

        public static T UseLog4NetAsLogger<T>(this T context) where T: DbContext
        {
            var serviceProvider = context.GetInfrastructure<IServiceProvider>();
            var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
            loggerFactory.AddProvider(Log4NetProvider.Instance);
            return context;
        }
    }
}

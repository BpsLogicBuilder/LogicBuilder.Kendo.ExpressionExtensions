using Microsoft.EntityFrameworkCore;

namespace LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests.Data.Configurations
{
    interface ITableConfiguration
    {
        void Configure(ModelBuilder modelBuilder);
    }
}

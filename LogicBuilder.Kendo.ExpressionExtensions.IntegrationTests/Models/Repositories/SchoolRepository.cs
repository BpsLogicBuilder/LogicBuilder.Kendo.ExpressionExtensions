using AutoMapper;
using LogicBuilder.EntityFrameworkCore.Repositories;
using LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests.Data.Stores;

namespace LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests.Models.Repositories
{
    public class SchoolRepository(ISchoolStore store, IMapper mapper) : ContextRepositoryBase(store, mapper), ISchoolRepository
    {
    }
}

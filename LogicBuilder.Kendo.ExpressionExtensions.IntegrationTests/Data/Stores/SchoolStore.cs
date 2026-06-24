using LogicBuilder.EntityFrameworkCore.Crud.DataStores;

namespace LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests.Data.Stores
{
    public class SchoolStore(SchoolContext context) : StoreBase(context), ISchoolStore
    {
    }
}

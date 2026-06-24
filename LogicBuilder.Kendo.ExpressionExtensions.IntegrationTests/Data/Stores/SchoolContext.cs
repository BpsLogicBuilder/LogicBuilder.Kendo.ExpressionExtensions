using Microsoft.EntityFrameworkCore;

namespace LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests.Data.Stores
{
    public class SchoolContext(DbContextOptions<SchoolContext> options) : BaseDbContext(options)
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<OfficeAssignment> OfficeAssignments { get; set; }
        public DbSet<CourseAssignment> CourseAssignments { get; set; }
    }
}

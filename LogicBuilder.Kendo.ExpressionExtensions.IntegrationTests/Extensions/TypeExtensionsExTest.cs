using LogicBuilder.Kendo.ExpressionExtensions.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests.Extensions
{
    public class TypeExtensionsExTest
    {
        #region GetUnderlyingElementType Tests

        private static readonly int[] sourceArray0 = [1, 2, 3];

        [Fact]
        public void GetUnderlyingElementType_ReturnsCorrectType_ForIQueryableOfInt()
        {
            // Arrange
            Expression<Func<IQueryable<int>>> expression = () => sourceArray0.AsQueryable();

            // Act
            Type result = expression.Body.GetUnderlyingElementType();

            // Assert
            Assert.Equal(typeof(int), result);
        }

        private static readonly string[] sourceArray1 = ["a", "b"];

        [Fact]
        public void GetUnderlyingElementType_ReturnsCorrectType_ForIEnumerableOfString()
        {
            // Arrange
            Expression<Func<IQueryable<string>>> expression = () => sourceArray1.AsQueryable();

            // Act
            Type result = expression.Body.GetUnderlyingElementType();

            // Assert
            Assert.Equal(typeof(string), result);
        }

        [Fact]
        public void GetUnderlyingElementType_ThrowsArgumentException_ForNonGenericType()
        {
            // Arrange
            Expression<Func<int>> expression = () => 42;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => expression.Body.GetUnderlyingElementType());
        }

        [Fact]
        public void GetUnderlyingElementType_ThrowsArgumentException_ForMultipleGenericArguments()
        {
            // Arrange
            Expression<Func<Tuple<int, string>>> expression = () => Tuple.Create(1, "test");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => expression.Body.GetUnderlyingElementType());
        }
        #endregion

        #region IsValueType Tests
        [Fact]
        public void IsValueType_ReturnsTrue_ForInt()
        {
            // Arrange
            Type type = typeof(int);

            // Act
            bool result = type.IsValueType();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValueType_ReturnsTrue_ForStruct()
        {
            // Arrange
            Type type = typeof(DateTime);

            // Act
            bool result = type.IsValueType();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValueType_ReturnsFalse_ForString()
        {
            // Arrange
            Type type = typeof(string);

            // Act
            bool result = type.IsValueType();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValueType_ReturnsFalse_ForClass()
        {
            // Arrange
            Type type = typeof(object);

            // Act
            bool result = type.IsValueType();

            // Assert
            Assert.False(result);
        }
        #endregion

        #region FirstSortableProperty Tests
        [Fact]
        public void FirstSortableProperty_ReturnsFirstProperty_WithPredefinedType()
        {
            // Arrange
            Type type = typeof(TestClassWithSortableProperties);

            // Act
            string result = type.FirstSortableProperty();

            // Assert
            Assert.Equal("Id", result);
        }

        [Fact]
        public void FirstSortableProperty_ReturnsCorrectProperty_WhenFirstPropertyIsNotSortable()
        {
            // Arrange
            Type type = typeof(TestClassWithNonSortableFirst);

            // Act
            string result = type.FirstSortableProperty();

            // Assert
            Assert.Equal("Name", result);
        }

        [Fact]
        public void FirstSortableProperty_ThrowsNotSupportedException_WhenNoSortableProperties()
        {
                       // Arrange
            Type type = typeof(object);
            // Act & Assert
            Assert.Throws<NotSupportedException>(type.FirstSortableProperty);

        }
        #endregion FirstSortableProperty Tests

        private class TestClassWithSortableProperties
        {
            public int Id { get; set; } //NOSONAR - required for testing
            public string Name { get; set; } //NOSONAR - required for testing
        }

        private class TestClassWithNonSortableFirst
        {
            public TypeExtensionsExTest NonSortable { get; set; } //NOSONAR - required for testing
            public string Name { get; set; } //NOSONAR - required for testing
        }
    }
}

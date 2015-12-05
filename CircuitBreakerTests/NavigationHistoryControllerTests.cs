using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CircuitBreaker;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace CircuitBreakerTests
{
    [TestFixture]
    public class NavigationHistoryControllerTests
    {
        [Test]
        public void GetHistory_WhenTimeoutAccrues10Times_ActivatePolicy()
        {
            // Arrage
            var dataProviderMock = Substitute.For<INavigationHistoryProvider>();
            var controller = new NavigationHistoryController(dataProviderMock);

            dataProviderMock.Get().Throws(CreateSqlException(-2));

            for (int i = 0; i < 10; i++)
            {
                controller.GetHistory();
            }

            // Act
            var result = controller.GetHistory();

            // Assert
            result.Should().NotBeNull().And.BeEmpty();
        }
        private SqlException CreateSqlException(int number)
        {
            var collectionConstructor = typeof(SqlErrorCollection)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, //visibility
                null, //binder
                new Type[0],
                null);
            var addMethod = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);
            var errorCollection = (SqlErrorCollection)collectionConstructor.Invoke(null);

            var errorConstructor = typeof(SqlError).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[]
                {
            typeof (int), typeof (byte), typeof (byte), typeof (string), typeof(string), typeof (string),
            typeof (int), typeof (uint)
                }, null);
            var error =
                errorConstructor.Invoke(new object[] { number, (byte)0, (byte)0, "server", "errMsg", "proccedure", 100, (uint)0 });

            addMethod.Invoke(errorCollection, new[] { error });

            var constructor = typeof(SqlException)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, //visibility
                    null, //binder
                    new[] { typeof(string), typeof(SqlErrorCollection), typeof(Exception), typeof(Guid) },
                    null); //param modifiers

            return (SqlException)constructor.Invoke(new object[] { "Error message", errorCollection, new DataException(), Guid.NewGuid() });
        }
    }
}

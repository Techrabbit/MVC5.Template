﻿using Moq;
using NUnit.Framework;
using Template.Components.Services;
using Template.Data.Core;

namespace Template.Tests.Tests.Components.Services
{
    [TestFixture]
    public class BaseServiceTests
    {
        private BaseService service;

        [SetUp]
        public void SetUp()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mock = new Mock<BaseService>(unitOfWorkMock.Object) { CallBase = true };

            service = mock.Object;
        }

        [TearDown]
        public void TearDown()
        {
            service.Dispose();
        }

        #region Method: Dispose()

        [Test]
        public void Dispose_CanDisposeMultipleTimes()
        {
            service.Dispose();
            service.Dispose();
        }

        #endregion
    }
}

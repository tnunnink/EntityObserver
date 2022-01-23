using AutoFixture;
using FluentAssertions;
using EntityObserver.Tests.TestModels;
using EntityObserver.Tests.TestObservers;
using NUnit.Framework;

namespace EntityObserver.Tests
{
    [TestFixture]
    public class SimpleValidationTests
    {
        private Address _model;
        private Fixture _fixture;
        private AddressObserver _observer;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _model = _fixture.Create<Address>();
            _observer = new AddressObserver(_model);
        }

        [Test]
        public void HasErrors_Valid_ShouldBeFalse()
        {
            _observer.HasErrors.Should().BeFalse();
        }

        [Test]
        public void HasErrors_Invalid_ShouldBeTrue()
        {
            _observer.City = string.Empty;

            _observer.HasErrors.Should().BeTrue();
        }
        
        [Test]
        public void HasErrors_ResolveError_ShouldBeFalse()
        {
            _observer.City = string.Empty;
            _observer.HasErrors.Should().BeTrue();

            _observer.City = _fixture.Create<string>();
            _observer.HasErrors.Should().BeFalse();
        }

        [Test]
        public void ErrorsChanged_HasErrors_ShouldBeRaised()
        {
            var monitor = _observer.Monitor();
            
            _observer.City = string.Empty;

            monitor.Should().Raise(nameof(_observer.ErrorsChanged));
        }
        
        [Test]
        public void ErrorsChanged_ClearErrors_ShouldBeRaised()
        {
            _observer.City = string.Empty;
            
            var monitor = _observer.Monitor();

            _observer.City = _fixture.Create<string>();

            monitor.Should().Raise(nameof(_observer.ErrorsChanged));
        }
    }
}
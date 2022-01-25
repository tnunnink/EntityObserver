using AutoFixture;
using EntityObserver.Tests.TestEntities;
using EntityObserver.Tests.TestObservers;
using FluentAssertions;
using NUnit.Framework;

namespace EntityObserver.Tests
{
    [TestFixture]
    public class InitializeObserverTests
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void New_NullObserverMembers_ShouldHaveNullInstances()
        {
            var person = new Person();
            var observer = new PersonObserver(person);

            observer.Address.Should().BeNull();
            observer.Emails.Should().BeNull();
            observer.Cars.Should().BeNull();
        }
    }
}
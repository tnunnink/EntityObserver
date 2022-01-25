using System;
using AutoFixture;
using EntityObserver.Tests.TestEntities;
using EntityObserver.Tests.TestObservers;
using FluentAssertions;
using NUnit.Framework;

namespace EntityObserver.Tests
{
    [TestFixture]
    public class InvalidObserverTests
    {
        private Fixture _fixture;
        private Person _person;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _person = _fixture.Create<Person>();
        }

        [Test]
        public void New_RegisterNullObserver_ShouldThrowArgumentNullException()
        {
            FluentActions.Invoking(() => new RegisterNullObserver(_person)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void GetValue_InvalidNameObserver_ShouldThrowArgumentException()
        {
            var observer = new InvalidMemberNameObserver(_person);

            FluentActions.Invoking(() => observer.First).Should().Throw<ArgumentException>()
                .WithMessage("Could not find property info for First");
        }

        [Test]
        public void GetValue_EmptyPropertyName_ShouldThrowArgumentException()
        {
            var observer = new InvalidMemberNameObserver(_person);

            FluentActions.Invoking(() => observer.Last).Should().Throw<ArgumentException>()
                .WithMessage("Property name can not be null or empty");
        }
    }
}
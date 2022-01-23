using AutoFixture;
using EntityObserver.Tests.TestEntities;
using EntityObserver.Tests.TestObservers;
using FluentAssertions;
using NUnit.Framework;

namespace EntityObserver.Tests
{
    [TestFixture]
    public class ComplexObserverTests
    {
        
        private Person _person;
        private Fixture _fixture;
        private PersonObserver _observable;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _person = _fixture.Create<Person>();
            _observable = new PersonObserver(_person);
        }
        
        [Test]
        public void New_Valid_ShouldNotBeNull()
        {
            var observable = new PersonObserver(_person);

            observable.Should().NotBeNull();
        }

        [Test]
        public void SetComplexMember_NewValue_IsChangedShouldBeTrue()
        {
            _observable.Address.City = _fixture.Create<string>();

            _observable.IsChanged.Should().BeTrue();
        }

        [Test]
        public void SetComplexMember_NewValue_ShouldRaisePropertyChangedEvents()
        {
            var monitor = _observable.Monitor();
            
            _observable.Address.Zip = _fixture.Create<int>();

            //monitor.Should().RaisePropertyChangeFor(m => m.Address.Zip);
            monitor.Should().RaisePropertyChangeFor(m => m.Address.IsChanged);
            monitor.Should().RaisePropertyChangeFor(m => m.IsChanged);
        }
    }
}
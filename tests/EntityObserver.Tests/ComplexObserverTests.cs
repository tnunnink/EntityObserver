using System.Linq;
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
        public void New_Valid_EntitiesShouldBeSameAsProvided()
        {
            var observable = new PersonObserver(_person);

            observable.Entity.Should().BeSameAs(_person);
            observable.Address.Entity.Should().BeSameAs(_person.Address);
            observable.Cars.First().Entity.Should().BeSameAs(_person.Cars.First());
            observable.Cars.Last().Entity.Should().BeSameAs(_person.Cars.Last());
        }

        [Test]
        public void SetComplexMember_NewValue_IsChangedShouldBeTrue()
        {
            _observable.Address.City = _fixture.Create<string>();

            _observable.IsChanged.Should().BeTrue();
        }

        [Test]
        public void SetComplexMember_NewValue_ShouldRaisePropertyChangedForBase()
        {
            var monitor = _observable.Monitor();

            _observable.Address.Zip = _fixture.Create<int>();
            
            monitor.Should().RaisePropertyChangeFor(m => m.IsChanged);
        }
        
        [Test]
        public void SetComplexMember_NewValue_ShouldRaisePropertyChangedForMember()
        {
            var monitor = _observable.Address.Monitor();

            _observable.Address.Zip = _fixture.Create<int>();
            
            monitor.Should().RaisePropertyChangeFor(m => m.Zip);
            monitor.Should().RaisePropertyChangeFor(m => m.IsChanged);
        }

        [Test]
        public void SetComplexMember_SameValue_ShouldNotRaisePropertyChangedForBase()
        {
            var monitor = _observable.Monitor();

            _observable.Address.Zip = _person.Address.Zip;
            
            monitor.Should().NotRaisePropertyChangeFor(m => m.IsChanged);
        }
        
        [Test]
        public void SetComplexMember_SameValue_ShouldNotRaisePropertyChangedForMember()
        {
            var monitor = _observable.Address.Monitor();

            _observable.Address.Zip = _person.Address.Zip;
            
            monitor.Should().NotRaisePropertyChangeFor(m => m.Zip);
            monitor.Should().NotRaisePropertyChangeFor(m => m.IsChanged);
        }
    }
}
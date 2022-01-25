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
        private PersonObserver _observer;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _person = _fixture.Create<Person>();
            _observer = new PersonObserver(_person);
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

        [Test] //todo not sure hot exactly to test this since its private
        public void New_RegisterMultipleSameObserver_ShouldHaveSingle()
        {
            var observable = new RegisterMultipleSameObserver(_person);

            observable.Should().NotBeNull();
        }

        [Test]
        public void SetComplexMember_NewValue_IsChangedShouldBeTrue()
        {
            _observer.Address.City = _fixture.Create<string>();

            _observer.IsChanged.Should().BeTrue();
        }

        [Test]
        public void SetComplexMember_NewValue_ShouldRaisePropertyChangedForBase()
        {
            var monitor = _observer.Monitor();

            _observer.Address.Zip = _fixture.Create<int>();
            
            monitor.Should().RaisePropertyChangeFor(m => m.IsChanged);
        }
        
        [Test]
        public void SetComplexMember_NewValue_ShouldRaisePropertyChangedForMember()
        {
            var monitor = _observer.Address.Monitor();

            _observer.Address.Zip = _fixture.Create<int>();
            
            monitor.Should().RaisePropertyChangeFor(m => m.Zip);
            monitor.Should().RaisePropertyChangeFor(m => m.IsChanged);
        }

        [Test]
        public void SetComplexMember_SameValue_ShouldNotRaisePropertyChangedForBase()
        {
            var monitor = _observer.Monitor();

            _observer.Address.Zip = _person.Address.Zip;
            
            monitor.Should().NotRaisePropertyChangeFor(m => m.IsChanged);
        }
        
        [Test]
        public void SetComplexMember_SameValue_ShouldNotRaisePropertyChangedForMember()
        {
            var monitor = _observer.Address.Monitor();

            _observer.Address.Zip = _person.Address.Zip;
            
            monitor.Should().NotRaisePropertyChangeFor(m => m.Zip);
            monitor.Should().NotRaisePropertyChangeFor(m => m.IsChanged);
        }

        [Test]
        public void GetOriginalValue_IsChanged_ShouldBeOriginal()
        {
            var newValue = _fixture.Create<string>();
            var original = _observer.Address.City;

            _observer.Address.City = newValue;

            var originalValue = _observer.Address.GetOriginalValue(m => m.City);
            originalValue.Should().Be(original);
        }

        [Test]
        public void GetOriginalValue_NoChange_ShouldBeCurrent()
        {
            var originalValue = _observer.Address.GetOriginalValue(m => m.City);
            
            originalValue.Should().Be(_observer.Address.City);
        }
    }
}
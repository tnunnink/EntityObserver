using System;
using AutoFixture;
using EntityObserver.Tests.TestEntities;
using FluentAssertions;
using EntityObserver.Tests.TestObservers;
using NUnit.Framework;

namespace EntityObserver.Tests
{
    [TestFixture]
    public class SimpleObserverTests
    {
        private Address _entity;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _entity = _fixture.Create<Address>();
        }

        [Test]
        public void New_Null_ShouldThrowArgumentNullException()
        {
            FluentActions.Invoking(() => new AddressObserver(null!)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void New_Valid_ShouldNotBeNull()
        {
            var observer = new AddressObserver(_entity);

            observer.Should().NotBeNull();
        }

        [Test]
        public void New_Valid_ModelShouldBeSame()
        {
            var observer = new AddressObserver(_entity);

            observer.Entity.Should().BeSameAs(_entity);
        }

        [Test]
        public void GetValue_WhenCalled_ShouldBeExpected()
        {
            var observer = new AddressObserver(_entity);

            var value = observer.Id;

            value.Should().Be(_entity.Id);
        }

        [Test]
        public void SetValue_NewValue_ShouldBeExpected()
        {
            var newValue = _fixture.Create<string>();
            var observer = new AddressObserver(_entity);

            observer.Street = newValue;

            observer.Street.Should().Be(newValue);
        }

        [Test]
        public void SetValue_NewValue_ShouldRaisePropertyChangedEvent()
        {
            var newValue = _fixture.Create<string>();
            var observer = new AddressObserver(_entity);
            var monitor = observer.Monitor();

            observer.Street = newValue;

            monitor.Should().RaisePropertyChangeFor(x => x.Street);
        }

        [Test]
        public void SetValue_SameValue_ShouldBeExpected()
        {
            var observer = new AddressObserver(_entity);

            observer.Street = _entity.Street;

            observer.Street.Should().Be(_entity.Street);
        }

        [Test]
        public void SetValue_SameValue_ShouldNotRaisePropertyChanged()
        {
            var observer = new AddressObserver(_entity);
            var monitor = observer.Monitor();

            observer.Street = _entity.Street;

            monitor.Should().NotRaisePropertyChangeFor(x => x.Street);
        }

        [Test]
        public void SetValue_NewValue_IsChangedShouldBeTrue()
        {
            var newValue = _fixture.Create<string>();
            var observer = new AddressObserver(_entity);

            observer.State = newValue;

            observer.IsChanged.Should().BeTrue();
        }

        [Test]
        public void SetValue_SameValue_IsChangedShouldBeFalse()
        {
            var observer = new AddressObserver(_entity);

            observer.State = _entity.State;

            observer.IsChanged.Should().BeFalse();
        }
        
        [Test]
        public void SetValue_NewValue_ShouldRaisePropertyChangedForIsChanged()
        {
            var newValue = _fixture.Create<string>();
            var observer = new AddressObserver(_entity);
            var monitor = observer.Monitor();

            observer.State = newValue;

            monitor.Should().RaisePropertyChangeFor(m => m.IsChanged);
        }

        [Test]
        public void SetValue_SameValue_ShouldRaisePropertyChangedForIsChanged()
        {
            var observer = new AddressObserver(_entity);
            var monitor = observer.Monitor();
            
            observer.State = _entity.State;

            monitor.Should().NotRaisePropertyChangeFor(m => m.IsChanged);
        }

        [Test]
        public void GetOriginalValue_NonMemberExpression_ShouldThrowArgumentException()
        {
            var observer = new AddressObserver(_entity);

            FluentActions.Invoking(() => observer.GetOriginalValue(m => m.ToString())).Should()
                .Throw<ArgumentException>();
        }
        
        [Test]
        public void GetOriginalValue_NewValue_ShouldBeExpected()
        {
            var newValue = _fixture.Create<string>();
            var original = _entity.State;
            var observer = new AddressObserver(_entity);

            observer.State = newValue;

            var originalValue = observer.GetOriginalValue(m => m.State);
            originalValue.Should().Be(original);
        }

        [Test]
        public void GetOriginalValue_SameValue_ShouldBeExpected()
        {
            var observer = new AddressObserver(_entity);

            observer.State = _entity.State;

            var originalValue = observer.GetOriginalValue(m => m.State);
            originalValue.Should().Be(_entity.State);
        }
        
        [Test]
        public void GetIsChanged_NonMemberExpression_ShouldThrowArgumentException()
        {
            var observer = new AddressObserver(_entity);

            FluentActions.Invoking(() => observer.GetIsChanged(m => m.ToString())).Should()
                .Throw<ArgumentException>();
        }

        [Test]
        public void GetIsChange_NewValue_ShouldBeTrue()
        {
            var newValue = _fixture.Create<int>();
            var observer = new AddressObserver(_entity);

            observer.Zip = newValue;

            var isChanged = observer.GetIsChanged(m => m.Zip);

            isChanged.Should().BeTrue();
        }

        [Test]
        public void GetIsChanged_SameValue_ShouldBeFalse()
        {
            var observer = new AddressObserver(_entity);

            observer.Zip = _entity.Zip;

            var isChanged = observer.GetIsChanged(m => m.Zip);

            isChanged.Should().BeFalse();
        }

        [Test]
        public void SetValue_ResetValue_IsChangedShouldBeFalse()
        {
            var newValue = _fixture.Create<string>();
            var original = _entity.City;
            var observer = new AddressObserver(_entity);

            observer.City = newValue;

            observer.IsChanged.Should().BeTrue();

            observer.City = original;

            observer.IsChanged.Should().BeFalse();
        }
    }
}
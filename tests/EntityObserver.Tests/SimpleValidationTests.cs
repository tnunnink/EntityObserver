﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using EntityObserver.Tests.TestEntities;
using FluentAssertions;
using EntityObserver.Tests.TestObservers;
using NUnit.Framework;

namespace EntityObserver.Tests
{
    [TestFixture]
    public class SimpleValidationTests
    {
        private Address _valid;
        private Fixture _fixture;
        private Address _invalid;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            
            _valid = _fixture.Build<Address>()
                .With(a => a.State, "MO")
                .With(a => a.Zip, 123456)
                .Create();
            
            _invalid = _fixture.Build<Address>().With(a => a.City, string.Empty).Create();
        }

        [Test]
        public void HasErrors_InvalidStateButUnvalidated_ShouldBeFalse()
        {
            var observer = new AddressObserver(_invalid);

            observer.HasErrors.Should().BeFalse();
        }

        [Test]
        public void HasErrors_ValidState_ShouldBeFalse()
        {
            var observer = new AddressObserver(_valid);
            
            observer.HasErrors.Should().BeFalse();
        }

        [Test]
        public void HasErrors_InvalidStateAndValidated_ShouldBeTrue()
        {
            var observer = new AddressObserver(_invalid);
            observer.Validate();
            
            observer.City = string.Empty;

            observer.HasErrors.Should().BeTrue();
        }
        
        [Test]
        public void HasErrors_ResolveErrors_ShouldBeFalse()
        {
            var observer = new AddressObserver(_invalid);

            observer.City = _valid.City;
            observer.State = _valid.State;
            observer.Zip = _valid.Zip;
            
            observer.HasErrors.Should().BeFalse();
        }

        [Test]
        public void ErrorsChanged_HasErrors_ShouldBeRaised()
        {
            var observer = new AddressObserver(_valid);
            var monitor = observer.Monitor();
            
            observer.City = string.Empty;

            monitor.Should().Raise(nameof(observer.ErrorsChanged));
        }
        
        [Test]
        public void ErrorsChanged_ClearError_ShouldBeRaised()
        {
            var observer = new AddressObserver(_invalid);
            observer.Validate();
            var monitor = observer.Monitor();

            observer.City = _fixture.Create<string>();

            monitor.Should().Raise(nameof(observer.ErrorsChanged));
        }

        [Test]
        public void Validate_InvalidEntity_HasErrorsShouldBeTrue()
        {
            var observer = new AddressObserver(_invalid);

            observer.Validate();

            observer.HasErrors.Should().BeTrue();
        }
        
        [Test]
        public void Validate_ValidEntity_HasErrorsShouldBeFalse()
        {
            var observer = new AddressObserver(_valid);

            observer.Validate();

            observer.HasErrors.Should().BeFalse();
        }
        
        [Test]
        public void Validate_MultipleValidations_ShouldHaveExpectedErrorCounts()
        {
            var observer = new AddressObserver(_invalid);

            observer.Validate();
            observer.Validate();
            observer.Validate();

            observer.HasErrors.Should().BeTrue();

            observer.GetErrors(m => m.City).Should().HaveCount(1);
            observer.GetErrors(m => m.State).Should().HaveCount(1);
            observer.GetErrors(m => m.Zip).Should().HaveCount(1);
        }
        
        [Test]
        public void ValidateProperty_EmptyString_ShouldThrowArgumentException()
        {
            var observer = new AddressObserver(_invalid);

            FluentActions.Invoking(() => observer.Validate("", _valid.City)).Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void ValidateProperty_NullValue_PropertyShouldHaveErrors()
        {
            var observer = new AddressObserver(_valid);
            observer.Validate();

            observer.Validate("City", null);

            var errors = (observer.GetErrors("City") as IEnumerable<string>)?.ToList();

            errors.Should().NotBeEmpty();
            errors.Should().Contain("City is required");
        }
        
        [Test]
        public void ValidateProperty_InvalidProperty_HasErrorsShouldBeTrue()
        {
            var observer = new AddressObserver(_invalid);

            observer.Validate(m => m.City);

            observer.HasErrors.Should().BeTrue();
        }
        
        [Test]
        public void ValidateProperty_ValidEntity_HasErrorsShouldBeFalse()
        {
            var observer = new AddressObserver(_valid);

            observer.Validate(m => m.City);

            observer.HasErrors.Should().BeFalse();
        }
        
        [Test]
        public void ValidateRequired_InvalidProperty_HasErrorsShouldBeTrue()
        {
            var observer = new AddressObserver(_invalid);

            observer.Validate(ValidationOption.Required);

            observer.HasErrors.Should().BeTrue();
        }
        
        [Test]
        public void ValidateRequired_ValidEntity_HasErrorsShouldBeFalse()
        {
            var observer = new AddressObserver(_valid);

            observer.Validate(ValidationOption.Required);

            observer.HasErrors.Should().BeFalse();
        }
        
        [Test]
        public void ValidateRequired_InvalidProperty_OnlyRequiredPropertiesShouldHaveErrors()
        {
            var observer = new AddressObserver(_invalid);

            observer.Validate(ValidationOption.Required);

            var errors = observer.GetErrors();

            errors.Should().HaveCount(1);
        }
        
        [Test]
        public void ValidateRequired_MultipleValidations_ShouldHaveExpectedErrorCounts()
        {
            var observer = new AddressObserver(_invalid);

            observer.Validate(ValidationOption.Required);
            observer.Validate(ValidationOption.Required);
            observer.Validate(ValidationOption.Required);

            observer.HasErrors.Should().BeTrue();

            observer.GetErrors(m => m.City).Should().HaveCount(1);
            observer.GetErrors(m => m.State).Should().HaveCount(0);
            observer.GetErrors(m => m.Zip).Should().HaveCount(0);
        }

        [Test]
        public void GetErrors_InvalidEntity_ShouldReturnExpected()
        {
            var observer = new AddressObserver(_invalid);
            observer.Validate();

            var errors = (observer.GetErrors("City") as IEnumerable<string>)?.ToList();

            errors.Should().NotBeNull();
            errors.Should().Contain("City is required");
        }
        
        [Test]
        public void GetErrors_ValidEntity_ShouldBeEmpty()
        {
            var observer = new AddressObserver(_valid);
            observer.Validate();

            var errors = (observer.GetErrors("City") as IEnumerable<string>)?.ToList();

            errors.Should().BeEmpty();
        }

        [Test]
        public void GetAllErrors_InvalidEntity_ShouldHaveExpectedCount()
        {
            var observer = new AddressObserver(_invalid);
            observer.Validate();

            var errors = observer.GetErrors();

            errors.Should().HaveCount(3);
        }
        
        [Test]
        public void GetAllErrors_ValidEntity_ShouldBeEmpty()
        {
            var observer = new AddressObserver(_valid);
            observer.Validate();

            var errors = observer.GetErrors();

            errors.Should().BeEmpty();
        }
    }
}
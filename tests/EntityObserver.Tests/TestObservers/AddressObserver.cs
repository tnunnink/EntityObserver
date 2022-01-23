using System;
using System.ComponentModel.DataAnnotations;
using EntityObserver;
using EntityObserver.Tests.TestModels;

namespace EntityObserver.Tests.TestObservers
{
    public class AddressObserver : Observer<Address>
    {
        public AddressObserver(Address model) : base(model)
        {
        }

        public Guid Id
        {
            get => GetValue<Guid>();
            set => SetValue(value);
        }

        [Required(ErrorMessage = "Street is required")]
        public string Street
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [Required(ErrorMessage = "City is required")]
        public string City
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [Required(ErrorMessage = "State is required")]
        public string State
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [Required(ErrorMessage = "Zip is required")]
        public int Zip
        {
            get => GetValue<int>();
            set => SetValue(value);
        }
    }
}
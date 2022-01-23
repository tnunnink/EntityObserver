using System.Collections.ObjectModel;
using System.Linq;
using EntityObserver.Tests.TestEntities;

namespace EntityObserver.Tests.TestObservers
{
    public class RegistrationObserver : Observer<Person>
    {
        public RegistrationObserver(Person entity) : base(entity)
        {
            Address = new AddressObserver(entity.Address);
            RegisterObserver(Address);

            Emails = new ObservableCollection<string>(entity.Emails);

            Cars = new ObserverCollection<CarObserver>(entity.Cars.Select(c => new CarObserver(c)));
            RegisterObserver(Cars);
        }
        
        public int Id
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        public string FirstName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string LastName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public int Age
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        public float Weight
        {
            get => GetValue<float>();
            set => SetValue(value);
        }

        public float Height
        {
            get => GetValue<float>();
            set => SetValue(value);
        }

        public bool IsEmployed
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
        
        public AddressObserver Address { get; }

        public ObservableCollection<string> Emails { get; }

        public ObserverCollection<CarObserver> Cars { get; }
    }
}
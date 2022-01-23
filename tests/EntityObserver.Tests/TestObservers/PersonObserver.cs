using System.Collections.ObjectModel;
using EntityObserver;
using EntityObserver.Tests.TestModels;

namespace EntityObserver.Tests.TestObservers
{
    public class PersonObserver : Observer<Person>
    {
        public PersonObserver(Person model) : base(model)
        {
            InitializeObservers();
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
        
        public AddressObserver Address { get; set; }

        public ObservableCollection<string> Emails { get; set; }

        public ObserverCollection<CarObserver> Cars { get; set; }
    }
}
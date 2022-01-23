using EntityObserver;
using EntityObserver.Tests.TestEntities;

namespace EntityObserver.Tests.TestObservers
{
    public class CarObserver : Observer<Car>
    {
        public CarObserver(Car model) : base(model)
        {
        }

        public string Vin
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Make
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        //todo this is interesting come back to this...
        /*public string Model
        {
            get => GetValue<string>();
            set => SetValue(value);
        }*/

        public int Mileage
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        public int Cost
        {
            get => GetValue<int>();
            set => SetValue(value);
        }
    }
}
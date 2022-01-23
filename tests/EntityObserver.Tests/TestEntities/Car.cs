namespace EntityObserver.Tests.TestEntities
{
    public class Car
    {
        public Car()
        {
        }

        public Car(string vin, string make, string model, int mileage, int cost)
        {
            Vin = vin;
            Make = make;
            Model = model;
            Mileage = mileage;
            Cost = cost;
        }
        
        public string Vin { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Mileage { get; set; }
        public int Cost { get; set; }
    }
}
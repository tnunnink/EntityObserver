using System;
using System.Collections.Generic;

namespace EntityObserver.Tests.TestEntities
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public float Weight { get; set; }
        public float Height { get; set; }
        public bool IsEmployed { get; set; }
        public Address Address { get; set; }
        public IEnumerable<string> Emails { get; set; }
        public List<Car> Cars { get; set; }

        public void Rename(string first, string last)
        {
            if (string.IsNullOrEmpty(first))
                throw new ArgumentNullException(nameof(first));
            
            if (string.IsNullOrEmpty(first))
                throw new ArgumentNullException(nameof(last));
            
            FirstName = first;
            LastName = last;
        }
    }
}
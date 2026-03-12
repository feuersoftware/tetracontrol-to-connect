using Bogus;
using FeuerSoftware.TetraControl2Connect.Models.Connect;
using System.Collections.Generic;

namespace FeuerSoftware.TetraControl2Connect.Test.Helper
{
    internal static class TestHelper
    {
        private static readonly Faker Faker = new("de");

        internal static List<VehicleModel> GenerateVehicles(int count)
        {
            List<VehicleModel> vehicles = [];
            for (int i = 0; i < count; i++)
            {
                vehicles.Add(GenerateVehicle());
            }

            return vehicles;
        }

        internal static VehicleModel GenerateVehicle()
        {
            return new VehicleModel()
            {
                CallSign = Faker.Random.AlphaNumeric(20),
                Crew = Faker.Random.Int(1, 9),
                Description = Faker.Random.AlphaNumeric(30),
                LocationIdentificationNumber = Faker.Random.Int(0, 99),
                Id = Faker.Random.Int(0, 999999),
                OrganizationCallSign = Faker.Random.AlphaNumeric(20),
                Phone = Faker.Phone.PhoneNumber(),
                PlaceName = Faker.Random.AlphaNumeric(20),
                RadioId = Faker.Random.Guid().ToString(),
                Subdivision = Faker.Random.Int(0, 20),
                VehicleIdentifier = Faker.Random.AlphaNumeric(20),
            };
        }

        internal static List<UserModel> GenerateUsers(int count)
        {
            List<UserModel> users = [];
            for (int i = 0; i < count; i++)
            {
                users.Add(GenerateUser());
            }

            return users;
        }

        internal static UserModel GenerateUser()
        {
            return new UserModel()
            {
                FirstName = Faker.Name.FirstName(),
                LastName = Faker.Name.LastName(),
                Email = Faker.Person.Email,
                Id = Faker.Random.Guid().ToString(),
                PagerIssi = Faker.Random.AlphaNumeric(7),
                UserName = Faker.Person.Email,
            };
        }
    }
}

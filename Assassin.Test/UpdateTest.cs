///-----------------------------------------------------------------
///   File:     UpdateTest.cs
///   Author:   Andre Laskawy           
///   Date:	    04.12.2017 09:56:02
///   Revision History: 
///   Name:  Andre Laskawy         Date:  04.12.2017 09:56:02      Description: Created
///-----------------------------------------------------------------

namespace Quant.Test
{
    using Assassin.Data.Repository;
    using Quant.Test.Models;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="UpdateTest" />
    /// </summary>
    public class UpdateTest
    {
        /// <summary>
        /// The Test1
        /// </summary>
        [Fact]
        public async Task Test_Update()
        {
            //QuantDataService dataService = new QuantDataService("MyAppId");

            Address address = new Address() { Street = "Test steet 1aa" };

            Person max = new Person();
            max.Age = 29;
            max.Name = "Max a";

            Person annika = new Person();
            annika.Age = 22;
            annika.Name = "Annika a";

            max.AddRelation(address, nameof(max.Addresses), nameof(address.Persons));
            annika.AddRelation(address, nameof(annika.Addresses), nameof(address.Persons));

            // Insert
            await DataHandler.Insert(address);

            // Update
            annika.Name = "Annika";
            max.Name = "Max";
            max.Addresses.FirstOrDefault().Street = "Test steet 1a";
            await DataHandler.Update(max);
            await DataHandler.Update(annika);

            // Get
            var result_max = DataHandler.GetById<Person>(max.Id, true);
            var result_annika = DataHandler.GetById<Person>(annika.Id, true);
            var street = DataHandler.GetById<Address>(address.Id, true);

            Assert.NotNull(result_max);
            Assert.NotNull(result_annika);
            Assert.NotNull(street);
            Assert.Equal("Max", result_max.Name);
            Assert.Equal(29, result_max.Age);
            Assert.Equal("Annika", result_annika.Name);
            Assert.Equal(22, result_annika.Age);
            Assert.Equal("Test steet 1a", result_max.Addresses.FirstOrDefault().Street);
            Assert.Equal("Test steet 1a", result_annika.Addresses.FirstOrDefault().Street);
            Assert.Equal("Test steet 1a", street.Street);
            Assert.Equal(2, street.Persons.Count);
        }
    }
}

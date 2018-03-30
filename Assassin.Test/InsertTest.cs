///-----------------------------------------------------------------
///   File:     InsertTest.cs
///   Author:   Andre Laskawy           
///   Date:	    04.12.2017 09:56:02
///   Revision History: 
///   Name:  Andre Laskawy         Date:  04.12.2017 09:56:02      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Test
{
    using Assassin.Data.Repository;
    using Quant.Test.Models;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="InsertTest" />
    /// </summary>
    public class InsertTest
    {
        /// <summary>
        /// Tests the basic i insert.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Test_Insert_Basic()
        {
            DataHandler.Initialize();
            DataHandler dataService = new DataHandler();

            Person max = new Person();
            max.Age = 29;
            max.Name = "Max";

            // Insert
            await DataHandler.Insert(max);

            // Get       
            var result = DataHandler.GetById<Person>(max.Id, true);

            Assert.NotNull(result);
            Assert.Equal("Max", result.Name);
            Assert.Equal(29, result.Age);
            Assert.Empty(result.Addresses);
        }

        /// <summary>
        /// Tests the insert unidrectional relation.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Test_Insert_Unidirectional_Relation()
        {
            Address address = new Address() { Street = "Test steet 1a" };

            Person max = new Person();
            max.Age = 29;
            max.Name = "Max";

            max.AddRelation(address, nameof(max.Addresses));

            // QuantDataService dataService = new QuantDataService("MyAppId");          

            // Insert
            await DataHandler.Insert(max);

            // Get
            var result = DataHandler.GetById<Person>(max.Id, true);
            var street = DataHandler.GetById<Address>(address.Id, true);

            Assert.NotNull(result);
            Assert.NotNull(street);
            Assert.Equal("Max", result.Name);
            Assert.Equal(29, result.Age);
            Assert.Equal("Test steet 1a", result.Addresses.FirstOrDefault().Street);
            Assert.Equal("Test steet 1a", street.Street);
            Assert.Empty(street.Persons);
        }

        /// <summary>
        /// Tests the insert bidirectional relation.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Test_Insert_Bidirectional_Relation()
        {
            //QuantDataService dataService = new QuantDataService("MyAppId");
            Address address = new Address() { Street = "Test steet 1a" };

            Person max = new Person();
            max.Age = 29;
            max.Name = "Max";

            max.AddRelation(address, nameof(max.Addresses), nameof(address.Persons));

            // Insert
            await DataHandler.Insert(max);

            // Get
            var result = DataHandler.GetById<Person>(max.Id, true);
            var street = DataHandler.GetById<Address>(address.Id, true);

            Assert.NotNull(result);
            Assert.NotNull(street);
            Assert.Equal("Max", result.Name);
            Assert.Equal(29, result.Age);
            Assert.Equal("Test steet 1a", result.Addresses.FirstOrDefault().Street);
            Assert.Equal("Test steet 1a", street.Street);
            Assert.Equal("Max", street.Persons.FirstOrDefault().Name);
        }

        /// <summary>
        /// The Test1
        /// </summary>
        [Fact]
        public async Task Test_Insert_Multiple_Bidrectional_Relation()
        {
            //QuantDataService dataService = new QuantDataService("MyAppId");

            Address address = new Address() { Street = "Test steet 1a" };

            Person max = new Person();
            max.Age = 29;
            max.Name = "Max";

            Person annika = new Person();
            annika.Age = 22;
            annika.Name = "Annika";

            max.AddRelation(address, nameof(max.Addresses), nameof(address.Persons));
            annika.AddRelation(address, nameof(annika.Addresses), nameof(address.Persons));

            // Insert
            await DataHandler.Insert(address);

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

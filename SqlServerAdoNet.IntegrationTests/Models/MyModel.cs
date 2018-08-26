using System.ComponentModel.DataAnnotations;

namespace StatKings.SqlServerAdoNet.IntegrationTests
{
    public class MyModel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}

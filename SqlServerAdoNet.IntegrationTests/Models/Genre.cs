using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatKings.SqlServerAdoNet.IntegrationTests
{
    public class Genre
    {
        [Key]
        public int GenreId { get; set; }

        public string Name { get; set; }
    }
}

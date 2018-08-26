using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatKings.SqlServerAdoNet.IntegrationTests
{
    public class Artist
    {
        [Key]
        public int ArtistId { get; set; }

        public string Name { get; set; }
    }
}

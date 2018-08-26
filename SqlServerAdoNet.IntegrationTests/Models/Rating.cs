using System;
using System.ComponentModel.DataAnnotations;

namespace StatKings.SqlServerAdoNet.IntegrationTests
{
    public class Rating
    {
        [Key]
        public int AlbumId { get; set; }

        [Key]
        public DateTime RatingDate { get; set; }

        public decimal Stars { get; set; }
    }
}

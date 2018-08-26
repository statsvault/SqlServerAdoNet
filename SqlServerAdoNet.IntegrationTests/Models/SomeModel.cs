using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatKings.SqlServerAdoNet.IntegrationTests
{
    [Table("Album")]
    public class SomeModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("AlbumId")]
        public int MemberA { get; set; }

        [Column("GenreId")]
        public int MemberB { get; set; }

        [Column("ArtistId")]
        public int MemberC { get; set; }
        
        [Column("Title")]
        public string MemberD { get; set; }

        [Column("Price")]
        public decimal? MemberE { get; set; }
    }
}

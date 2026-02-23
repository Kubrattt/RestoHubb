
namespace RestoHub.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Masalar : BaseEntity
    {
        private const string M = "Bu Alan Boþ Geçilemez.";


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Masalar()
        {
            this.Siparisler = new HashSet<Siparisler>();
        }
    
        public int MasaId { get; set; }
        [Required(ErrorMessage = M)]
        public int RestoranId { get; set; }
        [Required(ErrorMessage = M)]
        public string MasaKodu { get; set; }
        [Required(ErrorMessage = M)]
        public string MasaAdi { get; set; }
        [Required(ErrorMessage = M)]
        public string Konum { get; set; }
        [Required(ErrorMessage = M)]
        public bool Dolu { get; set; }
    
        public virtual Restoranlar Restoranlar { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Siparisler> Siparisler { get; set; }
    }
}

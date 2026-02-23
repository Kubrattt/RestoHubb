
namespace RestoHub.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;


    public partial class Kategoriler : BaseEntity
    {
        private const string M = "Bu Alan Boþ Geçilemez.";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Kategoriler()
        {
            this.Urunler = new HashSet<Urunler>();
        }
    
        public int KategoriId { get; set; }
        [Required(ErrorMessage = M)]
        public int RestoranId { get; set; }
        [Required(ErrorMessage = M)]
        public string KategoriAdi { get; set; }
        [Required(ErrorMessage = M)]
        public string Aciklama { get; set; }
    
        public virtual Restoranlar Restoranlar { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Urunler> Urunler { get; set; }
    }
}

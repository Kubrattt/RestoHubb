
namespace RestoHub.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Yetkiler:BaseEntity
    {
        private const string M = "Bu Alan Boþ Geçilemez.";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Yetkiler()
        {
            this.Kullanicilar = new HashSet<Kullanicilar>();
        }
    
        public int YetkiId { get; set; }
        [Required(ErrorMessage = M)]
        public string YetkiAdi { get; set; }
        public string Aciklama { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Kullanicilar> Kullanicilar { get; set; }
    }
}

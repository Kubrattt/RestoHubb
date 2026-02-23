namespace RestoHub.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class BirimTipleri: BaseEntity
    {
        private const string M = "Bu Alan Boþ Geçilemez.";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BirimTipleri()
        {
            this.Urunler = new HashSet<Urunler>();
        }
    
        public int BirimId { get; set; }
        [Required(ErrorMessage = M)]
        public string BirimAdi { get; set; }
        [Required(ErrorMessage = M)]
        public string BirimSembol { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Urunler> Urunler { get; set; }
    }
}


namespace RestoHub.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class SiparisDurumlari:BaseEntity
    {
        private const string M = "Bu Alan Boþ Geçilemez.";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SiparisDurumlari()
        {
            this.SiparisDetaylari = new HashSet<SiparisDetaylari>();
            this.Siparisler = new HashSet<Siparisler>();
        }
    
        public int DurumId { get; set; }
        [Required(ErrorMessage = M)]
        public string DurumAdi { get; set; }
        [Required(ErrorMessage = M)]
        public string Renk { get; set; }
        [Required(ErrorMessage = M)]
        public int GorunumSirasi { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SiparisDetaylari> SiparisDetaylari { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Siparisler> Siparisler { get; set; }
    }
}


namespace RestoHub.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Urunler:BaseEntity
    {
        private const string M = "Bu Alan Boþ Geçilemez.";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Urunler()
        {
            this.SiparisDetaylari = new HashSet<SiparisDetaylari>();
        }
    
        public int UrunId { get; set; }
        [Required(ErrorMessage = M)]
        public int RestoranId { get; set; }
        [Required(ErrorMessage = M)]
        public int KategoriId { get; set; }
        [Required(ErrorMessage = M)]
        public int BirimId { get; set; }
        public string UrunKodu { get; set; }
        [Required(ErrorMessage = M)]
        public string UrunAdi { get; set; }
        public string Aciklama { get; set; }
        public string KisaAciklama { get; set; }
        [Required(ErrorMessage = M)]
        public decimal Fiyat { get; set; }
        public Nullable<decimal> MaliyetFiyati { get; set; }
        public Nullable<decimal> IndirimliFiyat { get; set; }
        public Nullable<int> HazirlamaSuresi { get; set; }
        [Required(ErrorMessage = M)]
        public bool StokDurumu { get; set; }
    
        public virtual BirimTipleri BirimTipleri { get; set; }
        public virtual Kategoriler Kategoriler { get; set; }
        public virtual Restoranlar Restoranlar { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SiparisDetaylari> SiparisDetaylari { get; set; }
    }
}

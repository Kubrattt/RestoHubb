
namespace RestoHub.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Siparisler:BaseEntity
    {

        private const string M = "Bu Alan Boþ Geçilemez.";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Siparisler()
        {
            this.SiparisDetaylari = new HashSet<SiparisDetaylari>();
        }
    
        public int SiparisId { get; set; }
        [Required(ErrorMessage = M)]
        public int RestoranId { get; set; }
        [Required(ErrorMessage = M)]
        public int MasaId { get; set; }
        [Required(ErrorMessage = M)]
        public int DurumId { get; set; }
        [Required(ErrorMessage = M)]
        public int OlusturanId { get; set; }
        public Nullable<int> ServisYapanId { get; set; }
        public string MisafirAdi { get; set; }
        public Nullable<int> KisiSayisi { get; set; }
        [Required(ErrorMessage = M)]
        public decimal AraToplam { get; set; }
        public Nullable<decimal> IndirimTutari { get; set; }
        public Nullable<decimal> KDVTutari { get; set; }
        [Required(ErrorMessage = M)]
        public decimal ToplamTutar { get; set; }
        [Required(ErrorMessage = M)]
        public string OdemeDurumu { get; set; }
        public string OdemeYontemi { get; set; }
        public string Notlar { get; set; }
        public string IptalNedeni { get; set; }
        public System.DateTime SiparisTarihi { get; set; }
        public Nullable<System.DateTime> TamamlanmaTarihi { get; set; }
    
        public virtual Kullanicilar Kullanicilar { get; set; }
        public virtual Kullanicilar Kullanicilar1 { get; set; }
        public virtual Masalar Masalar { get; set; }
        public virtual Restoranlar Restoranlar { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SiparisDetaylari> SiparisDetaylari { get; set; }
        public virtual SiparisDurumlari SiparisDurumlari { get; set; }
    }
}

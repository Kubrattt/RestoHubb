namespace RestoHub.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Kullanicilar: BaseEntity
    {
        private const string M = "Bu Alan Boþ Geçilemez.";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Kullanicilar()
        {
            this.SiparisDetaylari = new HashSet<SiparisDetaylari>();
            this.Siparisler = new HashSet<Siparisler>();
            this.Siparisler1 = new HashSet<Siparisler>();
        }
    
        public int KullaniciId { get; set; }
        [Required(ErrorMessage = M)]
        public int RestoranId { get; set; }
        [Required(ErrorMessage = M)]
        public int YetkiId { get; set; }
        [Required(ErrorMessage = M)]
        public string KullaniciAdi { get; set; }
        [Required(ErrorMessage = M)]
        public string Ad { get; set; }
        [Required(ErrorMessage = M)]
        public string Soyad { get; set; }
        [Required(ErrorMessage = M)]
        [EmailAddress(ErrorMessage = "Geçerli Bir Email Giriniz.")]

        public string Email { get; set; }
        public string Telefon { get; set; }
        [Required(ErrorMessage = M)]
        public string Sifre { get; set; }
        [Required(ErrorMessage = M)]
        public System.DateTime BaslamaTarihi { get; set; }
        public Nullable<System.DateTime> SonGirisTarihi { get; set; }
    
        public virtual Restoranlar Restoranlar { get; set; }
        public virtual Yetkiler Yetkiler { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SiparisDetaylari> SiparisDetaylari { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Siparisler> Siparisler { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Siparisler> Siparisler1 { get; set; }
    }
}


namespace RestoHub.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class SiparisDetaylari : BaseEntity
    {
        private const string M = "Bu Alan Boþ Geçilemez.";

        public int SiparisDetayId { get; set; }
        [Required(ErrorMessage = M)]
        public int SiparisId { get; set; }
        [Required(ErrorMessage = M)]
        public int UrunId { get; set; }
        [Required(ErrorMessage = M)]
        public int DurumId { get; set; }
        [Required(ErrorMessage = M)]
        public decimal Miktar { get; set; }
        [Required(ErrorMessage = M)]
        public decimal BirimFiyat { get; set; }
        public Nullable<decimal> IndirimTutari { get; set; }
        [Required(ErrorMessage = M)]
        public decimal ToplamFiyat { get; set; }
        public string Notlar { get; set; }
        public Nullable<System.DateTime> HazirlanmaTarihi { get; set; }
        public Nullable<int> HazirlayanId { get; set; }
    
        public virtual Kullanicilar Kullanicilar { get; set; }
        public virtual SiparisDurumlari SiparisDurumlari { get; set; }
        public virtual Siparisler Siparisler { get; set; }
        public virtual Urunler Urunler { get; set; }
    }
}

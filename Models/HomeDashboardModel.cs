using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestoHub.Models
{
    public class HomeDashboardViewModel
    {
        // Ortak
        public string AktifKullaniciAdi { get; set; }
        public int AktifYetkiId { get; set; }

        // SuperAdmin için
        public int ToplamRestoran { get; set; }
        public int ToplamKullanici { get; set; }
        public List<Restoranlar> Restoranlar { get; set; }

        // Diğerleri için
        public Restoranlar AktifRestoran { get; set; }
        public int RestoranKullanici { get; set; }
        public string AktifRestoranAdi { get; set; }
    }
}
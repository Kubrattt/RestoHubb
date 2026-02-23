using RestoHub.Models;
using System.Linq;
using System.Web.Mvc;

namespace RestoHub.Controllers
{
    public class BaseController : Controller
    {
        public RestoHubDBEntities db = new RestoHubDBEntities();

        // ─── Session Anahtarları ─────────────────────────────────────────
        protected const string SESSION_KULLANICI_ID = "KullaniciId";
        protected const string SESSION_KULLANICI_ADI = "KullaniciAdi";
        protected const string SESSION_YETKI_ID = "YetkiId";
        protected const string SESSION_RESTORAN_ID = "RestoranId";
        protected const string SESSION_AD_SOYAD = "AdSoyad";
        protected const string SESSION_RESTORAN_ADI = "RestoranAdi";

        // ─── Kolay Erişim Property'leri ──────────────────────────────────

        protected string AktifRestoranAdi
    => Session[SESSION_RESTORAN_ADI]?.ToString() ?? "";

        protected int AktifKullaniciId
            => Session[SESSION_KULLANICI_ID] != null
               ? (int)Session[SESSION_KULLANICI_ID] : 0;

        protected int AktifYetkiId
            => Session[SESSION_YETKI_ID] != null
               ? (int)Session[SESSION_YETKI_ID] : 0;

        protected int AktifRestoranId
            => Session[SESSION_RESTORAN_ID] != null
               ? (int)Session[SESSION_RESTORAN_ID] : 0;

        protected string AktifKullaniciAdi
            => Session[SESSION_KULLANICI_ADI]?.ToString() ?? "";

        protected string AktifAdSoyad
            => Session[SESSION_AD_SOYAD]?.ToString() ?? "";

        protected bool SuperAdminMi => AktifYetkiId == 1;
        protected bool RestoranSahibiMi => AktifYetkiId == 2;
        protected bool RestoranMudurMu => AktifYetkiId == 3;
        protected bool AltYetkiMi => AktifYetkiId >= 4;

        // ─── Oturum Açık mı Kontrolü ─────────────────────────────────────
        protected bool OturumAcikMi => AktifKullaniciId > 0;

        // ─── Session'a Yaz (Login sırasında çağrılır) ────────────────────
        protected void OturumBaslat(Kullanicilar kullanici)
        {
            Session[SESSION_KULLANICI_ID]  = kullanici.KullaniciId;
            Session[SESSION_KULLANICI_ADI] = kullanici.KullaniciAdi;
            Session[SESSION_YETKI_ID]      = kullanici.YetkiId;
            Session[SESSION_RESTORAN_ID]   = kullanici.RestoranId;
            Session[SESSION_AD_SOYAD]      = $"{kullanici.Ad} {kullanici.Soyad}";

            var restoran = db.Restoranlar.FirstOrDefault(r => r.RestoranId == kullanici.RestoranId);

            Session[SESSION_RESTORAN_ADI] = restoran != null ? restoran.RestoranAdi : "RestoHub";
        }

        // ─── Session'ı Temizle (Logout sırasında çağrılır) ───────────────
        protected void OturumKapat()
        {
            Session.Clear();
            Session.Abandon();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using RestoHub.Models;

namespace RestoHub.Controllers
{
    public class SiparislersController : BaseController
    {
        // GET: Siparislers/Index
        public async Task<ActionResult> Index(string filtre = "aktif", int? restoranId = null)
        {
            var siparisler = db.Siparisler
                .Include(s => s.Kullanicilar)
                .Include(s => s.Kullanicilar1)
                .Include(s => s.Masalar)
                .Include(s => s.Restoranlar)
                .Include(s => s.SiparisDurumlari)
                .AsQueryable();

            if (SuperAdminMi)
            {
                ViewBag.Restoranlar = new SelectList(db.Restoranlar.ToList(), "RestoranId", "RestoranAdi", restoranId);
                if (restoranId.HasValue)
                    siparisler = siparisler.Where(s => s.RestoranId == restoranId.Value);
            }
            else
            {
                siparisler = siparisler.Where(s => s.RestoranId == AktifRestoranId);
            }

            switch (filtre)
            {
                case "aktif": siparisler = siparisler.Where(s => s.OdemeDurumu == "Ödenmedi"); break;
                case "tamamlandi": siparisler = siparisler.Where(s => s.OdemeDurumu == "Ödendi"); break;
                case "iptal": siparisler = siparisler.Where(s => s.SiparisDurumlari.DurumAdi == "İptal Edildi"); break;
            }

            ViewBag.Filtre = filtre;
            ViewBag.SeciliRestoranId = restoranId;
            return View(await siparisler.OrderByDescending(s => s.SiparisTarihi).ToListAsync());
        }

        // GET: Siparislers/Create
        public ActionResult Create(int? restoranId = null)
        {
            int rId = SuperAdminMi ? (restoranId ?? 0) : AktifRestoranId;

            ViewBag.BekliyorId       = db.SiparisDurumlari.FirstOrDefault(d => d.DurumAdi == "Bekliyor")?.DurumId ?? 1;
            ViewBag.SeciliRestoranId = rId;

            DoldurDropDown(rId);
            return View(new Siparisler { RestoranId = rId });
        }

        // POST: Siparislers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "RestoranId,MasaId,DurumId,ServisYapanId,MisafirAdi,KisiSayisi,Notlar")]
            Siparisler siparisler)
        {
            if (!SuperAdminMi)
                siparisler.RestoranId = AktifRestoranId;

            // Otomatik alanlar
            siparisler.OlusturanId   = AktifKullaniciId;
            siparisler.SiparisTarihi = DateTime.Now;
            siparisler.OdemeDurumu   = "Ödenmedi";
            siparisler.AraToplam     = 0;
            siparisler.ToplamTutar   = 0;
            siparisler.KDVTutari     = 0;
            siparisler.IndirimTutari = 0;
            if (siparisler.KisiSayisi <= 0) siparisler.KisiSayisi = 1;

            if (siparisler.DurumId <= 0)
                siparisler.DurumId = db.SiparisDurumlari
                    .FirstOrDefault(d => d.DurumAdi == "Bekliyor")?.DurumId ?? 1;

            // ModelState temizle
            foreach (var k in new[] { "OlusturanId","SiparisTarihi","OdemeDurumu",
                                       "AraToplam","ToplamTutar","KDVTutari","IndirimTutari" })
                ModelState.Remove(k);

            // Zorunlu alan kontrolleri
            if (siparisler.RestoranId <= 0)
                ModelState.AddModelError("RestoranId", "Lütfen bir restoran seçin.");
            if (siparisler.MasaId <= 0)
                ModelState.AddModelError("MasaId", "Lütfen bir masa seçin.");

            if (!ModelState.IsValid)
            {
                ViewBag.BekliyorId       = db.SiparisDurumlari.FirstOrDefault(d => d.DurumAdi == "Bekliyor")?.DurumId ?? 1;
                ViewBag.SeciliRestoranId = siparisler.RestoranId;
                DoldurDropDown(siparisler.RestoranId);
                return View(siparisler);
            }

            // -------------------------------------------------------
            // SEPET MANTIĞI: Aynı masada zaten açık (Ödenmedi) sipariş var mı?
            // Varsa yeni sipariş açma, doğrudan o siparişin detay sayfasına git.
            // -------------------------------------------------------
            var mevcutSiparis = await db.Siparisler
                .Where(s => s.MasaId     == siparisler.MasaId
                         && s.RestoranId == siparisler.RestoranId
                         && s.OdemeDurumu == "Ödenmedi")
                .OrderByDescending(s => s.SiparisTarihi)
                .FirstOrDefaultAsync();

            if (mevcutSiparis != null)
            {
                // Mevcut açık siparişe yönlendir
                TempData["msj"] = "Bu masada zaten açık bir sipariş var. Mevcut siparişe ürün eklendi.";
                return RedirectToAction("Create", "SiparisDetaylaris",
                    new { sipId = mevcutSiparis.SiparisId });
            }

            // Açık sipariş yoksa yeni oluştur
            try
            {
                db.Siparisler.Add(siparisler);
                await db.SaveChangesAsync();
                return RedirectToAction("Create", "SiparisDetaylaris",
                    new { sipId = siparisler.SiparisId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kayıt hatası: " + ex.Message);
            }

            ViewBag.BekliyorId       = db.SiparisDurumlari.FirstOrDefault(d => d.DurumAdi == "Bekliyor")?.DurumId ?? 1;
            ViewBag.SeciliRestoranId = siparisler.RestoranId;
            DoldurDropDown(siparisler.RestoranId);
            return View(siparisler);
        }

        // GET: SuperAdmin restoran seçince GET redirect
        public ActionResult RestoranSec(int restoranId)
        {
            return RedirectToAction("Create", new { restoranId });
        }

        // -------------------------------------------------------
        private void DoldurDropDown(int restoranId)
        {
            var restoranlar = SuperAdminMi
                ? db.Restoranlar.Where(r => r.Aktif == true).ToList()
                : db.Restoranlar.Where(r => r.RestoranId == AktifRestoranId && r.Aktif == true).ToList();

            ViewBag.RestoranId = new SelectList(restoranlar, "RestoranId", "RestoranAdi",
                restoranId > 0 ? (int?)restoranId : null);

            // Sadece seçili restoranın boş masaları
            var masalar = restoranId > 0
                ? db.Masalar
                    .Where(m => m.RestoranId == restoranId && m.Aktif == true && m.Dolu == false)
                    .ToList()
                : new List<Masalar>();

            // Sadece seçili restoranın aktif personeli
            var personeller = restoranId > 0
                ? db.Kullanicilar
                    .Where(k => k.RestoranId == restoranId && k.Aktif == true)
                    .ToList()
                : new List<Kullanicilar>();

            ViewBag.MasaId        = new SelectList(masalar, "MasaId", "MasaAdi", null);
            ViewBag.ServisYapanId = new SelectList(personeller, "KullaniciId", "KullaniciAdi", null);
        }
    }
}
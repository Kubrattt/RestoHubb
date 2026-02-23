using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using RestoHub.Models;

namespace RestoHub.Controllers
{
    public class SiparisDetaylarisController : BaseController
    {
        // GET: Mutfak/Hazırlık Ekranı
        public async Task<ActionResult> Index(string filtre = "bekliyor")
        {
            var detaylar = db.SiparisDetaylari
                .Include(s => s.Siparisler)
                .Include(s => s.Siparisler.Masalar)
                .Include(s => s.Urunler)
                .Include(s => s.SiparisDurumlari)
                .AsQueryable();

            if (!SuperAdminMi)
                detaylar = detaylar.Where(d => d.Siparisler.RestoranId == AktifRestoranId);

            if (filtre == "bekliyor")
                detaylar = detaylar.Where(d => d.SiparisDurumlari.DurumAdi == "Bekliyor");
            else if (filtre == "hazirlaniyor")
                detaylar = detaylar.Where(d => d.SiparisDurumlari.DurumAdi == "Hazırlanıyor");
            else if (filtre == "hazir")
                detaylar = detaylar.Where(d => d.SiparisDurumlari.DurumAdi == "Hazır");
            else if (filtre == "teslim")
                detaylar = detaylar.Where(d => d.SiparisDurumlari.DurumAdi == "Teslim Edildi");

            ViewBag.Filtre         = filtre;
            ViewBag.BekliyorId     = db.SiparisDurumlari.FirstOrDefault(d => d.DurumAdi == "Bekliyor")?.DurumId ?? 0;
            ViewBag.HazirlaniyorId = db.SiparisDurumlari.FirstOrDefault(d => d.DurumAdi == "Hazırlanıyor")?.DurumId ?? 0;
            ViewBag.HazirId        = db.SiparisDurumlari.FirstOrDefault(d => d.DurumAdi == "Hazır")?.DurumId ?? 0;
            ViewBag.TeslimId       = db.SiparisDurumlari.FirstOrDefault(d => d.DurumAdi == "Teslim Edildi")?.DurumId ?? 0;

            var liste = await detaylar
                .OrderBy(d => d.SiparisId)
                .ThenBy(d => d.OlusturmaTarihi)
                .ToListAsync();

            return View(liste);
        }

        // GET: Siparişe Ürün Ekleme
        public ActionResult Create(int? sipId)
        {
            if (sipId == null) return RedirectToAction("Index", "Siparislers");

            ViewBag.MevcutDetaylar = db.SiparisDetaylari
                .Include(d => d.Urunler)
                .Where(d => d.SiparisId == sipId.Value)
                .ToList();

            ViewBag.Siparis = db.Siparisler
                .Include(s => s.Masalar)
                .FirstOrDefault(s => s.SiparisId == sipId.Value);

            DoldurDropDown(sipId);
            return View();
        }

        // POST: Siparişe Ürün Ekleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int SiparisId, int UrunId, decimal Miktar, decimal? IndirimTutari, string Notlar)
        {
            // Tüm değerleri manuel al, ModelState binding sorununu bypass et
            int bekliyorId = db.SiparisDurumlari
                .Where(d => d.DurumAdi == "Bekliyor")
                .Select(d => d.DurumId)
                .FirstOrDefault();

            if (bekliyorId == 0) bekliyorId = 1;
            if (Miktar <= 0) Miktar = 1;

            var urun = await db.Urunler.FindAsync(UrunId);
            if (urun == null)
            {
                TempData["hata"] = "Ürün bulunamadı.";
                return RedirectToAction("Create", new { sipId = SiparisId });
            }

            decimal toplamFiyat = (urun.Fiyat * Miktar) - (IndirimTutari ?? 0);
            if (toplamFiyat < 0) toplamFiyat = 0;

            var detay = new SiparisDetaylari
            {
                SiparisId       = SiparisId,
                UrunId          = UrunId,
                DurumId         = bekliyorId,
                Miktar          = Miktar,
                BirimFiyat      = urun.Fiyat,
                ToplamFiyat     = toplamFiyat,
                IndirimTutari   = IndirimTutari,
                Notlar          = Notlar,
                OlusturmaTarihi = DateTime.Now,
                HazirlayanId    = AktifKullaniciId
            };

            db.SiparisDetaylari.Add(detay);
            await db.SaveChangesAsync();
            await ToplamGuncelle(SiparisId);

            TempData["msj"] = "ÜRÜN EKLENDİ";
            return RedirectToAction("Create", new { sipId = SiparisId });
        }

        // POST: Tek detay durum güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DurumGuncelle(int id, int yeniDurumId, string filtre = "bekliyor")
        {
            var detay = await db.SiparisDetaylari.FindAsync(id);
            if (detay == null) return HttpNotFound();

            if (!SuperAdminMi)
            {
                var sip = await db.Siparisler.FindAsync(detay.SiparisId);
                if (sip == null || sip.RestoranId != AktifRestoranId)
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            detay.DurumId = yeniDurumId;
            await db.SaveChangesAsync();
            TempData["msj"] = "DURUM GÜNCELLENDİ";
            return RedirectToAction("Index", new { filtre });
        }

        // POST: Siparişin TÜM detaylarının durumunu güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DurumGuncelleTumu(int siparisId, int yeniDurumId, string filtre = "bekliyor")
        {
            if (!SuperAdminMi)
            {
                var sip = await db.Siparisler.FindAsync(siparisId);
                if (sip == null || sip.RestoranId != AktifRestoranId)
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var detaylar = db.SiparisDetaylari
                .Where(d => d.SiparisId == siparisId)
                .ToList();

            foreach (var d in detaylar)
                d.DurumId = yeniDurumId;

            await db.SaveChangesAsync();
            TempData["msj"] = "SİPARİŞ DURUMU GÜNCELLENDİ";
            return RedirectToAction("Index", new { filtre });
        }

        // POST: Detay sil
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id, int? geriSiparisId)
        {
            var detay = await db.SiparisDetaylari.FindAsync(id);
            if (detay != null)
            {
                int sId = detay.SiparisId;
                db.SiparisDetaylari.Remove(detay);
                await db.SaveChangesAsync();
                await ToplamGuncelle(sId);
            }

            if (geriSiparisId.HasValue)
                return RedirectToAction("Create", new { sipId = geriSiparisId.Value });

            return RedirectToAction("Index");
        }

        private async Task ToplamGuncelle(int siparisId)
        {
            var siparis = await db.Siparisler.FindAsync(siparisId);
            if (siparis == null) return;

            var detaylar = db.SiparisDetaylari
                .Where(d => d.SiparisId == siparisId)
                .ToList();

            siparis.AraToplam   = detaylar.Sum(d => d.ToplamFiyat);
            siparis.ToplamTutar = siparis.AraToplam
                                  - (siparis.IndirimTutari ?? 0)
                                  + (siparis.KDVTutari ?? 0);
            await db.SaveChangesAsync();
        }

        private void DoldurDropDown(int? siparisId = null, SiparisDetaylari detay = null)
        {
            int rId = 0;
            if (siparisId.HasValue)
            {
                var siparis = db.Siparisler.Find(siparisId.Value);
                if (siparis != null) rId = siparis.RestoranId;
            }
            if (rId == 0 && !SuperAdminMi) rId = AktifRestoranId;

            var urunSorgu = db.Urunler.Where(u => u.Aktif);
            if (rId > 0) urunSorgu = urunSorgu.Where(u => u.RestoranId == rId);

            var urunler = urunSorgu.OrderBy(u => u.UrunAdi).ToList();

            ViewBag.UrunId     = new SelectList(urunler, "UrunId", "UrunAdi", detay?.UrunId);
            ViewBag.SiparisId  = siparisId ?? 0;
            ViewBag.BekliyorId = db.SiparisDurumlari
                .FirstOrDefault(d => d.DurumAdi == "Bekliyor")?.DurumId ?? 1;

            var fiyatDict = urunler.ToDictionary(u => u.UrunId.ToString(), u => u.Fiyat);
            ViewBag.UrunFiyatlariJson = Newtonsoft.Json.JsonConvert.SerializeObject(fiyatDict);
        }
    }
}
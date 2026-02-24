using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using RestoHub.Models;

namespace RestoHub.Controllers
{
    public class MasalarsController : BaseController
    {
        // GET: Masalar Listesi (Yönetim Paneli)
        // DÜZELTME: Index sayfası Siparisler modeli beklediği için 
        // buradaki yönlendirmeyi veya modeli eşitlememiz lazım.
        public async Task<ActionResult> Index(string filtre = "aktifBos")
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            // Eğer Index view'ın Siparisler bekliyorsa, seni DurumGuncelle'ye atalım ki patlama.
            return RedirectToAction("DurumGuncelle", new { filtre });
        }

        // GET: Masa Durumu Güncelleme Ekranı (Restoran Sahası)
        public async Task<ActionResult> DurumGuncelle(string filtre = "aktifBos")
        {
            var masalar = db.Masalar.AsQueryable();
            if (!SuperAdminMi) masalar = masalar.Where(x => x.RestoranId == AktifRestoranId && x.Aktif == true);
            else masalar = masalar.Where(x => x.Aktif == true);

            if (filtre == "aktifBos") masalar = masalar.Where(x => x.Dolu == false);
            else if (filtre == "aktifDolu") masalar = masalar.Where(x => x.Dolu == true);

            ViewBag.Filtre = filtre;
            return View(await masalar.ToListAsync());
        }

        // POST: Hızlı Dolu/Boş Değiştirme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DurumGuncellePost(int id, string filtre)
        {
            var masa = await db.Masalar.FindAsync(id);
            if (masa == null || (!SuperAdminMi && masa.RestoranId != AktifRestoranId)) return HttpNotFound();

            masa.Dolu = !masa.Dolu;
            masa.GuncellemeTarihi = DateTime.Now;
            await db.SaveChangesAsync();
            return RedirectToAction("DurumGuncelle", new { filtre });
        }

        // GET: Masa Detay ve Sipariş Görüntüleme
        public async Task<ActionResult> MasaDetay(int id)
        {
            var masa = await db.Masalar.FindAsync(id);
            if (masa == null) return HttpNotFound();

            if (!SuperAdminMi && masa.RestoranId != AktifRestoranId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            // Masanın "Ödenmedi" durumundaki siparişini getir
            var siparis = await db.Siparisler
                .Include(s => s.SiparisDetaylari.Select(d => d.Urunler))
                .Include(s => s.SiparisDetaylari.Select(d => d.SiparisDurumlari))
                .Include(s => s.SiparisDurumlari)
                .Where(s => s.MasaId == id && s.OdemeDurumu == "Ödenmedi")
                .OrderByDescending(s => s.SiparisTarihi)
                .FirstOrDefaultAsync();

            // KRİTİK DÜZELTME: Masa dolu ama sipariş yoksa patlama, yeni siparişe yönlendir
            if (masa.Dolu && siparis == null)
            {
                TempData["hata"] = "Masa dolu işaretlenmiş ancak aktif bir sipariş bulunamadı. Lütfen yeni bir sipariş açın.";
                return RedirectToAction("Create", "Siparislers", new { masaId = id, restoranId = masa.RestoranId });
            }

            ViewBag.Masa = masa;
            return View(siparis);
        }

        // POST: Ödeme Al ve Masayı Boşalt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> OdemeYap(int siparisId, string odemeYontemi, string odenenTutarStr)
        {
            int[] yetkililer = { 1, 2, 3, 6 };
            if (!yetkililer.Contains(AktifYetkiId))
            {
                TempData["hata"] = "Tahsilat yapma yetkiniz yok!";
                return RedirectToAction("MasaDetay", new { id = db.Siparisler.Find(siparisId)?.MasaId });
            }

            decimal odenenTutar = 0;
            if (string.IsNullOrEmpty(odenenTutarStr) || !decimal.TryParse(odenenTutarStr.Replace(".", ","), out odenenTutar))
            {
                TempData["hata"] = "Lütfen geçerli bir ödeme tutarı giriniz.";
                return RedirectToAction("MasaDetay", new { id = db.Siparisler.Find(siparisId)?.MasaId });
            }

            var siparis = await db.Siparisler.Include(s => s.Masalar).Include(s => s.SiparisDetaylari).FirstOrDefaultAsync(s => s.SiparisId == siparisId);
            if (siparis == null) return HttpNotFound();

            if (siparis.Masalar != null)
            {
                siparis.Masalar.Dolu = false;
                db.Entry(siparis.Masalar).State = EntityState.Modified;
            }

            siparis.OdemeDurumu = "Ödendi";
            siparis.OdemeYontemi = odemeYontemi;
            siparis.ToplamTutar = odenenTutar;
            siparis.TamamlanmaTarihi = DateTime.Now;

            await db.SaveChangesAsync();
            TempData["msj"] = "Tahsilat alındı, masa boşaltıldı.";

            // DÜZELTME: Seni Index'e değil DurumGuncelle'ye atıyoruz ki Model hatası vermesin.
            return RedirectToAction("DurumGuncelle", new { filtre = "aktifBos" });
        }

        public ActionResult Create()
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            DoldurDropDown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RestoranId,MasaKodu,MasaAdi,Konum,Dolu")] Masalar masalar)
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (!SuperAdminMi) masalar.RestoranId = AktifRestoranId;

            if (await db.Masalar.AnyAsync(x => x.MasaAdi == masalar.MasaAdi && x.RestoranId == masalar.RestoranId))
                ModelState.AddModelError("MasaAdi", "Bu masa adı zaten mevcut.");

            if (ModelState.IsValid)
            {
                masalar.Aktif = true;
                masalar.OlusturmaTarihi = DateTime.Now;
                db.Masalar.Add(masalar);
                await db.SaveChangesAsync();
                TempData["msj"] = "KAYIT BAŞARILI";
                return RedirectToAction("DurumGuncelle");
            }
            DoldurDropDown(masalar);
            return View(masalar);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (AltYetkiMi || id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Masalar masalar = await db.Masalar.FindAsync(id);
            if (masalar == null || (!SuperAdminMi && masalar.RestoranId != AktifRestoranId)) return HttpNotFound();
            DoldurDropDown(masalar);
            return View(masalar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MasaId,RestoranId,MasaKodu,MasaAdi,Konum,Dolu,Aktif,OlusturmaTarihi")] Masalar masalar)
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (!SuperAdminMi && masalar.RestoranId != AktifRestoranId) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                masalar.GuncellemeTarihi = DateTime.Now;
                db.Entry(masalar).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["msj"] = "GÜNCELLEME BAŞARILI";
                return RedirectToAction("DurumGuncelle");
            }
            DoldurDropDown(masalar);
            return View(masalar);
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (AltYetkiMi || id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Masalar masalar = await db.Masalar.FindAsync(id);
            if (masalar == null || (!SuperAdminMi && masalar.RestoranId != AktifRestoranId)) return HttpNotFound();
            return View(masalar);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Masalar masalar = await db.Masalar.FindAsync(id);
            if (masalar != null && (SuperAdminMi || masalar.RestoranId == AktifRestoranId))
            {
                masalar.Aktif = false;
                masalar.GuncellemeTarihi = DateTime.Now;
                await db.SaveChangesAsync();
                TempData["msj"] = "MASA PASİF YAPILDI";
            }
            return RedirectToAction("DurumGuncelle");
        }

        private void DoldurDropDown(Masalar masalar = null)
        {
            var restoranlar = SuperAdminMi
                ? db.Restoranlar.ToList()
                : db.Restoranlar.Where(r => r.RestoranId == AktifRestoranId).ToList();
            ViewBag.RestoranId = new SelectList(restoranlar, "RestoranId", "RestoranAdi", masalar?.RestoranId);
        }
    }
}
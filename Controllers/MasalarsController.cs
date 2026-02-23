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
        public async Task<ActionResult> Index(string filtre = "aktifBos")
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            var masalar = db.Masalar.AsQueryable();
            if (!SuperAdminMi) masalar = masalar.Where(x => x.RestoranId == AktifRestoranId);

            if (filtre == "aktifBos") masalar = masalar.Where(x => x.Aktif == true && x.Dolu == false);
            else if (filtre == "aktifDolu") masalar = masalar.Where(x => x.Aktif == true && x.Dolu == true);
            else if (filtre == "aktif") masalar = masalar.Where(x => x.Aktif == true);
            else if (filtre == "pasif") masalar = masalar.Where(x => x.Aktif == false);

            ViewBag.Filtre = filtre;
            return View(await masalar.ToListAsync());
        }

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

        // GET: Masanın açık siparişini göster (Kasiyer ve üstü)
        public async Task<ActionResult> MasaDetay(int id)
        {
            var masa = await db.Masalar.FindAsync(id);
            if (masa == null) return HttpNotFound();
            if (!SuperAdminMi && masa.RestoranId != AktifRestoranId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            // Masanın açık (Ödenmedi) siparişini bul
            var siparis = await db.Siparisler
                .Include(s => s.SiparisDetaylari.Select(d => d.Urunler))
                .Include(s => s.SiparisDurumlari)
                .Where(s => s.MasaId == id && s.OdemeDurumu == "Ödenmedi")
                .OrderByDescending(s => s.SiparisTarihi)
                .FirstOrDefaultAsync();

            ViewBag.Masa = masa;
            return View(siparis); // siparis null olabilir (masa dolu ama sipariş kaydı yoksa)
        }

        // POST: Ödeme al
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> OdemeYap(int siparisId, string odemeYontemi, decimal odenenTutar)
        {
            var siparis = await db.Siparisler
                .Include(s => s.Masalar)
                .FirstOrDefaultAsync(s => s.SiparisId == siparisId);

            if (siparis == null) return HttpNotFound();
            if (!SuperAdminMi && siparis.RestoranId != AktifRestoranId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            siparis.OdemeDurumu  = "Ödendi";
            siparis.OdemeYontemi = odemeYontemi;
            siparis.ToplamTutar  = odenenTutar;
            siparis.SiparisTarihi  = DateTime.Now;

            // Masayı boşalt
            if (siparis.Masalar != null)
            {
                siparis.Masalar.Dolu = false;
                siparis.Masalar.GuncellemeTarihi = DateTime.Now;
            }

            // Siparişin tüm detaylarını "Tamamlandı" durumuna çek
            var tamamlandiId = db.SiparisDurumlari
                .Where(d => d.DurumAdi == "Tamamlandı")
                .Select(d => d.DurumId)
                .FirstOrDefault();

            if (tamamlandiId > 0)
            {
                var detaylar = db.SiparisDetaylari.Where(d => d.SiparisId == siparisId).ToList();
                foreach (var d in detaylar)
                    d.DurumId = tamamlandiId;
            }

            await db.SaveChangesAsync();
            TempData["msj"] = "ÖDEME ALINDI";
            return RedirectToAction("DurumGuncelle", new { filtre = "aktifDolu" });
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
                ModelState.AddModelError("MasaAdi", "Bu masa adı bu restoranda kayıtlı.");

            if (ModelState.IsValid)
            {
                masalar.Aktif = true;
                masalar.OlusturmaTarihi = DateTime.Now;
                db.Masalar.Add(masalar);
                await db.SaveChangesAsync();
                TempData["msj"] = "KAYIT BAŞARILI";
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
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
            return RedirectToAction("Index");
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
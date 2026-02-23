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
    public class RestoranlarsController : BaseController
    {
        // --- RESTORAN LİSTESİ (Sadece SuperAdmin) ---
        public async Task<ActionResult> Index(bool? aktif)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var restoranlar = db.Restoranlar.AsQueryable();
            if (aktif.HasValue)
                restoranlar = restoranlar.Where(x => x.Aktif == aktif.Value);

            return View(await restoranlar.ToListAsync());
        }

        // --- RESTORANIM SAYFASI (Özel Düzenleme Sayfası) ---
        public async Task<ActionResult> Restoranim()
        {
            if (!RestoranSahibiMi && !SuperAdminMi)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var restoran = await db.Restoranlar.FindAsync(AktifRestoranId);
            if (restoran == null) return HttpNotFound();

            // Dosya adın Restoranim.cshtml olduğu için onu çağırıyoruz
            return View("Restoranim", restoran);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Restoranim([Bind(Include = "RestoranId,RestoranAdi,Telefon,Email,Adres,Aktif,OlusturmaTarihi")] Restoranlar restoranlar)
        {
            if (!RestoranSahibiMi && !SuperAdminMi)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (restoranlar.RestoranId != AktifRestoranId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (!ModelState.IsValid)
                return View("Restoranim", restoranlar);

            restoranlar.GuncellemeTarihi = DateTime.Now;
            db.Entry(restoranlar).State = EntityState.Modified;
            await db.SaveChangesAsync();

            TempData["msj"] = "BİLGİLER GÜNCELLENDİ";
            return View("Restoranim", restoranlar);
        }

        // --- YENİ KAYIT ---
        public ActionResult Create()
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RestoranAdi,Telefon,Email,Adres")] Restoranlar restoranlar)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (await db.Restoranlar.AnyAsync(x => x.RestoranAdi == restoranlar.RestoranAdi))
                ModelState.AddModelError("RestoranAdi", "Bu isimde bir restoran zaten var.");

            if (!ModelState.IsValid) return View(restoranlar);

            restoranlar.Aktif = true;
            restoranlar.OlusturmaTarihi = DateTime.Now;
            db.Restoranlar.Add(restoranlar);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // --- EDİT (Admin Paneli İçin) ---
        public async Task<ActionResult> Edit(int? id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Restoranlar restoranlar = await db.Restoranlar.FindAsync(id);
            if (restoranlar == null) return HttpNotFound();
            return View(restoranlar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "RestoranId,RestoranAdi,Telefon,Email,Adres,Aktif,OlusturmaTarihi")] Restoranlar restoranlar)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (!ModelState.IsValid) return View(restoranlar);

            restoranlar.GuncellemeTarihi = DateTime.Now;
            db.Entry(restoranlar).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // --- SİLME ---
        public async Task<ActionResult> Delete(int? id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Restoranlar restoranlar = await db.Restoranlar.FindAsync(id);
            if (restoranlar == null) return HttpNotFound();
            return View(restoranlar);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            Restoranlar restoranlar = await db.Restoranlar.FindAsync(id);
            if (restoranlar != null)
            {
                restoranlar.Aktif = false;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
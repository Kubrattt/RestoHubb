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
    public class BirimTiplerisController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(await db.BirimTipleri.ToListAsync());
        }

        public ActionResult Create()
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "BirimAdi,BirimSembol")] BirimTipleri birimTipleri)
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (await db.BirimTipleri.AnyAsync(x => x.BirimAdi == birimTipleri.BirimAdi))
                ModelState.AddModelError("BirimAdi", "Bu birim tipi sistemde kayıtlı. Lütfen başka bir birim tipi giriniz");

            if (!ModelState.IsValid)
                return View(birimTipleri);

            birimTipleri.OlusturmaTarihi = DateTime.Now;
            db.BirimTipleri.Add(birimTipleri);
            await db.SaveChangesAsync();

            TempData["msj"] = "KAYIT BAŞARILI";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            BirimTipleri birimTipleri = await db.BirimTipleri.FindAsync(id);
            if (birimTipleri == null)
                return HttpNotFound();

            return View(birimTipleri);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "BirimId,BirimAdi,BirimSembol,OlusturmaTarihi")] BirimTipleri birimTipleri)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (await db.BirimTipleri.AnyAsync(x =>
                x.BirimAdi == birimTipleri.BirimAdi &&
                x.BirimId != birimTipleri.BirimId))
            {
                ModelState.AddModelError("BirimTipleri", "Bu birim tipi sistemde kayıtlı. Lütfen başka bir birim tipi giriniz");
            }

            if (!ModelState.IsValid)
                return View(birimTipleri);

            db.Entry(birimTipleri).State = EntityState.Modified;
            await db.SaveChangesAsync();

            TempData["msj"] = "GÜNCELLEME BAŞARILI";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            BirimTipleri birimTipleri = await db.BirimTipleri.FindAsync(id);
            if (birimTipleri == null)
                return HttpNotFound();

            return View(birimTipleri);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            BirimTipleri birimTipleri = await db.BirimTipleri.FindAsync(id);
            db.BirimTipleri.Remove(birimTipleri);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
// Controllers/ServicesController.cs
using Kurs_HTML.Data;
using Kurs_HTML.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kurs_HTML.Controllers
{
public class ServicesController : Controller
{
    private readonly ApplicationDbContext _db;
    public ServicesController(ApplicationDbContext db) => _db = db;

    // GET: /Services
    public async Task<IActionResult> Index()
    {
        var list = await _db.Services
            .Select(s => new ServiceViewModel {
                ServiceId    = s.ServiceId,
                Name         = s.Name,
                Category     = s.Category,
                Notes        = s.Notes,
                BasePrice    = s.BasePrice,
                Duration     = s.Duration,
                PerformerRole= s.PerformerRole
            })
            .ToListAsync();
        return View(list);
    }

    // GET: /Services/Create
    public IActionResult Create() => View(new ServiceViewModel());

    // POST: /Services/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var ent = new Service {
            Name          = vm.Name,
            Notes         = vm.Notes,
            Category      = vm.Category,
            BasePrice     = vm.BasePrice,
            Duration      = vm.Duration,
            PerformerRole = vm.PerformerRole
        };
        _db.Services.Add(ent);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Services/Edit/5
    public IActionResult Edit(int id)
    {
        var s = _db.Services.Find(id);
        if (s == null) return NotFound();
        var vm = new ServiceViewModel {
            ServiceId     = s.ServiceId,
            Name          = s.Name,
            Notes         = s.Notes,
            Category      = s.Category,
            BasePrice     = s.BasePrice,
            Duration      = s.Duration,
            PerformerRole = s.PerformerRole
        };
        return View(vm);
    }

    // POST: /Services/Edit
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(ServiceViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var s = _db.Services.Find(vm.ServiceId);
        if (s == null) return NotFound();

        s.Name          = vm.Name;
        s.Notes         = vm.Notes;
        s.Category      = vm.Category;
        s.BasePrice     = vm.BasePrice;
        s.Duration      = vm.Duration;
        s.PerformerRole = vm.PerformerRole;

        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Services/Delete/5
    public IActionResult Delete(int id)
    {
        var s = _db.Services.Find(id);
        if (s == null) return NotFound();
        var vm = new ServiceViewModel {
            ServiceId = s.ServiceId,
            Name      = s.Name
        };
        return View(vm);
    }

    // POST: /Services/Delete
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(ServiceViewModel vm)
    {
        var s = _db.Services.Find(vm.ServiceId);
        if (s != null)
        {
            _db.Services.Remove(s);
            _db.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }

}

}

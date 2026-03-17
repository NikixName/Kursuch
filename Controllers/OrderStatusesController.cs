
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kurs_HTML.Data;
using Kurs_HTML.Models;

namespace Kurs_HTML.Controllers
{
    public class OrderStatusesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationDbContext _db;

        public OrderStatusesController(ApplicationDbContext db, ApplicationDbContext context)
        {
            _db = db;
            _context = context;
        }

        // GET: /OrderStatuses
        public async Task<IActionResult> Index()
        {
            var statuses = await _db.OrderStatuses
                                    .OrderBy(s => s.OrderStatusId)
                                    .ToListAsync();
            return View(statuses);
        }

        // GET: /OrderStatuses/Create
        public IActionResult Create()
        {
            return View(new OrderStatus());
        }

        // POST: /OrderStatuses/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderStatus m)
        {
            if (!ModelState.IsValid)
                return View(m);

            // Можно проверить, чтобы не было одинаковых Name:
            if (await _db.OrderStatuses.AnyAsync(s => s.Name == m.Name.Trim()))
            {
                ModelState.AddModelError(nameof(m.Name), "Статус с таким именем уже существует.");
                return View(m);
            }

            m.Name = m.Name.Trim();
            _db.OrderStatuses.Add(m);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

         


        [HttpGet]
        public IActionResult PaymentForm(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.Service)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            var model = new PaymentViewModel
            {
                OrderId = order.OrderId,
                ServiceName = order.Service.Name,
                Amount = order.Service.BasePrice
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult ConfirmPayment(int orderId)
        {
            var order = _db.Orders
                .Include(o => o.Service)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null) return NotFound();

            var model = new PaymentViewModel
            {
                OrderId = orderId,
                Amount = order.Service.BasePrice
            };

            return View(model); 
        }


        [HttpPost]
        public IActionResult ConfirmPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var order = _db.Orders
                .Include(o => o.Service)
                .FirstOrDefault(o => o.OrderId == model.OrderId);

            if (order == null) return NotFound();

            var existingReceipt = _db.Receipts.FirstOrDefault(r => r.OrderId == model.OrderId);
            if (existingReceipt == null)
            {
                _db.Receipts.Add(new Receipt
                {
                    OrderId = order.OrderId,
                    Amount = order.Service.BasePrice,
                    CreatedAt = DateTime.Now,
                    PayerName = model.PayerName
                });
            }

    order.IsPaid = true;
    _db.SaveChanges();

    return RedirectToAction("Profile", "Home");
}

        [HttpGet]
        public IActionResult MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var orders = _db.Orders
                .Include(o => o.Service)
                .Where(o => o.ClientId == userId.Value && !o.IsDeleted)
                .ToList();

            return View(orders);
        }

        // GET: /OrderStatuses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var s = await _db.OrderStatuses.FindAsync(id.Value);
            if (s == null) return NotFound();
            return View(s);
        }

        // POST: /OrderStatuses/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderStatus m)
        {
            if (id != m.OrderStatusId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(m);

            // Проверка на дубликат:
            if (await _db.OrderStatuses
                        .AnyAsync(s => s.OrderStatusId != id
                                    && s.Name == m.Name.Trim()))
            {
                ModelState.AddModelError(nameof(m.Name), "Статус с таким именем уже существует.");
                return View(m);
            }

            var existing = await _db.OrderStatuses.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = m.Name.Trim();
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /OrderStatuses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var s = await _db.OrderStatuses.FindAsync(id.Value);
            if (s == null) return NotFound();
            return View(s);
        }

        // POST: /OrderStatuses/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var s = await _db.OrderStatuses.FindAsync(id);
            if (s != null)
            {
                bool used = await _db.Orders.AnyAsync(o => o.OrderStatusId == id);
                if (used)
                {
                    TempData["Error"] = "Нельзя удалить статус – он используется в существующих заказах.";
                    return RedirectToAction(nameof(Index));
                }

                _db.OrderStatuses.Remove(s);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

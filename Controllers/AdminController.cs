using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kurs_HTML.Data;
using Kurs_HTML.Models;
using System.Linq;
using ClosedXML.Excel;

namespace Kurs_HTML.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }
        [HttpGet("Orders")]
        public IActionResult Orders()
        {
            var orders = _db.Orders
                .Include(o => o.Service)
                .Include(o => o.OrderStatus)
                .Include(o => o.Mechanic)
                .Include(o => o.CarWasher)
                .Where(o => !o.IsPaid)
                .Select(o => new OrderViewModel
                {
                    OrderId       = o.OrderId,
                    ServiceName   = o.Service.Name,
                    DateCreated   = o.DateCreated,
                    DateTime      = o.DateTime,
                    Name          = o.OrderStatus.Name,
                    PerformerName = o.MechanicId.HasValue
                        ? o.Mechanic!.FirstName + " " + o.Mechanic.LastName
                        : (o.CarWasherId.HasValue
                            ? o.CarWasher!.FirstName + " " + o.CarWasher.LastName
                            : "(не назначен)")
                })
                .ToList();

            return View(orders);
        }
        // GET: /Admin/EditOrder/5
        [HttpGet("EditOrder")]
        public IActionResult EditOrder(int id)
        {
            var o = _db.Orders
                .Include(x => x.Service)
                .Include(x => x.OrderStatus)
                .Include(x => x.Mechanic)
                .Include(x => x.CarWasher)
                .FirstOrDefault(x => x.OrderId == id);

            if (o == null)
                return NotFound();

            var allStatuses = _db.OrderStatuses.OrderBy(st => st.OrderStatusId).ToList();

            var mechList = _db.Mechanics.Select(m => new AssignedPersonItem
            {
                Value = $"Mech_{m.Id}",
                Text  = $"Механик: {m.FirstName} {m.LastName}"
            }).ToList();
            var washList = _db.CarWashers.Select(w => new AssignedPersonItem
            {
                Value = $"Washer_{w.Id}",
                Text  = $"Автомойщик: {w.FirstName} {w.LastName}"
            }).ToList();
            var allPerformers = mechList.Concat(washList).ToList();

            var vm = new EditOrderViewModel
            {
                OrderId            = o.OrderId,
                ServiceName        = o.Service.Name,
                CurrentDateTime    = o.DateTime,
                AllStatuses        = allStatuses,
                SelectedStatusId   = o.OrderStatusId,
                AllPerformers      = allPerformers,
                PerformerRoleAndId = o.MechanicId.HasValue ? $"Mech_{o.MechanicId.Value}"
                                 : o.CarWasherId.HasValue ? $"Washer_{o.CarWasherId.Value}"
                                 : null
            };

            return View(vm);
        }

        [HttpPost("ExportMonthlyIncomeReport")]
        public IActionResult ExportMonthlyIncomeReport()
        {
            var now = DateTime.Now;

            var reportData = _db.CompletedOrders
                .Where(o => o.CompletedAt.Month == now.Month && o.CompletedAt.Year == now.Year)
                .GroupBy(o => o.ServiceName)
                .Select(g => new
                {
                    ServiceName = g.Key,
                    Count = g.Count(),
                    Total = g.Count() * (_db.Services.FirstOrDefault(s => s.Name == g.Key)!.BasePrice)
                })
                .ToList();

            var grandTotal = reportData.Sum(r => r.Total);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Доход за месяц");

            string monthName = now.ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));
            ws.Cell(1, 1).Value = $"Отчет о доходах за {monthName}";
            ws.Range(1, 1, 1, 3).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.LightBlue);

            ws.Cell(2, 1).Value = "Услуга";
            ws.Cell(2, 2).Value = "Кол-во выполнений";
            ws.Cell(2, 3).Value = "Доход (BYN)";

            var headerRange = ws.Range(2, 1, 2, 3);
            headerRange.Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.LightGray)
                .Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            for (int i = 0; i < reportData.Count; i++)
            {
                ws.Cell(i + 3, 1).Value = reportData[i].ServiceName;
                ws.Cell(i + 3, 2).Value = reportData[i].Count;
                ws.Cell(i + 3, 3).Value = reportData[i].Total;
            }

            int totalRow = reportData.Count + 4;
            ws.Cell(totalRow, 2).Value = "Общий доход:";
            ws.Cell(totalRow, 3).Value = grandTotal;
            ws.Range(totalRow, 2, totalRow, 3).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.LightGreen)
                .Border.OutsideBorder = XLBorderStyleValues.Thin;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Доход_{now:MMMM_yyyy}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }


        // POST: /Admin/EditOrder
        [HttpPost("EditOrder"), ValidateAntiForgeryToken]
        public IActionResult EditOrder(EditOrderViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AllStatuses = _db.OrderStatuses.OrderBy(st => st.OrderStatusId).ToList();
                var mechList = _db.Mechanics.Select(m => new AssignedPersonItem
                {
                    Value = $"Mech_{m.Id}",
                    Text  = $"Механик: {m.FirstName} {m.LastName}"
                }).ToList();
                var washList = _db.CarWashers.Select(w => new AssignedPersonItem
                {
                    Value = $"Washer_{w.Id}",
                    Text  = $"Автомойщик: {w.FirstName} {w.LastName}"
                }).ToList();
                vm.AllPerformers = mechList.Concat(washList).ToList();
                return View(vm);
            }

            var o = _db.Orders.Find(vm.OrderId);
            if (o == null) return NotFound();

            o.DateTime     = vm.CurrentDateTime;
            o.OrderStatusId = vm.SelectedStatusId;

            int? mechId = null, washId = null;
            if (!string.IsNullOrEmpty(vm.PerformerRoleAndId))
            {
                if (vm.PerformerRoleAndId.StartsWith("Mech_"))
                {
                    var parts = vm.PerformerRoleAndId.Split('_');
                    mechId = int.Parse(parts[1]);
                    washId = null;
                }
                else if (vm.PerformerRoleAndId.StartsWith("Washer_"))
                {
                    var parts = vm.PerformerRoleAndId.Split('_');
                    washId = int.Parse(parts[1]);
                    mechId = null;
                }
            }
            o.MechanicId   = mechId;
            o.CarWasherId  = washId;

            _db.SaveChanges();
            return RedirectToAction("Orders");
        }

        // POST: /Admin/DeleteOrder
        [HttpPost("DeleteOrder"), ValidateAntiForgeryToken]
        public IActionResult DeleteOrder(int orderId)
        {
            var o = _db.Orders.Find(orderId);
            if (o != null)
            {
                _db.Orders.Remove(o);
                _db.SaveChanges();
            }
            return RedirectToAction("Orders");
        }

        // POST: /Admin/ChangeStatus
        [HttpPost("ChangeStatus"), ValidateAntiForgeryToken]
        public IActionResult ChangeStatus(int orderId, int newStatusId)
        {
            var o = _db.Orders.Find(orderId);
            if (o == null) return NotFound();

            if (!_db.OrderStatuses.Any(st => st.OrderStatusId == newStatusId))
            {
                ModelState.AddModelError("", "Некорректный статус");
                return RedirectToAction("Orders");
            }

            o.OrderStatusId = newStatusId;
            _db.SaveChanges();
            return RedirectToAction("Orders");
        }


        [HttpGet("CompletedOrders")]
        public IActionResult CompletedOrders()
        {
            var model = _db.CompletedOrders
                .Select(o => new AdminCompletedOrderViewModel
                {
                    OrderId       = o.OrderId,
                    ServiceName   = o.ServiceName,
                    PerformerName = o.PerformerName,
                    CompletedAt   = o.CompletedAt
                })
                .ToList();

            return View(model);
        }
        // GET: /Admin/ExportCompletedToExcel
        [HttpGet("ExportCompletedToExcel")]
        public IActionResult ExportCompletedToExcel()
        {

            var completedStatus = _db.OrderStatuses.FirstOrDefault(st => st.Name == "Completed");
            if (completedStatus == null)
                return RedirectToAction("CompletedOrders");

            var orders = _db.Orders
                .Where(o => o.OrderStatusId == completedStatus.OrderStatusId)
                .Include(o => o.Service)
                .Include(o => o.Client)
                .Include(o => o.Mechanic)
                .Include(o => o.CarWasher)
                .Select(o => new
                {
                    o.OrderId,
                    ClientName    = o.Client.FirstName + " " + o.Client.LastName,
                    ServiceName   = o.Service.Name,
                    DateTime      = o.DateTime,
                    PerformerName = o.MechanicId.HasValue
                                        ? o.Mechanic!.FirstName + " " + o.Mechanic.LastName
                                        : (o.CarWasherId.HasValue 
                                           ? o.CarWasher!.FirstName + " " + o.CarWasher.LastName
                                           : "(не назначен)"),
                    Status        = o.OrderStatus.Name
                })
                .ToList();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ВыполненныеЗаказы");
                worksheet.Cell(1, 1).Value = "№ Заказа";
                worksheet.Cell(1, 2).Value = "Клиент";
                worksheet.Cell(1, 3).Value = "Услуга";
                worksheet.Cell(1, 4).Value = "Дата/Время";
                worksheet.Cell(1, 5).Value = "Исполнитель";
                worksheet.Cell(1, 6).Value = "Статус";

                int row = 2;
                foreach (var o in orders)
                {
                    worksheet.Cell(row, 1).Value = o.OrderId;
                    worksheet.Cell(row, 2).Value = o.ClientName;
                    worksheet.Cell(row, 3).Value = o.ServiceName;
                    worksheet.Cell(row, 4).Value = o.DateTime.ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cell(row, 5).Value = o.PerformerName;
                    worksheet.Cell(row, 6).Value = o.Status;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    string excelName = $"CompletedOrders_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        excelName);
                }
            }
        }
    }
}

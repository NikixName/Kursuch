using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Kurs_HTML.Data;
using Kurs_HTML.Models;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

namespace Kurs_HTML.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly string _avatarFolder = "wwwroot/images/avatars";

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
            if (!Directory.Exists(_avatarFolder))
                Directory.CreateDirectory(_avatarFolder);
        }

        // 1) Главная: GET / или /Home/Index
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        // 2) Альтернативная главная после входа: GET /Home/Index1
        [HttpGet("Index1")]
        public IActionResult Index1()
        {
            return View();
        }

        // 3) Каталог услуг: GET /Home/Table?serviceId=...
        [HttpGet("Table")]
        public IActionResult Table(int? serviceId)
        {
            var now = DateTime.Now;
            var services = _db.Services.Select(s => new ServiceViewModel
            {
                ServiceId = s.ServiceId,
                Name = s.Name,
                Notes = s.Notes,
                Category = s.Category,
                BasePrice = s.BasePrice,
                Duration = s.Duration,
                Available = !_db.Orders.Any(o =>
                                 o.ServiceId == s.ServiceId
                                 && o.DateTime > now
                                 && o.DateTime < now.AddHours(24)),
                DiscountText = s.Notes.Contains("Акция") ? "-20%" : null
            }).ToList();

            var vm = new TableViewModel
            {
                Services = services,
                ServiceToBook = serviceId,
                PreselectSlot = serviceId.HasValue
                    ? _db.Orders
                        .Where(o => o.ServiceId == serviceId.Value && o.DateTime > now)
                        .OrderBy(o => o.DateTime)
                        .Select(o => (DateTime?)o.DateTime)
                        .FirstOrDefault()
                    : null
            };

            return View(vm);
        }

        // 4) Вход: GET /Home/SignIn
        [HttpGet("SignIn")]
        public IActionResult SignIn()
        {
            return View(new SignInViewModel());
        }

        // 5) Вход: POST /Home/SignIn
        [HttpPost("SignIn"), ValidateAntiForgeryToken]
        public IActionResult SignIn(SignInViewModel m)
        {
            if (!ModelState.IsValid)
                return View(m);

            if (_TryLogin(m.Email, m.Password, out var id, out var role))
                return SignInSuccess(id, role);

            ModelState.AddModelError("", "Неверный логин или пароль");
            return View(m);
        }

        // 6) Выход: POST /Home/Logout
        [HttpPost("Logout"), ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // 7) Регистрация: GET /Home/Register
        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // 8) Регистрация: POST /Home/Register
        [HttpPost("Register"), ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel m)
        {
            if (!ModelState.IsValid)
                return View(m);

            if (_db.Clients.Any(u => u.Email == m.Email))
            {
                ModelState.AddModelError(nameof(m.Email), "Пользователь с такой почтой уже существует");
                return View(m);
            }

            _db.Clients.Add(new Client
            {
                FirstName = m.FirstName,
                LastName = m.LastName,
                Email = m.Email,
                Password = m.Password
            });
            _db.SaveChanges();

            return RedirectToAction("Index1");
        }

        [HttpPost("ChangeStatus"), ValidateAntiForgeryToken]
        public IActionResult ChangeStatus(int orderId, int newStatusId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || string.IsNullOrEmpty(userRole))
                return RedirectToAction("SignIn");

            var order = _db.Orders.Find(orderId);
            if (order == null)
                return NotFound();

            if (userRole == "Mechanic" && order.MechanicId != userId.Value)
                return Forbid();
            if (userRole == "CarWasher" && order.CarWasherId != userId.Value)
                return Forbid();
            // Админ может менять статус у любого заказа, поэтому его проверяем отдельно
            if (userRole != "Administrator" && userRole != "Mechanic" && userRole != "CarWasher")
                return Forbid();

            // Проверяем, что переданный статус существует
            if (!_db.OrderStatuses.Any(st => st.OrderStatusId == newStatusId))
            {
                ModelState.AddModelError("", "Некорректный статус.");
                return RedirectToAction("Profile");
            }

            order.OrderStatusId = newStatusId;
            _db.SaveChanges();
            return RedirectToAction("Profile");
        }


        // 9) Профиль: GET /Home/Profile
        [HttpGet("Profile")]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || string.IsNullOrEmpty(userRole))
                return RedirectToAction("SignIn");

            var model = new ProfileViewModel();

            switch (userRole)
            {
                case "Client":
                    MapUserFields(_db.Clients.Find(userId.Value), model);
                    break;
                case "Administrator":
                    MapUserFields(_db.Administrators.Find(userId.Value), model);
                    break;
                case "Mechanic":
                    MapUserFields(_db.Mechanics.Find(userId.Value), model);
                    break;
                case "CarWasher":
                    MapUserFields(_db.CarWashers.Find(userId.Value), model);
                    break;
                default:
                    return RedirectToAction("SignIn");
            }

            List<OrderViewModel> orders = new();

            if (userRole == "Client")
            {
                orders = _db.Orders
                    .Where(o => o.ClientId == userId.Value &&
                               (   (o.OrderStatus.Name == "Обрабатываеться") ||
                                   (o.OrderStatus.Name == "В процессе") ||
                                   (o.OrderStatus.Name == "Выполнен" && !o.IsPaid) ||
                                   (o.OrderStatus.Name == "Выполнен" && o.IsPaid)
                               ) &&
                                   !o.IsReceiptDownloaded
                               )
                    .Include(o => o.Service)
                    .Include(o => o.OrderStatus)
                    .Select(o => new OrderViewModel
                    {
                        OrderId = o.OrderId,
                        ServiceName = o.Service.Name,
                        DateCreated = o.DateCreated,
                        DateTime = o.DateTime,
                        Name = o.OrderStatus.Name,
                        IsPaid = o.IsPaid
                    })
                    .ToList();
            }
            else if (userRole == "Mechanic")
            {
                orders = _db.Orders
                    .Where(o =>
                        (o.MechanicId == userId.Value || (o.MechanicId == null && o.Service.PerformerRole == "Mechanic")) &&
                        o.OrderStatus.Name != "Выполнен" &&
                         o.IsPaid == false)

                    .Include(o => o.Service)
                    .Include(o => o.OrderStatus)
                    .Select(o => new OrderViewModel
                    {
                        OrderId = o.OrderId,
                        ServiceName = o.Service.Name,
                        DateCreated = o.DateCreated,
                        DateTime = o.DateTime,
                        Name = o.OrderStatus.Name,
                        PerformerName = o.MechanicId == userId.Value ? "Вы" : "(не назначен)"
                    })
                    .ToList();
            }

            else if (userRole == "CarWasher")
            {
                orders = _db.Orders
                    .Where(o =>
                        (o.CarWasherId == userId.Value || (o.CarWasherId == null && o.Service.PerformerRole == "CarWasher")) &&
                        o.OrderStatus.Name != "Выполнен" &&
                         o.IsPaid == false)
                    .Include(o => o.Service)
                    .Include(o => o.OrderStatus)
                    .Select(o => new OrderViewModel
                    {
                        OrderId = o.OrderId,
                        ServiceName = o.Service.Name,
                        DateCreated = o.DateCreated,
                        DateTime = o.DateTime,
                        Name = o.OrderStatus.Name,
                        PerformerName = o.CarWasherId == userId.Value ? "Вы" : "(не назначен)"
                    })
                    .ToList();
            }

            else if (userRole == "Administrator")
            {
                orders = _db.Orders
                    .Include(o => o.Service)
                    .Include(o => o.OrderStatus)
                    .Select(o => new OrderViewModel
                    {
                        OrderId = o.OrderId,
                        ServiceName = o.Service.Name,
                        DateCreated = o.DateCreated,
                        DateTime = o.DateTime,
                        Name = o.OrderStatus.Name
                    })
                    .ToList();
            }

            model.Orders = orders;
            return View(model);
        }



        // 10) Сохранение профиля: POST /Home/Profile
        [HttpPost("Profile"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel m)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || string.IsNullOrEmpty(userRole))
                return RedirectToAction("SignIn");

            dynamic userEntity = userRole switch
            {
                "Client" => _db.Clients.Find(userId.Value),
                "Administrator" => _db.Administrators.Find(userId.Value),
                "Mechanic" => _db.Mechanics.Find(userId.Value),
                "CarWasher" => _db.CarWashers.Find(userId.Value),
                _ => null
            };
            if (userEntity == null)
                return RedirectToAction("SignIn");

            if (m.AvatarFile?.Length > 0)
            {
                var fileName = $"{userRole}_{userId}_{Path.GetFileName(m.AvatarFile.FileName)}";
                var savePath = Path.Combine(_avatarFolder, fileName);
                await using var fs = System.IO.File.Create(savePath);
                await m.AvatarFile.CopyToAsync(fs);
                userEntity.AvatarPath = $"/images/avatars/{fileName}";
            }

            userEntity.FirstName = m.FirstName;
            userEntity.LastName = m.LastName;
            userEntity.Phone = m.Phone;
            userEntity.Car = m.Car;
            userEntity.License = m.License;

            _db.SaveChanges();
            return RedirectToAction("Profile");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult AcceptOrder(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var order = _db.Orders
                .Include(o => o.Service)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null || userId == null || string.IsNullOrEmpty(userRole))
                return RedirectToAction("Profile");

            if (userRole == "Mechanic" && order.MechanicId == null && order.Service.PerformerRole == "Mechanic")
            {
                order.MechanicId = userId;
                order.OrderStatusId = 2; // Выполняется
            }
            else if (userRole == "CarWasher" && order.CarWasherId == null && order.Service.PerformerRole == "CarWasher")
            {
                order.CarWasherId = userId;
                order.OrderStatusId = 2; // Выполняется
            }

            _db.SaveChanges();
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteOrder(int orderId)
        {
            var order = _db.Orders
                .Include(o => o.Service)
                .Include(o => o.Mechanic)
                .Include(o => o.CarWasher)
                .FirstOrDefault(o => o.OrderId == orderId);

            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (order == null || userId == null || string.IsNullOrEmpty(userRole))
                return RedirectToAction("Profile");

            if ((userRole == "Mechanic" && order.MechanicId == userId.Value) ||
                (userRole == "CarWasher" && order.CarWasherId == userId.Value))
            {
                // Переход на форму составления акта
                return RedirectToAction("WorkReportForm", new { orderId = order.OrderId });
            }

            return RedirectToAction("Profile");
        }


        [HttpGet]
        public IActionResult WorkReportForm(int orderId)
        {
            var order = _db.Orders.Include(o => o.Service).FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            ViewBag.ServiceName = order.Service.Name;
            return View(new WorkReportViewModel { OrderId = orderId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitWorkReport(WorkReportViewModel model)
        {
            var order = _db.Orders
                .Include(o => o.Service)
                .Include(o => o.Mechanic)
                .Include(o => o.CarWasher)
                .FirstOrDefault(o => o.OrderId == model.OrderId);

            if (order == null) return NotFound();

            // Добавляем акт
            _db.WorkReports.Add(new WorkReport
            {
                OrderId = order.OrderId,
                Comments = model.Comments,
                CreatedAt = DateTime.Now
            });

            if (!_db.Receipts.Any(r => r.OrderId == order.OrderId))
            {
                _db.Receipts.Add(new Receipt
                {
                    OrderId = order.OrderId,
                    Amount = order.Service?.BasePrice ?? 0,
                    CreatedAt = DateTime.Now
                });
            }

            // Меняем статус
            var completedStatus = _db.OrderStatuses.FirstOrDefault(s => s.Name == "Выполнен");
            if (completedStatus != null)
                order.OrderStatusId = completedStatus.OrderStatusId;

            // Добавляем в CompletedOrders
            var performerName = order.Mechanic != null
                ? $"{order.Mechanic.FirstName} {order.Mechanic.LastName}"
                : order.CarWasher != null
                    ? $"{order.CarWasher.FirstName} {order.CarWasher.LastName}"
                    : "(неизвестно)";

            _db.CompletedOrders.Add(new CompletedOrder
            {
                OrderId = order.OrderId,
                CompletedAt = DateTime.Now,
                PerformerName = performerName,
                ServiceName = order.Service?.Name ?? "(неизвестно)"
            });

            _db.SaveChanges();
            return RedirectToAction("Profile");
        }



        public IActionResult DownloadReport(int orderId)
        {
            var report = _db.WorkReports
                .Include(r => r.Order)
                .ThenInclude(o => o.Service)
                .FirstOrDefault(r => r.OrderId == orderId);

            if (report == null)
                return NotFound();

            var content = $"Акт по заказу #{orderId}\nУслуга: {report.Order.Service.Name}\nДата: {report.CreatedAt:dd.MM.yyyy}\nКомментарии: {report.Comments}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);

            return File(bytes, "text/plain", $"WorkReport_{orderId}.txt");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PayOrder(int orderId)
        {
            var order = _db.Orders.Include(o => o.Service).FirstOrDefault(o => o.OrderId == orderId);
            if (order == null || order.IsPaid) return NotFound();

            order.IsPaid = true;
            _db.Receipts.Add(new Receipt
            {
                OrderId = order.OrderId,
                Amount = order.Service.BasePrice,
                CreatedAt = DateTime.Now
            });

            _db.SaveChanges();
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult DownloadReceipt(int orderId)
        {

            if (orderId <= 0)
            {
                Console.WriteLine("❌ Некорректный orderId");
                return BadRequest("Некорректный идентификатор заказа.");
            }

            try
            {
                var receipt = _db.Receipts
                    .Include(r => r.Order)
                        .ThenInclude(o => o.Service)
                    .Include(r => r.Order)
                        .ThenInclude(o => o.WorkReport)
                    .FirstOrDefault(r => r.OrderId == orderId);

                var order = receipt.Order;

                order.IsPaid = true;
                order.IsReceiptDownloaded = true;
                order.IsDeleted = true;
                _db.SaveChanges();

                using var mem = new MemoryStream();
                using (var wordDoc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
                {
                    var mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    var body = new Body();

                    body.Append(CreateParagraph("ЧЕК", bold: true, size: 28, center: true));
                    body.Append(CreateParagraph($"Дата оплаты: {receipt.CreatedAt:dd.MM.yyyy}", center: true));
                    body.Append(new Paragraph(new Run(new Text(""))));

                    body.Append(CreateParagraph($"Номер заказа: {receipt.OrderId}"));
                    body.Append(CreateParagraph($"Услуга: {order.Service.Name}"));
                    body.Append(CreateParagraph($"Сумма: {receipt.Amount:F2} руб."));

                    if (!string.IsNullOrWhiteSpace(receipt.PayerName))
                    {
                        body.Append(CreateParagraph($"Плательщик: {receipt.PayerName}"));
                    }

                    body.Append(new Paragraph(new Run(new Text(""))));

                    if (order.WorkReport != null && !string.IsNullOrWhiteSpace(order.WorkReport.Comments))
                    {
                        body.Append(CreateParagraph("Комментарии исполнителя:", bold: true));
                        var parts = order.WorkReport.Comments.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            body.Append(CreateParagraph("• " + part.Trim()));
                        }
                    }

                    body.Append(new Paragraph(new Run(new Text(""))));
                    body.Append(CreateParagraph("Спасибо за обращение в Clean Motors!", center: true));

                    mainPart.Document.Append(body);
                    mainPart.Document.Save();
                }

                Console.WriteLine($"✅ Чек для заказа {orderId} сгенерирован.");
                return File(mem.ToArray(),
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    $"Receipt_{orderId}.docx");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при создании чека: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера при создании чека.");
            }
        }







        private static Paragraph CreateParagraph(string text, bool bold = false, int size = 24, bool center = false)
        {
            var runProps = new RunProperties();
            runProps.Append(new FontSize { Val = size.ToString() });
            if (bold) runProps.Append(new Bold());

            var run = new Run();
            run.Append(runProps);
            run.Append(new Text(text));

            var paraProps = new ParagraphProperties();
            if (center) paraProps.Append(new Justification { Val = JustificationValues.Center });

            var para = new Paragraph();
            para.Append(paraProps);
            para.Append(run);
            return para;
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CancelOrder(int orderId)
        {
            var uid = HttpContext.Session.GetInt32("UserId");
            var order = _db.Orders.Find(orderId);
            if (order != null && uid != null && order.ClientId == uid.Value)
            {
                _db.Orders.Remove(order);
                _db.SaveChanges();
            }
            return RedirectToAction("Profile");
        }

        // GET: /Home/Book?serviceId=5
        [HttpGet("Book")]
        public IActionResult Book(int serviceId)
        {
            Console.WriteLine($"[GET Book] Session UserId = {HttpContext.Session.GetInt32("UserId")}");

            var s = _db.Services.Find(serviceId);
            if (s == null) return NotFound();

            var vm = new BookViewModel
            {
                ServiceId = s.ServiceId,
                ServiceName = s.Name,
                BusySlots = _db.Orders
                                  .Where(o => o.ServiceId == serviceId)
                                  .Select(o => o.DateTime)
                                  .ToList()
            };
            return View(vm);
        }






        // POST: /Home/Book
        [HttpPost("Book"), ValidateAntiForgeryToken]
        public IActionResult Book(BookViewModel vm)
        {
            var clientId = HttpContext.Session.GetInt32("UserId");
            if (clientId == null)
            {
                return RedirectToAction("SignIn");
            }


            if (vm.SelectedDateTime <= DateTime.Now)
                ModelState.AddModelError(nameof(vm.SelectedDateTime), "Выберите дату в будущем.");

            if (_db.Orders.Any(o =>
                o.ServiceId == vm.ServiceId &&
                o.DateTime == vm.SelectedDateTime)) 
            {
                ModelState.AddModelError(nameof(vm.SelectedDateTime), "Этот слот уже занят.");
            }

            Console.WriteLine($"[POST Book] Получено: {vm.SelectedDateTime}, Сейчас: {DateTime.Now}");
            Console.WriteLine($"[POST Book] ModelState.IsValid = {ModelState.IsValid}");

            foreach (var kvp in ModelState)
            {
                foreach (var error in kvp.Value.Errors)
                {
                    Console.WriteLine($"‼️ Ошибка в поле '{kvp.Key}': {error.ErrorMessage}");
                }
            }


            if (!ModelState.IsValid)
            {
                vm.BusySlots = _db.Orders
                    .Where(o => o.ServiceId == vm.ServiceId)
                    .Select(o => o.DateTime)
                    .ToList();
                return View(vm);
            }

            if (clientId == null) return RedirectToAction("SignIn");

            _db.Orders.Add(new Order
            {
                ClientId = clientId.Value,
                ServiceId = vm.ServiceId,
                DateTime = vm.SelectedDateTime,
                DateCreated = DateTime.Now,
                OrderStatusId = 1,
                MechanicId = null,
                CarWasherId = null
            });
            _db.SaveChanges();

            return RedirectToAction("Profile");
        }

        private bool _TryLogin(string email, string pwd, out int id, out string role)
        {
            var c = _db.Clients.FirstOrDefault(x => x.Email == email && x.Password == pwd);
            if (c != null) { id = c.Id; role = "Client"; return true; }

            var a = _db.Administrators.FirstOrDefault(x => x.Email == email && x.Password == pwd);
            if (a != null) { id = a.Id; role = "Administrator"; return true; }

            var m = _db.Mechanics.FirstOrDefault(x => x.Email == email && x.Password == pwd);
            if (m != null) { id = m.Id; role = "Mechanic"; return true; }

            var w = _db.CarWashers.FirstOrDefault(x => x.Email == email && x.Password == pwd);
            if (w != null) { id = w.Id; role = "CarWasher"; return true; }

            id = 0; role = "";
            return false;
        }


        private IActionResult SignInSuccess(int id, string role)
        {
            HttpContext.Session.SetInt32("UserId", id);
            HttpContext.Session.SetString("UserRole", role);
            return RedirectToAction("Profile");
        }

        private void MapUserFields(dynamic src, ProfileViewModel dst)
        {
            dst.FirstName = src.FirstName;
            dst.LastName = src.LastName;
            dst.Email = src.Email;
            dst.Phone = src.Phone ?? "";
            dst.Car = src.Car ?? "";
            dst.License = src.License ?? "";
            dst.AvatarPath = src.AvatarPath;
            dst.Role = HttpContext.Session.GetString("UserRole")!;
        }
    }
}

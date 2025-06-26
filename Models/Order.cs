
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Kurs_HTML.Models
{
    public class Order
{
    public int OrderId { get; set; }
    public int ClientId { get; set; }
    public Client Client   { get; set; } = null!;
    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public int? MechanicId { get; set; }
    public Mechanic? Mechanic { get; set; }

    public int? CarWasherId { get; set; }
    public CarWasher? CarWasher { get; set; }

    public int OrderStatusId { get; set; }
    public OrderStatus OrderStatus { get; set; } = null!;
    public DateTime DateTime { get; set; }
    public DateTime DateCreated { get; set; }
    public bool IsPaid { get; set; } = false;

    public virtual WorkReport? WorkReport { get; set; }

    public bool IsReceiptDownloaded { get; set; }
    public bool IsDeleted { get; set; } = false;

}

}


namespace Sample.Domain
{
    using System;
    using System.ComponentModel.DataAnnotations;


    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }

        public string OrderNumber { get; set; }

        public OrderStatus Status { get; set; }
    }
}
namespace Sample.Api
{
    using System;
    using System.ComponentModel.DataAnnotations;


    public record SubmitOrderModel
    {
        [Required]
        public Guid OrderId { get; init; }

        [Required]
        [MinLength(6)]
        public string OrderNumber { get; init; }
    }
}
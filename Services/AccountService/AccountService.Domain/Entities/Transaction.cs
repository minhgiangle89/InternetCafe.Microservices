﻿using InternetCafe.Common.Entities;
using InternetCafe.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int? UserId { get; set; }
        public int? SessionId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }

        public Account Account { get; set; } = null!;
    }
}

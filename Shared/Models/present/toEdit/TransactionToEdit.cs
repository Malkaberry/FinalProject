﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programmin2_classroom.Shared.Models.present.toEdit
{
    public class TransactionToEdit
    {
        public int id { get; set; }
        public bool transType { get; set; }
        public double transValue { get; set; }
        public string valueType { get; set; }
        public DateTime transDate { get; set; }
        public string? description { get; set; }
        public bool fixedMonthly { get; set; }
        public int? tagID { get; set; }
        public int? parentTransID { get; set; }
        public string transTitle { get; set; }
    }
}

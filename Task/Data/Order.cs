﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Data
{
	public class Order : IComparable
	{
		public int OrderID { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal Total { get; set; }
		public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            var otherOrder = obj as Order;
            return OrderID.CompareTo(otherOrder.OrderID);
        }
	}
}

// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SampleSupport;
using Task.Data;

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

        [Category("Agregate Operators")]
		[Title("Sum - Task 1")]
		[Description("Выдайте список всех клиентов, чей суммарный оборот (сумма всех заказов) превосходит некоторую величину X. " +
                     "Продемонстрируйте выполнение запроса с различными X (подумайте, можно ли обойтись без копирования запроса несколько раз)")]
        public void Linq1()
        {
            int x = 6520;
            List<Customer> customers = dataSource.Customers
                .Where(cust => cust.Orders.Sum(o => o.Total) > x);
            foreach (var customer in customers)
            {
                Console.WriteLine($"{customer.CompanyName} {customer.Orders.Sum(t => t.Total)}");
            }
		}        
        
        [Category("Restriction Operators")]
		[Title("SelectMany - Task 1")]
		[Description("Для каждого клиента составьте список поставщиков, находящихся в той же стране и том же городе. Сделайте задания с использованием группировки и без.")]
        public void Linq2()
        {
            var suppliersAndCustomers = dataSource.Suppliers.SelectMany(
                s => dataSource.Customers.Where(c => s.Country == c.Country && s.City == c.City),
                (s, c) => new {Customer = c, Supplier = s});
            foreach (var user in suppliersAndCustomers)
            {
                Console.WriteLine($"Customer:{user.Customer.CompanyName} - Supplier:{user.Supplier.SupplierName}");
            }
		}

        [Category("Restriction Operators")]
		[Title("Where - Task 1")]
		[Description("Найдите всех клиентов, у которых были заказы, превосходящие по сумме величину X")]
        public void Linq3()
        {
            int x = 6520;
			IEnumerable<Customer> customers = dataSource.Customers.Select(c => c).Where(c => c.Orders.Any(o => o.Total > x));
            foreach (var user in customers)
            {
                Console.WriteLine($"Customer:{user.CompanyName}");
            }
		}

        [Category("Restriction Operators")]
		[Title("SelectMany - Task 2")]
		[Description("Выдайте список клиентов с указанием, начиная с какого месяца какого года они стали клиентами (принять за таковые месяц и год самого первого заказа)")]
        public void Linq4()
        {
			var customers = dataSource.Customers.SelectMany(c=> c.Orders, 
                (c, o) => new
                {
                    Customer = c.CompanyName,
					StartDate = c.Orders.GroupBy(x => x.OrderDate).First()
                });
            foreach (var customer in customers)
            {
                Console.WriteLine($"Customer:{customer.Customer} - {customer.StartDate}");
            }
		}

        [Category("Ordering Operators")]
		[Title("Order - Task 1")]
		[Description("Выдайте список клиентов с указанием, начиная с какого месяца какого года они стали клиентами (принять за таковые месяц и год самого первого заказа)" +
					 "выдайте список отсортированным по году, месяцу, оборотам клиента (от максимального к минимальному) и имени клиента")]
        public void Linq5()
        {
            var customers = dataSource.Customers.SelectMany(c => c.Orders,
                    (c, o) => new
                    {
                        Customer = c,
                        StartYear = c.Orders.GroupBy(x => x.OrderDate).First().First().OrderDate.Year,
                        StartMonth = c.Orders.GroupBy(x => x.OrderDate).First().First().OrderDate.Month,
                        StartDay = c.Orders.GroupBy(x => x.OrderDate).First().First().OrderDate.Day,
                        TotalSum = c.Orders.Sum(x => x.Total)
                    })
                .OrderByDescending(o => o.StartYear)
                .ThenByDescending(o => o.StartMonth)
                .ThenByDescending(o => o.StartDay)
                .ThenByDescending(o => o.Customer.Orders.Sum(x => x.Total)).Distinct();
            foreach (var customer in customers)
            {
                Console.WriteLine($"Customer:{customer.Customer.CompanyName} - {customer.StartYear}/{customer.StartMonth}/{customer.StartDay} - {customer.TotalSum}");
            }
		}

        [Category("Restriction Operators")]
		[Title("Where - Task 2")]
		[Description("Укажите всех клиентов, у которых указан нецифровой почтовый код или не заполнен регион " +
                     "или в телефоне не указан код оператора (для простоты считаем, что это равнозначно «нет круглых скобочек в начале»).")]
        public void Linq6()
        {
            IEnumerable<Customer> customers = dataSource.Customers.Where(c => string.IsNullOrEmpty(c.Region) 
                                                               || c.PostalCode != null && c.PostalCode.Any(x => !char.IsDigit(x))
                                                               || !c.Phone.StartsWith("(") || !c.Phone.StartsWith(")"));
            foreach (var customer in customers)
            {
                Console.WriteLine($"{customer.Region} - region, {customer.PostalCode} - postal code, {customer.Phone} - phone");
            }
		}

        [Category("Grouping Operators")]
		[Title("Group By - Task 1")]
		[Description("Сгруппируйте все продукты по категориям, внутри – по наличию на складе, внутри последней группы отсортируйте по стоимости")]
        public void Linq7()
        {
            var categories = dataSource.Products.GroupBy(p => p.Category)
                .Select(x => new
                {
                    Category = x.Key,
                    Products = x.Select(m => m.UnitsInStock).GroupBy(m => m)
                        .Select(price => new
                        {
                            NumberInStock = price,
                            Prices = x.Select(n => n.UnitPrice).GroupBy(n => n)
                        })
                });
            foreach (var category in categories)
            {
                Console.WriteLine($"category - {category.Category}");
                foreach (var instock in category.Products)
                {
                    Console.WriteLine($"\tNumber in stock - {instock.NumberInStock.Key}");
                    foreach (var price in instock.Prices)
                    {
                        Console.WriteLine($"\t\tprice - {price.Key}");
                    }
                }
            }
        }

        [Category("Grouping Operators")]
		[Title("Group By - Task 2")]
		[Description("Сгруппируйте товары по группам «дешевые», «средняя цена», «дорогие». Границы каждой группы задайте сами")]
        public void Linq8()
        {
            IEnumerable<IGrouping<string, Product>> groups = dataSource.Products.GroupBy(x => x.UnitPrice <= 20 ? "Cheap"
                : x.UnitPrice > 20 && x.UnitPrice <= 60 ? "Middle" : "High");
            foreach (var group in groups)
            {
                Console.WriteLine($"{group.Key}");
                foreach (var product in group)
                {
                    Console.WriteLine($"\t{product.ProductName} - {product.UnitPrice}");
                }
            }
        }

        [Category("Grouping Operators")]
		[Title("Group By - Task 3")]
		[Description("Рассчитайте среднюю прибыльность каждого города (среднюю сумму заказа по всем клиентам из данного города) " +
                     "и среднюю интенсивность (среднее количество заказов, приходящееся на клиента из каждого города)")]
        public void Linq9()
        {
            var cities = dataSource.Customers.GroupBy(c => c.City,
                (key, group) => new
                {
                    Customer = key,
                    AverageIncome = group.Average(x => x.Orders.Sum(o => o.Total)),
                    AveragIntensity = group.Average(x => x.Orders.Length)
                });
            foreach (var city in cities)
            {
                Console.WriteLine($"{city.Customer} income - {city.AverageIncome:##.###} intensity - {city.AveragIntensity:##.###}");
            }
        }

        [Category("Grouping Operators")]
		[Title("Group By - Task 4")]
		[Description("Сделайте среднегодовую статистику активности клиентов по месяцам (без учета года), " +
                     "статистику по годам, по годам и месяцам (т.е. когда один месяц в разные годы имеет своё значение).")]
        public void Linq10()
        {
            var customersStatistics = dataSource.Customers.Select(customer => new
            {
                customer.CompanyName,
                MonthStatistics = customer.Orders.GroupBy(o => o.OrderDate.Month)
                    .Select(x => new
                    {
                        Month = x.Key, 
                        OrdersCount = x.Count()
                    }),
                YearStatistics = customer.Orders.GroupBy(o => o.OrderDate.Year)
                    .Select(x => new
                    {
                        Year = x.Key, 
                        OrdersCount = x.Count()
                    }),
                YearMonthStatistics = customer.Orders.GroupBy(o => new { year = o.OrderDate.Year, month = o.OrderDate.Month })
                    .Select(x => new
                    {
                        x.Key.month,
                        x.Key.year, 
                        OrdersCount = x.Count()
                    })
            });

            foreach (var statistic in customersStatistics)
            {
                Console.WriteLine($"{statistic.CompanyName}");
                foreach (var monthStatistic in statistic.MonthStatistics)
                {
                    Console.WriteLine($"\tMonth {monthStatistic.Month} - {monthStatistic.OrdersCount}");
                }
                foreach (var yearStatistics in statistic.YearStatistics)
                {
                    Console.WriteLine($"\tYear {yearStatistics.Year} - {yearStatistics.OrdersCount}");
                }
                foreach (var monthStatistic in statistic.YearMonthStatistics)
                {
                    Console.WriteLine($"\t{monthStatistic.year}/{monthStatistic.month} - {monthStatistic.OrdersCount}");
                }
            }
        }
    }
}

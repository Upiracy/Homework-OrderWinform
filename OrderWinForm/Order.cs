using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

namespace OrderServer
{
    [Serializable]
    public class Order : IComparable<Order>
    {
        public ulong id;
        public DateTime time;
        public string customer;
        public List<OrderItem> items = new List<OrderItem>();
        public decimal price;

        public override int GetHashCode()
        {
            return (int)(id % int.MaxValue);
        }
        public override string ToString()
        {
            string str = id + " ";
            foreach (OrderItem item in items)
            {
                str += item.ToString() + " ";
            }
            str += customer + " $" + price + " " + time.ToShortDateString();
            return str;
        }

        public Order() { }
        public Order(ulong id, DateTime date, string customer, decimal price)
        {
            this.id = id;
            this.time = date;
            this.customer = customer;
            this.price = price;
        }

        public static Order Parse(string str)
        {
            Order order = new Order();
            string[] statement = str.Split(' ');
            order.id = ulong.Parse(statement[0]);
            order.customer = statement[statement.Length - 3];
            order.price = long.Parse(statement[statement.Length - 2]);
            order.time = DateTime.Parse(statement[statement.Length - 1]);
            for (int i = 1; i < statement.Length - 3; i++)
            {
                OrderItem item = OrderItem.Parse(statement[i]);
                for (int j = 0; j < order.items.Count; j++)
                {
                    if (item.Equals(order.items[j]))
                    {
                        item.number += order.items[j].number;
                        order.items.RemoveAt(j);
                    }
                }
                order.items.Add(item);
            }
            return order;
        }
        public bool SelectGood(string[] goodName)
        {
            foreach (OrderItem item in items)
            {
                foreach (string name in goodName)
                {
                    if (item.good.ToLower() == name) return true;
                }
            }
            return false;
        }
        public bool SelectProducer(string[] producer)
        {
            foreach (OrderItem item in items)
            {
                foreach (string name in producer)
                {
                    if (item.producer.ToLower() == name) return true;
                }
            }
            return false;
        }
        public bool SelectCustomer(string[] customers)
        {
            foreach (string name in customers)
            {
                if (customer.ToLower() == name) return true;
            }
            return false;
        }
        public bool SelectID(long[] id)
        {
            foreach (ulong l in id)
            {
                if (this.id == l) return true;
            }
            return false;
        }
        public bool SelectPrice(decimal[] price)
        {
            foreach (decimal l in price)
            {
                if (this.price == l) return true;
            }
            return false;
        }
        public bool SelectDate(DateTime[] date)
        {
            foreach (DateTime l in date)
            {
                if (this.time == l) return true;
            }
            return false;
        }
        public int CompareTo(Order other)
        {
            int c = id.CompareTo(other.id);
            if (c == 0) return price.CompareTo(other.price);
            return c;
        }
    }

    [Serializable]
    public struct OrderItem
    {
        public string good;
        public long number;
        public string producer;

        public OrderItem(string good, long number, string producer) { this.good = good; this.number = number; this.producer = producer; }
        public override string ToString()
        {
            return good + "(" + producer + ")" + "*" + number;
        }
        public static OrderItem Parse(string str)
        {
            OrderItem item;
            item.good = str.Substring(0, str.IndexOf('('));
            item.producer = str.Substring(str.IndexOf('(') + 1, str.IndexOf(')') - str.IndexOf('(') - 1);
            if (!str.Contains('*')) item.number = 1;
            else item.number = long.Parse(str.Substring(str.IndexOf('*') + 1));
            return item;
        }
        public override bool Equals(object obj)
        {
            try
            {
                OrderItem item = (OrderItem)obj;
                return good == item.good && producer == item.producer;
            }
            catch
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [Serializable]
    public class OrderTable : IEnumerable<Order>
    {
        public List<Order> orders = new List<Order>();

        public OrderTable(List<Order> orders)
        {
            this.orders = orders;
        }

        public Order this[ulong id]
        {
            get
            {
                foreach (Order order in orders)
                {
                    if (order.id == id)
                    {
                        return order;
                    }
                }
                return null;
            }
            set
            {
                for (int i = 0; i < orders.Count; i++)
                {
                    if (orders[i].id == id)
                    {
                        orders[i] = value;
                    }
                }
            }
        }
        public bool AddOrder(Order order)
        {
            if (this[order.id] != null) return false;
            this.orders.Add(order);
            return true;
        }
        public List<Order> DeleteOrder(List<Order> orders)
        {
            List<Order> errors = new List<Order>();
            foreach (Order order in orders)
            {
                if (this[order.id] == null) errors.Add(order);
                else this.orders.Remove(this[order.id]);
            }
            return errors;
        }

        public void WriteTable(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, orders);
                fs.Close();
                fs.Dispose();
            }
        }
        public void ReadTable(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    orders = bf.Deserialize(fs) as List<Order>;
                    fs.Close();
                    fs.Dispose();
                }
            }
            catch (FileNotFoundException)
            {
                //Console.WriteLine("File Not Found!");
            }
            catch
            {
                //Console.WriteLine("Read Error!");
            }
        }

        public void Sort()
        {
            orders.Sort();
        }

        public override string ToString()
        {
            string str = "";
            foreach (Order order in orders)
            {
                str += order + ";\n";
            }
            return str;
        }

        public IEnumerator<Order> GetEnumerator()
        {
            foreach (Order o in orders)
                yield return o;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (Order o in orders)
                yield return o;
        }
    }

    public class OrderService
    {
        OrderTable table = new OrderTable(new List<Order>());

        void ShowOrigin()
        {
            Console.WriteLine(table);
        }
        void AddProgram()
        {
            Console.WriteLine("Format: ID GOOD(PRODUCER)*NUMBER GOOD(PRODUCER)*NUMBER ... CUSTOMER PRICE DATE, type \"q\" to quit");
            for (; ; )
            {
                Console.Write("ADD：");
                string line = Console.ReadLine().ToLower();
                if (line == "e" || line == "end" || line == "ends" || line == "q" || line == "quit")
                {
                    return;
                }
                else
                {
                    try
                    {
                        if (!table.AddOrder(Order.Parse(line)))
                        {
                            Console.WriteLine("The ID inputed has occured.");
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Parse Error. Format: ID GOOD(PRODUCER)*NUMBER GOOD(PRODUCER)*NUMBER ... CUSTOMER PRICE DATE");
                    }
                }
            }
        }
        void SelectProgram()
        {
            List<Order> list = new List<Order>();
            foreach (Order order in table.orders)
            {
                list.Add(order);
            }
            Console.WriteLine("Format: ID/GOOD/CUSTOMER/PRODUCER/PRICE >/=/< value1 value2 value3,  type \"q\" to quit, type\"d\" to delete");
            for (; ; )
            {
                IEnumerable<Order> temp = new List<Order>();
                Console.Write("SELECT：");
                string line = Console.ReadLine().ToLower();
                if (line == "e" || line == "end" || line == "ends" || line == "q" || line == "quit")
                {
                    return;
                }
                else if (line == "d" || line == "delete")
                {
                    table.DeleteOrder(list);
                    ShowOrigin();
                }
                else
                {
                    int operation = 0;
                    string[] statement = line.Split(' ');
                    if (statement[1].ToLower() == "=" || statement[1].ToLower() == "==") operation = 0;
                    else if (statement[1].ToLower() == "<") operation = 1;
                    else if (statement[1].ToLower() == ">") operation = 2;
                    else Console.WriteLine("Illegal operation;");
                    if (statement[0].ToLower() == "id")
                    {
                        if (operation == 1)
                        {
                            temp = (from m in list where m.id < ulong.Parse(statement[2]) orderby m.price select m);
                            list = temp.ToList();
                            list.Sort();
                        }
                        else if (operation == 2)
                        {
                            temp = (from m in list where m.id > ulong.Parse(statement[2]) orderby m.price select m);
                            list = temp.ToList();
                            list.Sort();
                        }
                        else
                        {
                            long[] id = new long[statement.Length - 2];
                            for (int i = 2; i < statement.Length; i++) if (statement[i] != "") id[i - 2] = long.Parse(statement[i]);
                            temp = from m in list where m.SelectID(id) orderby m.price select m;
                            list = temp.ToList();
                            list.Sort();
                        }
                    }
                    else if (statement[0].ToLower() == "good")
                    {
                        if (operation != 0) Console.WriteLine("Illegal statement.");
                        else
                        {
                            temp = (from m in list where m.SelectGood(statement) orderby m.price select m);
                            list = temp.ToList();
                            list.Sort();
                        }
                    }
                    else if (statement[0].ToLower() == "customer")
                    {
                        if (operation != 0) Console.WriteLine("Illegal statement.");
                        else
                        {
                            temp = (from m in list where m.SelectCustomer(statement) orderby m.price select m);
                            list = temp.ToList();
                            list.Sort();
                        }
                    }
                    else if (statement[0].ToLower() == "producer")
                    {
                        if (operation != 0) Console.WriteLine("Illegal statement.");
                        else
                        {
                            temp = (from m in list where m.SelectProducer(statement) orderby m.price select m);
                            list = temp.ToList();
                            list.Sort();
                        }
                    }
                    else if (statement[0].ToLower() == "price")
                    {
                        if (operation == 1)
                        {
                            temp = (from m in list where m.price < long.Parse(statement[2]) orderby m.price select m);
                            list = temp.ToList();
                            list.Sort();
                        }
                        else if (operation == 2)
                        {
                            temp = (from m in list where m.price > long.Parse(statement[2]) orderby m.price select m);
                            list = temp.ToList();
                            list.Sort();
                        }
                        else
                        {
                            decimal[] price = new decimal[statement.Length - 2];
                            for (int i = 2; i < statement.Length; i++) price[i - 2] = decimal.Parse(statement[i]);
                            temp = (from m in list where m.SelectPrice(price) orderby m.price select m);
                            list = temp.ToList();
                            list.Sort();
                        }
                    }
                    else Console.WriteLine("Illegal property name;");
                    if (list.Count == 0) Console.WriteLine("NULL");
                    else foreach (Order order in list) Console.WriteLine(order);
                }
            }
        }
        void UpdateProgram()
        {
            Console.Write("Update:");
            string line = Console.ReadLine();
            try
            {
                string[] statement = line.Split(' ');
                if (table[ulong.Parse(statement[0])] != null)
                    table[ulong.Parse(statement[0])] = Order.Parse(line);
                else Console.WriteLine("Can't find the id");
            }
            catch
            {
                Console.WriteLine("Parse Error.");
            }
        }
        void ReadProgram()
        {
            Console.Write("Path:");
            string path = Console.ReadLine();
            table.ReadTable(path);
            Console.WriteLine(table);
        }
        void WriteProgram()
        {
            try
            {
                Console.Write("Path:");
                string path = Console.ReadLine();
                table.WriteTable(path);
                Console.WriteLine("Succesfully Saved");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found!");
            }
        }

        public void Excute()
        {
            for (; ; )
            {
                try
                {
                    string line = Console.ReadLine().ToLower();
                    if (line == "h" || line == "help" || line == "helps")
                    {
                        Console.WriteLine("H: help, T:show table, A: add, S: select, C: clear, U: update, R: read, W: write");
                    }
                    else if (line == "t" || line == "table" || line == "show table")
                    {
                        ShowOrigin();
                    }
                    else if (line == "a" || line == "add")
                    {
                        AddProgram();
                        ShowOrigin();
                    }
                    else if (line == "s" || line == "select")
                    {
                        SelectProgram();
                    }
                    else if (line == "c" || line == "clear")
                    {
                        table = new OrderTable(new List<Order>());
                        Console.WriteLine("Table has been cleared.");
                    }
                    else if (line == "u" || line == "update")
                    {
                        UpdateProgram();
                    }
                    else if (line == "r" || line == "read")
                    {
                        ReadProgram();
                    }
                    else if (line == "w" || line == "write")
                    {
                        WriteProgram();
                    }
                    else
                    {
                        Console.WriteLine("Null command! Type \"H\" to show help list.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }
    }
}

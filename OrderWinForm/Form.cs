using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using OrderServer;

namespace OrderWinForm
{
    public partial class OrderServer : Form
    {
        OrderEditor orderEditor = new OrderEditor();
        OrderSelector selector = new OrderSelector();

        public static Order focus = null;

        List<Order> table = new List<Order>();

        public OrderServer()
        {
            InitializeComponent();
        }

        private void CreateTable_Click(object sender, EventArgs e)
        {
            bindingSource.DataSource = null;
            table.Clear();
            Table_Refresh(table);
        }

        private void AddRow_Click(object sender, EventArgs e)
        {
            focus = new Order();
            focus.id = FindMinEmpty();
            focus.customer = "Customer";
            focus.price = 0;
            focus.time = DateTime.Now;
            focus.items = new List<OrderItem>();
            //focus.items.Add(new OrderItem("",1,""));
            orderEditor.LoadOrder(focus);          
            orderEditor.ShowDialog();
            if (orderEditor.DialogResult == DialogResult.Cancel) return; 
            table.Add(focus);
            Table_Refresh(table);
        }

        void Table_Sort()
        {
            table.Sort();
        }
        void Table_Refresh(List<Order> table)
        {
            dataGridView.DataSource = bindingSource;
            bindingSource.DataSource = new BindingList<Order>(table);
            for(int i = 0; i < table.Count; i++)
            {
                Table_Refresh(table, i);
            }
        }
        void Table_Refresh(List<Order> table, int row)
        {
            dataGridView.Rows[row].Cells[0].Value = table[row].id;
            dataGridView.Rows[row].Cells[1].Value = table[row].customer;
            dataGridView.Rows[row].Cells[2].Value = table[row].price;
            dataGridView.Rows[row].Cells[3].Value = table[row].time.Date.ToShortDateString();
            if (table[row].items.Count == 1) dataGridView.Rows[row].Cells[4].Value = table[row].items[0].ToString();
            else if (table[row].items.Count > 1) dataGridView.Rows[row].Cells[4].Value = table[row].items[0].ToString() + "...";
            else dataGridView.Rows[row].Cells[4].Value = "None";
        }

        public ulong FindMinEmpty()
        {
            List<Order> n = new List<Order>(table);
            n.Sort();
            ulong id = 1;
            foreach(Order o in n)
            {
                if (id < o.id) return id;
                else if (id == o.id) id++;
                else return id;
            }
            return id;
        }

        public bool HasID(ulong id)
        {
            if (id == 0) return true;
            foreach(Order o in table)
            {
                if (o.id == id) return true;
            }
            return false;
        }
        public Order Find(ulong id)
        {
            foreach (Order o in table)
            {
                if (o.id == id) return o;
            }
            return null;
        }

        private void SortTable_Click(object sender, EventArgs e)
        {
            Table_Sort();
            Table_Refresh(table);
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if(dataGridView.Columns[e.ColumnIndex].Name == "Details")
            {
                if (e.RowIndex >= table.Count || e.RowIndex < 0) return;
                focus = table[e.RowIndex];
                orderEditor.LoadOrder(focus);
                orderEditor.ShowDialog();
                if(orderEditor.DialogResult == DialogResult.OK)
                {
                    Table_Refresh(table);
                }
            }
            if (dataGridView.Columns[e.ColumnIndex].Name == "Delete")
            {
                if (e.RowIndex >= table.Count || e.RowIndex < 0) return;
                table.RemoveAt(e.RowIndex);
                Table_Refresh(table);
            }
        }

        private void LoadTable_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "表格|*.mtb";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.InitialDirectory = "D:\\";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        table = bf.Deserialize(fs) as List<Order>;
                        fs.Close();
                        fs.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                    //Console.WriteLine("Read Error!");
                }
                finally
                {
                    Table_Refresh(table);
                }
            }
        }

        private void SaveTable_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "表格|*.mtb";
            saveFileDialog.CheckPathExists = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(fs, table);
                        fs.Close();
                        fs.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                }
                Table_Refresh(table);
            }
        }

        private void SelectTable_Click(object sender, EventArgs e)
        {
            selector.table = new List<Order>(table);
            selector.Table_Refresh(selector.table);
            if (selector.ShowDialog() == DialogResult.OK)
            {
                if(selector.behaviour == SelectBehave.Delete)
                {
                    foreach (Order order in selector.table)
                    {
                        if (table.Contains(order)) this.table.Remove(order);
                    }
                }
                else if(selector.behaviour == SelectBehave.OverWrite)
                {
                    this.table = selector.table;
                }
                Table_Refresh(table);
            }         
        }

        private void Import_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "表格|*.mtb";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.InitialDirectory = "D:\\";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        List<Order> orders = bf.Deserialize(fs) as List<Order>;
                        fs.Close();
                        fs.Dispose();
                        foreach(Order o in orders)
                        {
                            o.id = FindMinEmpty();
                            table.Add(o);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                }
                finally
                {
                    Table_Refresh(table);
                }
            }
        }
    }
}

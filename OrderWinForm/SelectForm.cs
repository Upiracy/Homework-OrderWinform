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
    public enum SelectBehave { None, Delete, OverWrite}

    public partial class OrderSelector : Form
    {
        public List<Order> table = new List<Order>();
        public SelectBehave behaviour = SelectBehave.None;

        public OrderSelector()
        {
            InitializeComponent();
        }

        private void SelectForm_Load(object sender, EventArgs e)
        {

        }

        public void Table_Refresh(List<Order> table)
        {
            dataGridView.Rows.Clear();
            for (int i = 0; i < table.Count; i++)
            {
                dataGridView.Rows.Add();
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

        private void Ins_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView.Columns[e.ColumnIndex].Name == "Delete")
            {
                if (e.RowIndex >= table.Count || e.RowIndex < 0) return;
                table.RemoveAt(e.RowIndex);
                Table_Refresh(table);
            }
        }

        private void RunIns_Click(object sender, EventArgs e)
        {
            string line = Ins.Text.ToLower();
            IEnumerable<Order> temp = new List<Order>();
            int operation = 0;
            string[] statement = line.Split(' ');
            if (statement[1].ToLower() == "=" || statement[1].ToLower() == "==") operation = 0;
            else if (statement[1].ToLower() == "<") operation = 1;
            else if (statement[1].ToLower() == ">") operation = 2;
            else Debug.Assert(false, "Illegal operation;");
            if (statement[0].ToLower() == "id")
            {
                if (operation == 1)
                {
                    temp = (from m in table where m.id < ulong.Parse(statement[2]) orderby m.price select m);
                    table = temp.ToList();
                    table.Sort();
                }
                else if (operation == 2)
                {
                    temp = (from m in table where m.id > ulong.Parse(statement[2]) orderby m.price select m);
                    table = temp.ToList();
                    table.Sort();
                }
                else
                {
                    long[] id = new long[statement.Length - 2];
                    for (int i = 2; i < statement.Length; i++) if (statement[i] != "") id[i - 2] = long.Parse(statement[i]);
                    temp = from m in table where m.SelectID(id) orderby m.price select m;
                    table = temp.ToList();
                    table.Sort();
                }
            }
            else if (statement[0].ToLower() == "good" || statement[0].ToLower() == "item")
            {
                if (operation != 0) Console.WriteLine("Illegal statement.");
                else
                {
                    temp = (from m in table where m.SelectGood(statement) orderby m.price select m);
                    table = temp.ToList();
                    table.Sort();
                }
            }
            else if (statement[0].ToLower() == "customer")
            {
                if (operation != 0) Console.WriteLine("Illegal statement.");
                else
                {
                    temp = (from m in table where m.SelectCustomer(statement) orderby m.price select m);
                    table = temp.ToList();
                    table.Sort();
                }
            }
            else if (statement[0].ToLower() == "producer")
            {
                if (operation != 0) Console.WriteLine("Illegal statement.");
                else
                {
                    temp = (from m in table where m.SelectProducer(statement) orderby m.price select m);
                    table = temp.ToList();
                    table.Sort();
                }
            }
            else if (statement[0].ToLower() == "price")
            {
                if (operation == 1)
                {
                    temp = (from m in table where m.price < long.Parse(statement[2]) orderby m.price select m);
                    table = temp.ToList();
                    table.Sort();
                }
                else if (operation == 2)
                {
                    temp = (from m in table where m.price > long.Parse(statement[2]) orderby m.price select m);
                    table = temp.ToList();
                    table.Sort();
                }
                else
                {
                    decimal[] price = new decimal[statement.Length - 2];
                    for (int i = 2; i < statement.Length; i++) price[i - 2] = decimal.Parse(statement[i]);
                    temp = (from m in table where m.SelectPrice(price) orderby m.price select m);
                    table = temp.ToList();
                    table.Sort();
                }
            }
            else if (statement[0].ToLower() == "date")
            {
                if(operation == 1)
                {
                    temp = (from m in table where m.time < DateTime.Parse(statement[2]) orderby m.price select m);
                    table = temp.ToList();
                    table.Sort();
                }
                else if (operation == 2)
                {
                    temp = (from m in table where m.time > DateTime.Parse(statement[2]) orderby m.price select m);
                    table = temp.ToList();
                    table.Sort();
                }
                else
                {
                    DateTime[] date = new DateTime[statement.Length - 2];
                    for (int i = 2; i < statement.Length; i++) if (statement[i] != "") date[i - 2] = DateTime.Parse(statement[i]);
                    temp = from m in table where m.SelectDate(date) orderby m.price select m;
                    table = temp.ToList();
                    table.Sort();
                }
            }
            else Debug.Assert(false, "Illegal property name;");
            Table_Refresh(table);
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.behaviour = SelectBehave.None;
            Close();
        }

        private void SaveAs_Click(object sender, EventArgs e)
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

        private void Overwrite_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.behaviour = SelectBehave.OverWrite;
            Close();
        }

        private void DeleteFrom_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.behaviour = SelectBehave.Delete;
            Close();
        }
    }
}

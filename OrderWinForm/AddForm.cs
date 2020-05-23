using System;
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
    public partial class OrderEditor : Form
    {
        Order order = new Order();

        public OrderEditor()
        {
            InitializeComponent();
        }

        private void AddForm_Load(object sender, EventArgs e)
        {

        }

        private void OK_Click(object sender, EventArgs e)
        {
            List<OrderItem> items = new List<OrderItem>();
            for(int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                OrderItem item = new OrderItem();
                item.good = dataGridView1.Rows[i].Cells[0].Value.ToString();
                item.producer = dataGridView1.Rows[i].Cells[1].Value.ToString();
                item.number = long.Parse(dataGridView1.Rows[i].Cells[2].Value.ToString());
                items.Add(item);
            }
            order.items = items;
            order.id = ulong.Parse(ID_TextField.Text);
            order.price = decimal.Parse(Price_TextField.Text);
            order.customer = Customer_TextField.Text;
            order.time = dateTimePicker1.Value;
            OrderServer.focus = order;
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void ID_TextField_TextChanged(object sender, EventArgs e)
        {
            try
            {
                order.id = ulong.Parse(ID_TextField.Text);
            }
            catch
            {
                order.id = 0;
                ID_TextField.Text = "";
            }
        }

        private void Customer_TextField_TextChanged(object sender, EventArgs e)
        {
            order.customer = Customer_TextField.Text;
        }

        private void Price_TextField_TextChanged(object sender, EventArgs e)
        {
            try
            {
                order.price = decimal.Parse(Price_TextField.Text);
            }
            catch
            {
                order.price = -1;
                Price_TextField.Text = "";
            }
        }

        public void LoadOrder(Order order)
        {
            this.order = order;
            ID_TextField.Text = order.id.ToString();
            Customer_TextField.Text = order.customer;
            Price_TextField.Text = order.price.ToString();
            Table_Refresh();
        }

        void Table_Refresh()
        {
            dataGridView1.Rows.Clear();
            for (; dataGridView1.Rows.Count <= order.items.Count; dataGridView1.Rows.Add()) ;
            for (int i = 0; i < order.items.Count; i++)
            {
                Table_Refresh(order.items, i);
            }
            //Debug.Assert(dataGridView1.Rows[0].Cells[0].Value.ToString() == "", dataGridView1.Rows[0].Cells[0].Value.ToString() + "D");
        }

        void Table_Refresh(List<OrderItem> table, int row)
        {
            dataGridView1.Rows[row].Cells[0].Value = order.items[row].good;
            dataGridView1.Rows[row].Cells[1].Value = order.items[row].producer;
            dataGridView1.Rows[row].Cells[2].Value = order.items[row].number.ToString();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            order.time = dateTimePicker1.Value;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Delete")
            {
                if (e.RowIndex >= dataGridView1.Columns.Count - 1 || e.RowIndex < 0) return;
                dataGridView1.Rows.RemoveAt(e.RowIndex);
            }
        }
    }
}

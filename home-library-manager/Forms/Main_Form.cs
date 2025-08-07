using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace home_library_manager
{
    public partial class Main_Form : Form
    {
        public Main_Form()
        {
            InitializeComponent();
            InitializeToolTip();
        }

        private void InitializeToolTip()
        {
            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(btnAdd, "Add a new book to your library");
            tooltip.SetToolTip(btnFind, "Browse your library");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Main_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to close?", "Closing program",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            using (Browse_Form form = new Browse_Form())
            {
                form.ShowDialog();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (AddBook_Form form = new AddBook_Form())
            {
                form.ShowDialog();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using home_library_manager.DataAccess;

namespace home_library_manager
{
    public partial class Browse_Form : Form
    {

        private DataTable booksTable;

        public Browse_Form()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void Browse_Form_Load(object sender, EventArgs e)
        {
            LoadAllBooks();
            SetupDataGrid();
        }

        private void InitializeCustomComponents()
        {
            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(picBox, "Right-click on the grid to edit or delete a selected row");

            cbFilter.Items.AddRange(new string[] { "All categories", "Title", "Author", "Genre", "Room" });


            ClearAll();
        }

        private void SetupDataGrid()
        {
            dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGrid.DefaultCellStyle.SelectionBackColor = Color.LightSkyBlue;
            dataGrid.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGrid.Columns["Id"].Visible = false; 
            dataGrid.ScrollBars = ScrollBars.Vertical;
        }

        private void ClearAll()
        {
            rtbFilter.Clear();
            rtbFilter.Focus();
            cbFilter.SelectedIndex = 0;
        }


        private void LoadAllBooks()
        {
            string query = "SELECT * FROM Book";
            booksTable = DatabaseHelper.ExecuteSelect(query);
            dataGrid.DataSource = booksTable;
        }

        private void UpdateRowInGrid(int bookId, string title, string author, string genre, string room)
        {
            foreach (DataRow row in booksTable.Rows)
            {
                if (Convert.ToInt32(row["Id"]) == bookId)
                {
                    row["Title"] = title;
                    row["Author"] = author;
                    row["Genre"] = genre;
                    row["Room"] = room;
                    break;
                }
            }
            booksTable.AcceptChanges();
        }

        private void UpdateDataAfterEdit(int id)
        {
            string query = "SELECT * FROM Book WHERE Id = @Id";
            var parameters = new Dictionary<string, object> { { "@Id", id } };

            DataTable updatedBookTable = DatabaseHelper.ExecuteSelect(query, parameters);

            if (updatedBookTable.Rows.Count > 0)
            {
                var updatedRow = updatedBookTable.Rows[0];
                UpdateRowInGrid(
                    id,
                    updatedRow["Title"].ToString(),
                    updatedRow["Author"].ToString(),
                    updatedRow["Genre"].ToString(),
                    updatedRow["Room"].ToString()
                );
            }

            ClearAll();
        }


        private  void btnShowAll_Click(object sender, EventArgs e)
        {
            LoadAllBooks();
        }

        private void rtbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnFilter_Click(sender, e);
            }
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            string keyword = rtbFilter.Text.Trim();
            string category = cbFilter.SelectedItem.ToString();


            string query;
            var parameters = new Dictionary<string, object>();

            if (category == "All categories")
            {
                query = @"SELECT * FROM Book 
                          WHERE Title LIKE @kw OR Author LIKE @kw OR Genre LIKE @kw OR Room LIKE @kw";
                parameters.Add("@kw", $"%{keyword}%");
            }
            else
            {
                query = $"SELECT * FROM Book WHERE {category} LIKE @kw";
                parameters.Add("@kw", $"%{keyword}%");
            }

            booksTable = DatabaseHelper.ExecuteSelect(query, parameters);
            dataGrid.DataSource = booksTable;

        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = dataGrid.SelectedRows[0];

            int id = Convert.ToInt32(row.Cells["Id"].Value);
            string title = row.Cells["Title"].Value?.ToString() ?? "";
            string author = row.Cells["Author"].Value?.ToString() ?? "";
            string genre = row.Cells["Genre"].Value?.ToString() ?? "";
            string room = row.Cells["Room"].Value?.ToString() ?? "";

            using (Form editForm = new AddBook_Form(id, title, author, genre, room))
            {
                if (editForm.ShowDialog() != DialogResult.Cancel)
                {
                    UpdateDataAfterEdit(id);
                }
            }

            
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
   

            if ( MessageBox.Show("Are you sure you want to delete this book?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning)  == DialogResult.Yes)
            {
                int bookId = Convert.ToInt32(dataGrid.SelectedRows[0].Cells["Id"].Value);

                string deleteQuery = "DELETE FROM Book WHERE Id = @Id";
                var parameters = new Dictionary<string, object> { { "@Id", bookId } };

                int rowsAffected =  DatabaseHelper.ExecuteNonQuery(deleteQuery, parameters);

                if (rowsAffected > 0)
                {

                    MessageBox.Show("Book deleted successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadAllBooks();
                    ClearAll();

                }
                else
                {
                    MessageBox.Show("Deletion failed. Book not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
    }
}

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
    public partial class AddBook_Form : Form
    {

        private int? editingBookId = null;

        private ErrorProvider errorProvider;
        
        public AddBook_Form()
        {
            InitializeComponent();
            InitializeCustomComponents();

            btnAdd.Text = "Add Book";
            titleLabel.Text = "Add a Book to Your Library";
            this.Text = "Add a New Book";

            btnAdd.DialogResult = DialogResult.None;
        }

        public AddBook_Form(int bookId, string title, string author, string genre, string room) : this()
        {
            editingBookId = bookId;

            rtbTitle.Text = title.Trim();
            rtbAuthor.Text = author.Trim();
            rtbGenre.Text = genre.Trim();
            rtbRoom.Text = room.Trim();

            btnAdd.Text = "Update";
            titleLabel.Text = "Edit the Details of this Book";
            this.Text = "Edit Book Details";

            btnAdd.DialogResult = DialogResult.OK;

            this.Shown += (s, e) =>
            {
                rtbTitle.Focus();
                rtbTitle.SelectionStart = rtbTitle.Text.Length;
                rtbAuthor.SelectionStart = rtbAuthor.Text.Length;
                rtbGenre.SelectionStart = rtbGenre.Text.Length;
                rtbRoom.SelectionStart = rtbRoom.Text.Length;
            };
        }

        private void InitializeCustomComponents()
        {
            errorProvider = new ErrorProvider();
        }

        private bool ValidateInputs()
        {
            bool isValid = true;
            errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(rtbTitle.Text))
            {
                errorProvider.SetError(rtbTitle, "Title is required.");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(rtbAuthor.Text))
            {
                errorProvider.SetError(rtbAuthor, "Author is required.");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(rtbGenre.Text))
            {
                errorProvider.SetError(rtbGenre, "Genre is required.");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(rtbRoom.Text))
            {
                errorProvider.SetError(rtbRoom, "Room is required.");
                isValid = false;
            }

            return isValid;
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {

            if (!ValidateInputs())
                return;


            string title = rtbTitle.Text;
            string author = rtbAuthor.Text;
            string genre = rtbGenre.Text;
            string room = rtbRoom.Text;


            string query;
            Dictionary<string, object> parameters;

            if (editingBookId == null)
            {
                query = @"INSERT INTO Book (Title, Author, Genre, Room) 
                          VALUES (@title, @author, @genre, @room)";
                parameters = new Dictionary<string, object>
                {
                    { "@title", title },
                    { "@author", author },
                    { "@genre", genre },
                    { "@room", room }
                };
            }
            else
            {
                query = @"UPDATE Book 
                          SET Title = @title, Author = @author, Genre = @genre, Room = @room 
                          WHERE Id = @id";
                parameters = new Dictionary<string, object>
                {
                    { "@title", title },
                    { "@author", author },
                    { "@genre", genre },
                    { "@room", room },
                    { "@id", editingBookId.Value }
                };
            }

            try
            {
                int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    string message = editingBookId == null ?
                        "Book successfully added to your library!" :
                        "Book updated successfully!";
                    MessageBox.Show(message, "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (editingBookId == null)
                        ClearForm();
                    else
                        this.Close();
                }
                else
                {
                    string message = editingBookId == null ?
                        "Failed to add the book. Try again." :
                        "Failed to update the book. Try again.";
                    MessageBox.Show(message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error:\n" + ex.Message, "SQL Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAdd.Text = "Add Book";
            }
        }


        private void ClearForm()
        {
            rtbTitle.Clear();
            rtbAuthor.Clear();
            rtbGenre.Clear();
            rtbRoom.Clear();

            rtbTitle.Focus();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

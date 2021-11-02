using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Drawing.Printing;

namespace ChapterTen10_2
{
    public partial class frmInventory : Form
    {
        public frmInventory()
        {
            InitializeComponent();
        }

        SqlConnection inventoryConnection;
        SqlCommand inventoryCommand;
        SqlDataAdapter inventoryAdapter;
        DataTable inventoryTable;
        CurrencyManager inventoryManager;

        string myState;
        int myBookmark;
        int pageNumber;

        private void frmInventory_Load(object sender, EventArgs e)
        {
            //connect to sales database
            inventoryConnection = new SqlConnection("Data Source=.\\SQLEXPRESS; AttachDbFilename=" + Application.StartupPath + "SQLInventoryDB.mdf; Integrated Security=True; Connect Timeout=30; User Instance=True");
            inventoryConnection.Open();

            //establish inventory command object
            inventoryCommand = new SqlCommand("SELECT * FROM Inventory ORDER BY Item", inventoryConnection);

            //establish inventory data adapter/ data table
            inventoryAdapter = new SqlDataAdapter();
            inventoryAdapter.SelectCommand = inventoryCommand;
            inventoryTable = new DataTable();
            inventoryAdapter.Fill(inventoryTable);

            //bind controls
            txtItem.DataBindings.Add("Text", inventoryTable, "Item");
            txtLocation.DataBindings.Add("Text", inventoryTable, "Location");
            txtStore.DataBindings.Add("Text", inventoryTable, "Store");
            dtpDatePurchased.DataBindings.Add("Text", inventoryTable, "DatePurchased");
            txtPurchaseCost.DataBindings.Add("Text", inventoryTable, "PurchaseCost");
            txtSerialNumber.DataBindings.Add("Text", inventoryTable, "SerialNumber");
            chkEngraved.DataBindings.Add("Checked", inventoryTable, "Engraved");
            lblPhotoFile.DataBindings.Add("Text", inventoryTable, "PhotoFile");

            // establish currency manager
            inventoryManager = (CurrencyManager)this.BindingContext[inventoryTable];

            ShowPhoto();
            SetState("View");
        }

        private void frmInventory_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myState.Equals("Edit") || myState.Equals("Add"))
            {
                MessageBox.Show("You must finish the current edit before stopping.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            else
            {

                try
                {
                    // save the update Inventory table
                    SqlCommandBuilder inventoryAdapterCommand = new SqlCommandBuilder(inventoryAdapter);
                    inventoryAdapter.Update(inventoryTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Saving Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // close the connection
                inventoryConnection.Close();

                // dispose of the objects
                inventoryCommand.Dispose();
                inventoryAdapter.Dispose();
                inventoryTable.Dispose();
            }
        }
        private void ShowPhoto()
        {
            //display photo
            if (!lblPhotoFile.Text.Equals(""))
            {
                try
                {
                    picItem.Image = Image.FromFile(lblPhotoFile.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Loading Photo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                picItem.Image = null;
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            inventoryManager.Position = 0;
            ShowPhoto();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            inventoryManager.Position--;
            ShowPhoto();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            inventoryManager.Position++;
            ShowPhoto();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            inventoryManager.Position = inventoryManager.Count - 1;
            ShowPhoto();
        }

        private void SetState(string appState)
        {
            myState = appState;
            switch (myState)
            {
                case "View":
                    btnFirst.Enabled = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnLast.Enabled = true;
                    btnEdit.Enabled = true;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnAdd.Enabled = true;
                    txtItem.ReadOnly = true;
                    txtLocation.ReadOnly = true;
                    txtStore.ReadOnly = true;
                    dtpDatePurchased.Enabled = false;
                    txtPurchaseCost.ReadOnly = true;
                    txtSerialNumber.ReadOnly = true;
                    chkEngraved.Enabled = false;
                    btnLoadPhoto.Enabled = false;
                    break;
                default: // "Edit", "Add"
                    btnFirst.Enabled = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnLast.Enabled = false;
                    btnEdit.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnAdd.Enabled = false;
                    txtItem.ReadOnly = false;
                    txtLocation.ReadOnly = false;
                    txtStore.ReadOnly = false;
                    dtpDatePurchased.Enabled = true;
                    txtPurchaseCost.ReadOnly = false;
                    txtSerialNumber.ReadOnly = false;
                    chkEngraved.Enabled = true;
                    btnLoadPhoto.Enabled = true;
                    break;
            }
            txtItem.Focus();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetState("Edit");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtItem.Text.Trim().Equals(""))
            {
                MessageBox.Show("You must enter an item description.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtItem.Focus();
                return;
            }
            string savedItem = txtItem.Text;
            int savedRow;
            inventoryManager.EndCurrentEdit();
            if (myState.Equals("Add"))
            {
                inventoryTable.Rows[inventoryManager.Count - 1]["Engraved"] = chkEngraved.Checked;
                chkEngraved.DataBindings.Add("Checked", inventoryTable, "Engraved");
            }
            inventoryTable.DefaultView.Sort = "Item";
            savedRow = inventoryTable.DefaultView.Find(savedItem);
            inventoryManager.Position = savedRow;
            SetState("View");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            inventoryManager.CancelCurrentEdit();
            if (myState.Equals("Add"))
            {
                inventoryManager.Position = myBookmark;
                chkEngraved.DataBindings.Add("Checked", inventoryTable, "Engraved");
            }
            ShowPhoto();
            SetState("View");
        }

        private void btnLoadPhoto_Click(object sender, EventArgs e)
        {
            try
            {
                if (dglOpen.ShowDialog() == DialogResult.OK)
                {
                    lblPhotoFile.Text = dglOpen.FileName;
                    ShowPhoto();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Opening Photo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            myBookmark = inventoryManager.Position;
            //clear picture
            picItem.Image = null;
            //remove binding from checkbox
            chkEngraved.DataBindings.Clear();
            chkEngraved.Checked = false;
            SetState("Add");
            inventoryManager.AddNew();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this record?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                inventoryManager.RemoveAt(inventoryManager.Position);
                ShowPhoto();
            }
            SetState("View"); 
        }

        private void txtItem_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                txtLocation.Focus();
            }
        }

        private void txtLocation_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                txtStore.Focus();
            }
        }

        private void txtStore_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                if (dtpDatePurchased.Enabled)
                {
                    dtpDatePurchased.Focus();
                }
                else
                {
                    txtPurchaseCost.Focus();
                }
            }
        }

        private void dtpDatePurchased_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                txtPurchaseCost.Focus();
            }
        }

        private void txtPurchaseCost_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || (int)e.KeyChar == 8)
            {
                e.Handled = false;
            }
            else if (e.KeyChar == 13)
            {
                txtSerialNumber.Focus();
                e.Handled = false;
            }
            else if (e.KeyChar == '.')
            {
                if (txtPurchaseCost.Text.IndexOf(".") == -1)
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtSerialNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                if (btnLoadPhoto.Enabled)
                {
                    btnLoadPhoto.Focus();
                }
                else
                {
                    txtItem.Focus();
                }
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            // Declare the document
            PrintDocument inventoryDocument;

            // Create the document and name it
            inventoryDocument = new PrintDocument();
            inventoryDocument.DocumentName = "Home Inventory";

            // Add code handler
            inventoryDocument.PrintPage += new PrintPageEventHandler(this.PrintInventory);

            // Print document in preview control
            pageNumber = 1;
            int savedPosition = inventoryManager.Position;
            dlgPreview.Document = inventoryDocument;
            dlgPreview.ShowDialog();

            // Dispose of document when done printing
            inventoryDocument.Dispose();
            inventoryManager.Position = savedPosition;
            ShowPhoto();
        }

        private void PrintInventory(object sender, PrintPageEventArgs e)
        {
            // move through records, printing each one
            inventoryManager.Position = pageNumber - 1;
            ShowPhoto();

            // print header
            Font myFont = new Font("Arial", 14, FontStyle.Bold);
            int y = e.MarginBounds.Top + 50;
            e.Graphics.DrawString("Home Inventory (" + DateTime.Now.ToShortTimeString() + ") - Page " + pageNumber.ToString(), myFont, Brushes.Black, e.MarginBounds.Left, y);
            y += 2 * Convert.ToInt32(myFont.GetHeight(e.Graphics));

            // print text information
            myFont = new Font("Arial", 12, FontStyle.Bold);
            e.Graphics.DrawString("Item: ", myFont, Brushes.Black, e.MarginBounds.X, y);
            e.Graphics.DrawString(txtItem.Text, myFont, Brushes.Black, e.MarginBounds.X + 150, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            e.Graphics.DrawString("Location: ", myFont, Brushes.Black, e.MarginBounds.X, y);
            e.Graphics.DrawString(txtLocation.Text, myFont, Brushes.Black, e.MarginBounds.X + 150, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            e.Graphics.DrawString("Store: ", myFont, Brushes.Black, e.MarginBounds.X, y);
            e.Graphics.DrawString(txtStore.Text, myFont, Brushes.Black, e.MarginBounds.X + 150, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            e.Graphics.DrawString("Date Purchased: ", myFont, Brushes.Black, e.MarginBounds.X, y);
            e.Graphics.DrawString(dtpDatePurchased.Text, myFont, Brushes.Black, e.MarginBounds.X + 150, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            e.Graphics.DrawString("Purchase Cost: ", myFont, Brushes.Black, e.MarginBounds.X, y);
            e.Graphics.DrawString("$" + String.Format("{0:f2}", txtPurchaseCost.Text), myFont, Brushes.Black, e.MarginBounds.X + 150, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            e.Graphics.DrawString("Serial Number: ", myFont, Brushes.Black, e.MarginBounds.X, y);
            e.Graphics.DrawString(txtSerialNumber.Text, myFont, Brushes.Black, e.MarginBounds.X + 150, y);
            y += 50;

            // print picture (4 inches wide, height based on height/width ratio of image)
            int h = Convert.ToInt32(400 * picItem.Image.Height / picItem.Image.Width);
            e.Graphics.DrawImage(picItem.Image, e.MarginBounds.X, y, 400, h);
            pageNumber++;
            if (pageNumber <= inventoryManager.Count)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
                pageNumber = 1;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

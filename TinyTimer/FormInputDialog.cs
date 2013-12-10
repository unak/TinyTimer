using System;
using System.Windows.Forms;

namespace TinyTimer
{
    public partial class FormInputDialog : Form
    {
        public string Title
        {
            set
            {
                editTitle.Text = value;
            }
            get
            {
                return editTitle.Text;
            }
        }

        public FormInputDialog(string title)
        {
            InitializeComponent();

            this.Title = title;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}

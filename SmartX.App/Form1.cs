using SmartX.App.Forms;

namespace SmartX.App
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using var sensorsForm = new Sensors();
            Hide();

            try
            {
                sensorsForm.ShowDialog(this);
            }
            finally
            {
                Show();
            }
        }

        private void btnLocked_Click(object sender, EventArgs e)
        {
            using var dashboard = new RealTimeDashboard();
            dashboard.ShowDialog(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(  "Network Topology and Mesh Routing is reserved for the final PoE module.", "Network Topology", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}

/*
 Reference List

Samuel, J., 2012. Button disable and enable. Stack Overflow, 23 January. Available at: https://stackoverflow.com/questions/8975569/button-disable-and-enable
 */
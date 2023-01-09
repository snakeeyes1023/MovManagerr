using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Security.Policy;
using System.Windows.Forms;
using M3USync.Infrastructures.Configurations;

namespace MovManagerr.App
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            Program.OnWebServerStatusChanged += Program_OnWebServerStatusChanged;

            // Cache le winform
            this.Load += new EventHandler(Main_OnLoad);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ShowInTaskbar = false;


            notifyIcon.Icon = new Icon("Icons/icon_off.ico", 40, 40);
            notifyIcon.Text = "MovManagerr";
            notifyIcon.DoubleClick += OnNotifyIcon_Click;

            // Création du menu contextuel
            ContextMenuStrip menu = new ContextMenuStrip();
            // Ajout de l'icône dans la barre des tâches
            AddMenuOptions(menu);

            notifyIcon.ContextMenuStrip = menu;

            RestartServer_Click(default, default);
        }

        private void Program_OnWebServerStatusChanged(bool newStatus)
        {
            if (newStatus)
            {
                notifyIcon.Icon = new Icon("Icons/Icon.ico", 40, 40);
            }
            else
            {
                notifyIcon.Icon = new Icon("Icons/Icon_off.ico", 40, 40);
            }
        }
            

        private void OnNotifyIcon_Click(object? sender, EventArgs e)
        {
            if (e is MouseEventArgs mouse && mouse.Button == MouseButtons.Left)
            {
                //show context menu
                OpenInBrowserMenuItem_Click(sender, e);
            }
        }

        private void AddMenuOptions(ContextMenuStrip menu)
        {
            menu.BackColor = Color.White;
            menu.ForeColor = Color.Black;
            
            menu.CreateAwesomeItem("Ouvrir dans le navigateur").Click += new EventHandler(OpenInBrowserMenuItem_Click);
            menu.CreateAwesomeItem("Gérer les configurations").Click += new EventHandler(ManageConfigurationsMenuItem_Click);
            menu.Items.Add(new ToolStripSeparator());
            menu.CreateAwesomeItem("Redémarrer le serveur web").Click += new EventHandler(RestartServer_Click);
            menu.CreateAwesomeItem("Stopper le serveur web").Click += new EventHandler(StopServer_Click);
            menu.CreateAwesomeItem("Démarer le serveur web").Click += new EventHandler(RestartServer_Click);
            menu.Items.Add(new ToolStripSeparator());
            menu.CreateAwesomeItem("Quitter").Click += new EventHandler(ExitMenuItem_Click);
        }

        private void StopServer_Click(object? sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Program.StopWebHost();
            });
        }

        private void RestartServer_Click(object? sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Program.StopWebHost();
                Thread.Sleep(5000);
                Program.StartWebHost();
            });
        }


        private void ManageConfigurationsMenuItem_Click(object? sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Preferences.Instance._PreferenceFolder);
        }

        private void OpenInBrowserMenuItem_Click(object? sender, EventArgs e)
        {
            Process.Start("explorer", "http://127.0.0.1:5000");
        }

        private void Main_OnLoad(object? sender, EventArgs e)
        {
            this.Size = new Size(0, 0);
        }


        private void ExitMenuItem_Click(object? sender, EventArgs e)
        {
            StopServer_Click(sender, e);

            Thread.Sleep(5000);
            
            Application.Exit();
        }
    }

    public static class ContextMenuStripExtensions
    {

        public static ToolStripMenuItem CreateAwesomeItem(this ContextMenuStrip menu, string text)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            item.Padding = new Padding(20, 20, 20, 0);      
            menu.Items.Add(item);

            return item;
        }
    }
}
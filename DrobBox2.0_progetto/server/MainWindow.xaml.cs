using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;

namespace server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            sql t = new sql();
            t.Run();
        }




        public void Run()
        {
            Console.WriteLine("Connessione");
            // Per prima cosa si crea una connessione "conn" 
            // mediante SqlConnection con i dati del server
            // al quale si desidera accedere. Il nome del server è BARIBAL,
            // il database che contiente la tabella è master

            SqlConnection conn = new SqlConnection("Data Source=Table1; Integrated Security=SSPI; Initial Catalog=master");

            Console.WriteLine("Comando");
            // Ora bisogna creare il comando cmd mediante SqlCommand.
            // In questo caso un semplice SELECT * per prelevare
            // tutte le colonne dalla tabella
            SqlCommand cmd = new SqlCommand("SELECT * FROM dropbox", conn);
            try
            {
                // La connessione era solo impostata, ora la si apre
                conn.Open();
                // Si utilizza la classe DataReader per leggere la
                // tabella un record per volta, e via via stamparne
                // il contenuto sulla console
                SqlDataReader myReader = cmd.ExecuteReader();
                RichTextBox r = new RichTextBox();

                // Ad ogni record letto...
                // (perchè in questo caso legge l'intera riga)
                while (myReader.Read())
                {
                    // ... estrae i valori e li stampa a schermo
                    r.AppendText("caio");
                }
                // Chiude il DataReader		
                myReader.Close();
                // Chiude la Connessione
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occured -->> {0}", e);
            }

        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}

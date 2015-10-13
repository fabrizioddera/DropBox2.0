using System;
using System.Data.SqlClient;

public class sql
{
    public static void sql_main()
    {
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
        string source = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\Pc\Documents\GitHub\DropBox2.0\DrobBox2.0_progetto\dropbox.sql;Integrated Security=True";

        SqlConnection conn = new SqlConnection(source);
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
            Console.WriteLine("Code \t Emp. Name \t Emp. Phone");
            Console.WriteLine("-----------------------------------------");
            // Ad ogni record letto...
            // (perchè in questo caso legge l'intera riga)
            while (myReader.Read())
            {
                // ... estrae i valori e li stampa a schermo
                Console.WriteLine("{0}\t{1}\t\t{2}", myReader.GetInt32(0), myReader.GetString(1), myReader.GetString(2));
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
}
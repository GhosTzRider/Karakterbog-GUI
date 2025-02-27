using Google.Protobuf.Compiler;
using Karakterbog_GUI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Karakterbog_GUI
{
    public partial class MainWindow : Window
    {
        private List<Person> personer;
        private string connectionString = "server=127.0.0.1;uid=root;pwd=Odense202123!;database=karakterbog_database";

        public MainWindow()
        {
            InitializeComponent();
            personer = new List<Person>();
            KarakterListe.ItemsSource = personer;
            HentDataFraDatabase();
        }

        private void TilføjKarakter_Click(object sender, RoutedEventArgs e)
        {
            string personNavn = NavnTextBox.Text;
            string fagNavn = FagTextBox.Text;
            if (float.TryParse(KarakterTextBox.Text, out float karakter))
            {
                TilføjDataTilDatabase(personNavn, fagNavn, karakter);
                HentDataFraDatabase();
            }
            else
            {
                MessageBox.Show("Ugyldig karakter. Indtast et tal.");
            }
        }

        private void FjernKarakter_Click(object sender, RoutedEventArgs e)
        {
            Person valgtPerson = KarakterListe.SelectedItem as Person;
            if (valgtPerson != null)
            {
                FjernDataFraDatabase(valgtPerson.Navn);
                HentDataFraDatabase();
            }
        }

        private void HentDataFraDatabase()
        {
            personer.Clear();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Elev_Navn, Elev_Fag, Elev_Karakter FROM karakter_liste";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string navn = reader.GetString("Elev_Navn");
                        string fag = reader.GetString("Elev_Fag");
                        float karakter = reader.GetFloat("Elev_Karakter");

                        Person person = personer.FirstOrDefault(p => p.Navn == navn);
                        if (person == null)
                        {
                            person = new Person(navn);
                            personer.Add(person);
                        }
                        person.TilføjFag(fag, karakter);
                    }
                }
            }
            KarakterListe.Items.Refresh();
            OpdaterStatistik();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && (textBox.Text == "Navn" || textBox.Text == "Fag" || textBox.Text == "Karakter"))
            {
                textBox.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Name == "NavnTextBox") textBox.Text = "Navn";
                if (textBox.Name == "FagTextBox") textBox.Text = "Fag";
                if (textBox.Name == "KarakterTextBox") textBox.Text = "Karakter";
            }
        }

        private void TilføjDataTilDatabase(string navn, string fag, float karakter)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO karakter_liste (Elev_Navn, Elev_Fag, Elev_Karakter) VALUES (@Elev_Navn, @Elev_Fag, @Elev_Karakter)";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Elev_Navn", navn);
                    cmd.Parameters.AddWithValue("@Elev_Fag", fag);
                    cmd.Parameters.AddWithValue("@Elev_Karakter", karakter);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void FjernDataFraDatabase(string navn)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM karakter_liste WHERE Elev_Navn = @Elev_Navn";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Elev_Navn", navn);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void OpdaterStatistik()
        {
            if (personer.Any())
            {
                float gennemsnit = personer.Average(p => p.Gennemsnit);
                float højeste = personer.Max(p => p.KarakterStatistik().højestekarakter);
                float laveste = personer.Min(p => p.KarakterStatistik().lavestekarakter);

                HøjesteText.Text = højeste.ToString();
                LavesteText.Text = laveste.ToString();
                GennemsnitText.Text = gennemsnit.ToString("F2");
                KarakterListe.Items.Refresh();
            }
            else
            {
                HøjesteText.Text = "-";
                LavesteText.Text = "-";
                GennemsnitText.Text = "-";
            }
            {
                
            }
        }
    }
}

public class Person
{
    public string Navn { get; set; }
    public List<Fag> FagListe { get; set; } = new List<Fag>();

    public float Gennemsnit => FagListe.Any() ? FagListe.Average(f => f.Karakter) : 0;
    public string FagNavne => string.Join(", ", FagListe.Select(f => f.Navn));
    public string Karakterer => string.Join(", ", FagListe.Select(f => f.Karakter.ToString()));

    public Person(string navn)
    {
        Navn = navn;
    }

    public void TilføjFag(string fagNavn, float karakter)
    {
        FagListe.Add(new Fag(fagNavn, karakter));
    }

    public (float højestekarakter, float lavestekarakter, float gennemsnitligeKarakter) KarakterStatistik()
    {
        if (FagListe.Any())
        {
            float højeste = FagListe.Max(f => f.Karakter);
            float laveste = FagListe.Min(f => f.Karakter);
            float gennemsnit = FagListe.Average(f => f.Karakter);
            return (højeste, laveste, gennemsnit);
        }
        return (0, 0, 0);
    }
}


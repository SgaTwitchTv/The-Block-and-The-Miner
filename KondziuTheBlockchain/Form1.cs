using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KondziuTheBlockchain
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource cts;
        private Stopwatch stopwatch = new Stopwatch();
        public Form1()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "KondziuTheBlockchain - Proof of Work Mechanism";
            button1.Text = "MINE THIS";
            button1.BackColor = Color.Crimson;
            button1.ForeColor = Color.White;
            button1.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            dataGridView1.BackgroundColor = Color.FromArgb(30, 30, 30);
            dataGridView1.GridColor = Color.FromArgb(60, 60, 60);
            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            dataGridView1.DefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.RoyalBlue;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            textBox3.Text = "1";     // Block ID
            textBox4.Text = "0";     // Nonce
            textBox2.Text = "waiting to mine...";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                ofd.Title = "Select Transactions CSV File";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LoadCsvToGrid(ofd.FileName);
                }
            }
        }

        private void LoadCsvToGrid(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length < 2)
                {
                    throw new Exception("CSV must have header + at least one row");
                }

                // Create DataTable with Transaction_ID
                var dt = new DataTable();
                dt.Columns.Add("Transaction_ID", typeof(int));
                dt.Columns.Add("Date", typeof(string));
                dt.Columns.Add("From", typeof(string));
                dt.Columns.Add("To", typeof(string));
                dt.Columns.Add("Amount", typeof(decimal));

                for (int i = 1; i < lines.Length; i++) // skip header
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length < 4)
                    {
                        continue;
                    }

                    decimal amount = 0m;
                    decimal.TryParse(
                        parts[4].Trim(),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out amount);

                    dt.Rows.Add(
                        int.TryParse(parts[0].Trim(), out int id) ? id : i,   // use CSV ID or fallback to row number
                        parts[1].Trim(),   // Date
                        parts[2].Trim(),   // From
                        parts[3].Trim(),   // To
                        amount             // Amount → now correctly parsed!
                    );
                }

                dataGridView1.DataSource = dt;
                this.Text = $"KondziuTheBlockchain – {dt.Rows.Count} transactions loaded – Ready to mine!";
                MessageBox.Show($"Successfully loaded {dt.Rows.Count} transactions!", "Loaded",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading CSV:\n" + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox3.Text = "1";
            textBox2.Text = "waiting to mine...";
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "STOP MINING")
            {
                cts?.Cancel();
                return;
            }

            // Validation
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Load a CSV file first!", "No data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(textBox5.Text, out int difficulty) || difficulty < 1 || difficulty > 12)
            {
                MessageBox.Show("Difficulty must be 1–12", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //Tu dane z textboxow
            string blockId = textBox3.Text.Trim();
            string previousHash = string.IsNullOrWhiteSpace(textBox2.Text) || textBox2.Text == "waiting to mine..."
                ? new string('0', 64)
                : textBox2.Text;

            // Robimy dane z tabeli do hashowania
            string data = "";
            int txId = 1;
            foreach (DataRow row in ((DataTable)dataGridView1.DataSource).Rows)
            {
                data += $"{txId++}|{row["Date"]}|{row["From"]}|{row["To"]}|{row["Amount"]}\n";
            }

            cts = new CancellationTokenSource();
            uint nonce = 0;
            string target = new string('0', difficulty);

            button1.Text = "STOP MINING";
            button1.BackColor = Color.DarkOrange;

            textBox4.Text = "0";
            textBox2.ForeColor = Color.Gray;
            textBox2.Text = "mining...";
            stopwatch.Restart();

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    nonce++;
                    textBox4.Text = nonce.ToString();

                    // The header to be hashed ( czyli to co hashujemy i szukamy zer )
                    string header = blockId + previousHash + data + nonce.ToString();

                    // DOUBLE SHA-256
                    string hash = Sha256BitLevel.ComputeHash(Sha256BitLevel.ComputeHash(header));

                    // Dashboard
                    textBox2.Text = hash;
                    if (nonce % 500 == 0)
                    {
                        double speed = nonce / stopwatch.Elapsed.TotalSeconds;
                        this.Text = $"KondziuTheBlockchain | {speed:F0} H/s | Nonce: {nonce}";
                    }

                    Application.DoEvents(); // keeps GUI responsive

                    if (hash.StartsWith(target))
                    {
                        textBox2.ForeColor = Color.Lime;
                        textBox2.Font = new Font(textBox2.Font, FontStyle.Bold);
                        this.Text = "KondziuTheBlockchain - BLOCK MINED!";
                        MessageBox.Show(
                            $"BLOCK MINED!\n\n" +
                            $"Block: {blockId}\n" +
                            $"Nonce: {nonce}\n" +
                            $"Hash: {hash}\n" +
                            $"Target: {target}...\n\n" +
                            $"Anyone in the world can verify this block is valid!",
                            "SUCCESS!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Auto-fill previous hash for next block
                        // (optional — remove if you don't want chain)
                        // textBox3.Text = (int.Parse(blockId) + 1).ToString();

                        break;
                    }
                }
            }
            catch (OperationCanceledException) { }

            button1.Text = "MINE THIS BLOCK!";
            button1.BackColor = Color.Crimson;
            stopwatch.Stop();
            this.Text = "KondziuTheBlockchain";
        }
    }
}

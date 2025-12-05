using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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
            this.Text = "KondziuTheBlockchain - Proof of Work Demo";
            button1.Text = "MINE THIS BLOCK!";
            button1.BackColor = Color.Crimson;
            button1.ForeColor = Color.White;
            button1.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox3.Text = "1";
            textBox5.Text = "0";
            textBox1.Text = "Tutaj daj tekst";
            textBox4.Text = "0";
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "STOP MINING")
            {
                cts?.Cancel();
                return;
            }

            // === Read values from your textboxes ===
            string blockId = textBox3.Text.Trim();           // Block ID
            string data = textBox1.Text.Trim();           // Data
            string previousHash = "0000000000000000000000000000000000000000000000000000000000000000"; // genesis
            if (!string.IsNullOrEmpty(textBox2.Text) && textBox2.Text.Length == 64)
                previousHash = textBox2.Text; // use last hash if exists

            if (!int.TryParse(textBox5.Text, out int difficulty) || difficulty < 1 || difficulty > 10)
            {
                MessageBox.Show("Difficulty must be 1–10", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // === Setup mining ===
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

                    // === Real Bitcoin-style block header (simplified but authentic) ===
                    string header = blockId + previousHash + data + nonce.ToString();

                    // DOUBLE SHA-256 — exactly like Bitcoin!
                    string hash = Sha256BitLevel.ComputeHash(
                                    Sha256BitLevel.ComputeHash(header));

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
